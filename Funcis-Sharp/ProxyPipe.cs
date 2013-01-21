using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
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

		public ProxyPipe(NodeMap<LocalNode> locals, string uri)
		{
			Settings = new PipeSettings();
			var u = new Uri(uri);
			Settings.HostName = u.Host;
			Settings.Port = u.Port;
			Settings.BasePath = u.PathAndQuery;
			Nodes = new JArray(locals.Nodes.Select(node => new JObject(new JProperty("name", node.Name), new JProperty("classes", node.Classes)))).ToString();
			var listen = Task.Run(() => _listen());
			var parse = Task.Run(() => _parse());
		}

		private string innerBuffer = "";
		private Queue<string> bufferQueue = new Queue<string>();
		private void _listen()
		{
			while (true)
			{
				try
				{
					var webReq = WebRequest.CreateHttp(Uri);
					webReq.Timeout = Settings.Timeout;
					webReq.Method = "POST";
					webReq.SendChunked = true;
					webReq.KeepAlive = false;

					using (var req = new StreamWriter(webReq.GetRequestStream()))
					{
						req.Write(this.GetAuthMessage() + Settings.Delimiter);
						req.Flush();
					}
					using (var response = webReq.GetResponse())
					{
						using (var res = new StreamReader(response.GetResponseStream()))
						{
							while (!res.EndOfStream)
							{
								
								innerBuffer += (char)res.Read();
								if (innerBuffer != null && innerBuffer.EndsWith(Settings.Delimiter))
								{
									var match = Regex.Match(innerBuffer, "^(.+?)" + Settings.Delimiter);
									if (match.Success)
									{
										var cap = match.Groups[1].Value;
										innerBuffer = innerBuffer.Substring(match.Groups[0].Length);
										bufferQueue.Enqueue(cap);
									}
									lock (_parseLock)
										Monitor.Pulse(_parseLock);
								}
							} 
						}
					}
					innerBuffer = null;
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
					buffer = "";
					Thread.Sleep(500);
				}
			}
		}

		readonly object _parseLock = new object();
		Queue<JObject> _datas = new Queue<JObject>();
		private void _parse()
		{
			while (true)
			{
				string str = null;
				lock (_parseLock)
				{
					Monitor.Wait(_parseLock);
					if (bufferQueue.Count == 0)
						continue;
					str = bufferQueue.Dequeue();
				}
				if (string.IsNullOrEmpty(str))
					continue;
				try
				{
					var jo = JObject.Parse(str);
					JToken id;
					if (jo.TryGetValue("id", out id))
					{
						this.Id = id.Value<string>();
					}
					else
					{
						lock (_readLock)
						{
							_datas.Enqueue(jo);
							Monitor.Pulse(_readLock);
						}
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(str);
					Console.WriteLine(e.ToString());
				}
			}
		}

		readonly object _readLock = new object();
		public async Task<JObject> ReceiveAsync()
		{
			return await Task.Run(() =>
			{
				lock (_readLock)
				{
					Monitor.Wait(_readLock);
					return _datas.Dequeue();
				}
			});
			//var jo = await _receiveAsync();
			//return jo;
		}

		public bool CanSend
		{
			get { return false; }
		}

		/*private CancellationTokenSource returnCancel;
		private void createReq()
		{
			currentReq = WebRequest.CreateHttp(Uri);
			currentReq.ReadWriteTimeout = Settings.Timeout;
			currentReq.Method = "POST";
			currentReq.SendChunked = true;

			using (var req = new StreamWriter(currentReq.GetRequestStream()))
			{
				req.Write(this.GetAuthMessage() + Settings.Delimiter);
			}
			res = new StreamReader(currentReq.GetResponse().GetResponseStream());
			returnCancel = new CancellationTokenSource();
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
						str = await Task.Run<string>(() =>
						{
							return this.ReadUntilAsync(res, returnCancel.Token);
						});
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

		public async Task<JObject> ReceiveAsync2()
		{
			var jo = await _receiveAsync();
			return jo;
		}

		private char? nextChar(StreamReader sr, CancellationToken token)
		{
			char[] buf = new char[1];
			Task t = null;
			try
			{
				t = sr.ReadAsync(buf, 0, 1);
				t.Wait(token);
				if (token.IsCancellationRequested)
					Console.WriteLine("Cancelled");
				if (!t.IsCompleted || t.IsCanceled || token.IsCancellationRequested)
					return null;
			}
			catch (Exception e)
			{
				if (!token.IsCancellationRequested)
					throw e;
				return null;
			}
			return buf[0];
		}

		public string ReadUntilAsync(StreamReader sr, CancellationToken token)
		{
			if (buffer == null) buffer = "";
			do
			{
				var nc = nextChar(sr, token);
				if (nc.HasValue)
					buffer += nc.Value;
				else
					return null;
			} while (!buffer.EndsWith(Settings.Delimiter));
			var match = Regex.Match(buffer, "^(.+?)" + Settings.Delimiter);
			if (match.Success)
			{
				var cap = match.Groups[1].Value;
				buffer = buffer.Substring(match.Groups[0].Length);
				return cap;
			}
			return null;
		}*/
	}
}
