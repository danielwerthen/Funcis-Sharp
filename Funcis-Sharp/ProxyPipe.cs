﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FuncisSharp
{
	
	public class ProxyPipe
	{
		public PipeSettings Settings { get; set; }

		private string Uri
		{
			get
			{
				return string.Format("http://{0}:{1}{2}",
					Settings.HostName,
					Settings.Port,
					Path.Combine(Settings.BasePath, "/connect"));
			}
		}

		private HttpWebRequest currentReq;
		private StreamReader res;
		public string Nodes { get; set; }
		public string Id { get; set; }
		private string buffer { get; set; }

		private string GetAuthMessage()
		{
			return string.Format("{{ \"secret\": \"{0}\", \"nodes\": {1}{2} }}",
				Settings.Secret,
				this.Nodes,
				!string.IsNullOrEmpty(this.Id) ? string.Format(", \"id\": \"{0}\"", this.Id) : "");
		}

		public ProxyPipe(string nodes, PipeSettings settings = null)
		{
			Settings = settings != null ? settings : new PipeSettings();
			Nodes = nodes;
		}

		public ProxyPipe(NodeMap<LocalNode> locals, string uri)
		{
			Settings = new PipeSettings();
			var u = new Uri(uri);
			Settings.HostName = u.Host;
			Settings.Port = u.Port;
			Settings.BasePath = u.PathAndQuery;
			Nodes = new JArray(locals.Nodes.Select(node => new JObject(new JProperty("name", node.Name), new JProperty("classes", node.Classes)))).ToString();
		}

		public bool CanSend
		{
			get { return false; }
		}

		private void createReq()
		{
			currentReq = WebRequest.CreateHttp(Uri);
			currentReq.Timeout = Settings.Timeout;
			currentReq.Method = "POST";
			currentReq.SendChunked = true;

			using (var req = new StreamWriter(currentReq.GetRequestStream()))
			{
				req.Write(this.GetAuthMessage() + Settings.Delimiter);
			}

			res = new StreamReader(currentReq.GetResponse().GetResponseStream());
		}

		private async Task<JObject> _receiveAsync()
		{
			while (true)
			{
				try
				{
					if (res == null)
						createReq();
					string str;
					do
					{
						str = await this.ReadUntilAsync(res);
					} while (str == null || str.Length == 0);
					var jo = JObject.Parse(str);
					JToken id;
					if (jo.TryGetValue("id", out id))
					{
						this.Id = id.Value<string>();
						return await _receiveAsync();
					}
					return jo;
				}
				catch (Exception)
				{
					res = null;
					Console.WriteLine("Retrying in 100ms");
					Thread.Sleep(100);
				}
			}
		}

		public JObject Receive()
		{
			var jo = _receiveAsync();
			jo.Wait();
			return jo.Result;
		}

		public async Task<JObject> ReceiveAsync()
		{
			var jo = await _receiveAsync();
			return jo;
		}

		private async Task<char> nextChar(StreamReader sr)
		{
			char[] buf = new char[1];
			await sr.ReadAsync(buf, 0, 1);
			return buf[0];
		}

		public async Task<string> ReadUntilAsync(StreamReader sr)
		{
			if (buffer == null) buffer = "";
			do
			{
				buffer += await nextChar(sr);
			} while (!buffer.EndsWith(Settings.Delimiter));
			var match = Regex.Match(buffer, "^(.+?)" + Settings.Delimiter);
			if (match.Success)
			{
				var cap = match.Groups[1].Value;
				buffer = buffer.Substring(match.Groups[0].Length);
				return cap;
			}
			return null;
		}
	}
}
