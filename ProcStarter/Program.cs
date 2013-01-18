using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcStarter
{
	class Program
	{
		private static async void ReadOutputAsync(Process p, CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				try
				{
					var t = await p.StandardOutput.ReadLineAsync();
					Console.WriteLine(t);
				}
				catch (Exception)
				{
				}
			}
			return;
		}

		static Process p;

		private static void Start(CancellationToken mainToken)
		{
			var info = GetInfo(url);
			p = Process.Start(info);
			p.EnableRaisingEvents = true;
			CancellationTokenSource source = new CancellationTokenSource();
			var token = source.Token;
			p.Exited += (sender, e) =>
			{
				source.Cancel();
				if (mainToken.IsCancellationRequested)
					return;
				Console.WriteLine("Restarting in 1000ms");
				Thread.Sleep(1000);
				Start(mainToken);
			};
			ReadOutputAsync(p, token);
		}

		private static async void Listen(CancellationToken token, Action<string> restart)
		{
			HttpServer server = new HttpServer(3001);
			while (!token.IsCancellationRequested)
			{
				var url = await server.Receive();
				if (url != null)
					restart(url);
			}
		}

		private static ProcessStartInfo GetInfo(string URL)
		{
			var info = new ProcessStartInfo("KnxNode", URL);
			info.RedirectStandardOutput = true;
			info.UseShellExecute = false;
			return info;
		}

		static string url = "http://localhost:3000";
		static void Main(string[] args)
		{
			CancellationTokenSource source = new CancellationTokenSource();
			Start(source.Token);
			var main = Process.GetCurrentProcess();
			main.EnableRaisingEvents = true;
			main.Exited += (o, e) =>
			{
				if (!p.HasExited)
					p.Kill();
			};
			
			Listen(source.Token, (u) =>
			{
				if (!p.HasExited)
					p.Kill();
				url = u;
			});
			Console.ReadLine();
			source.Cancel();
		}
	}
}
