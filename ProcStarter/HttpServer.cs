using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProcStarter
{
	public class HttpServer : IDisposable
	{
		HttpListener listener;
		public HttpServer(int Port)
		{
			listener = new HttpListener();
			listener.Prefixes.Add("http://localhost:" + Port + "/");
			listener.Start();
		}

		public async Task<string> Receive()
		{
			var context = await listener.GetContextAsync();
			string url = null;
			using (StreamReader sr = new StreamReader(context.Request.InputStream))
			{
				var data = await sr.ReadToEndAsync();
				var jo = JObject.Parse(data);
				JToken token;
				if (jo.TryGetValue("url", out token))
					url = token.Value<string>();
			}
			using (StreamWriter sw = new StreamWriter(context.Response.OutputStream))
			{
				await sw.WriteAsync((new JObject(new JProperty("result", "OK"))).ToString());
			}
			return url;
		}

		#region IDisposable Members

		public void Dispose()
		{
			listener.Close();
		}

		#endregion
	}
}
