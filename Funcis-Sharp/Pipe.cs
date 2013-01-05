using Newtonsoft.Json.Linq;
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
	public class PipeSettings
	{
		public string HostName { get; set; }
		public int Port { get; set; }
		public string BasePath { get; set; }
		public int Timeout { get; set; }
		public string Delimiter { get; set; }
		public string Secret { get; set; }

		public PipeSettings()
		{
			HostName = "Localhost";
			Port = 80;
			BasePath = "";
			Timeout = 30 * 1000;
			Delimiter = "::";
			Secret = "very_secret_key";
		}
	}
	public class Pipe
	{
		private PipeSettings Settings { get; set; }

		public Pipe(string uri)
		{
			Settings = new PipeSettings();
			var u = new Uri(uri);
			Settings.HostName = u.Host;
			Settings.Port = u.Port;
			Settings.BasePath = u.PathAndQuery;
			timer = new Timer((s) => _end(), null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
		}

		HttpWebRequest currentReq;
		StreamWriter req;
		Timer timer;

		private void createReq()
		{
			currentReq = WebRequest.CreateHttp(Uri);
			currentReq.Timeout = Settings.Timeout;
			currentReq.Method = "POST";
			currentReq.SendChunked = true;

			req = new StreamWriter(currentReq.GetRequestStream());
			timer.Change(Settings.Timeout, System.Threading.Timeout.Infinite);
		}

		public async Task Send(JObject data)
		{
			while (true)
			{
				try
				{
					if (req == null)
						createReq();
					await req.WriteAsync(data.ToString());
					break;
				}
				catch (Exception)
				{
					this._end();
					Console.WriteLine("Failed in sending, retrying in 100ms");
					Thread.Sleep(100);
				}
			}
		}

		private void _end()
		{
			if (req == null)
				return;
			req.Dispose();
			req = null;
			if (currentReq == null)
				return;
			var res = new StreamReader(currentReq.GetResponse().GetResponseStream());
			res.Dispose();
		}

		private string Uri
		{
			get
			{
				return string.Format("http://{0}:{1}{2}",
					Settings.HostName,
					Settings.Port,
					Settings.BasePath + "/call");
			}
		}

	}
}
