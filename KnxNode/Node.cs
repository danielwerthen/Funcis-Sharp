using FuncisSharp;
using Knx.Router;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Knx.Infrastructure.DataTypes;
using Knx.Infrastructure;
using System.Threading;
using System.IO;
using System.Net;

namespace KnxNode
{
	class Node
	{
		static void Main(string[] argss)
		{
			/*var webReq = WebRequest.CreateHttp("http://kod-test.azurewebsites.net");
			//var webReq = WebRequest.CreateHttp("http://localhost:3051/hello.js");
			//var webReq = WebRequest.CreateHttp("http://polar-sands-2139.herokuapp.com");
			webReq.Timeout = 30000;
			webReq.Method = "POST";
			webReq.SendChunked = true;
			webReq.KeepAlive = true;

			using (var req = new StreamWriter(webReq.GetRequestStream()))
			{
				req.Write("Hello");
				req.Flush();
			}
			using (var response = webReq.GetResponse())
			{
				using (var res = new StreamReader(response.GetResponseStream()))
				{
					while (!res.EndOfStream)
					{
						Console.Write((char)res.Read());
					}
					Console.ReadLine();
				}
			}
			return;*/
			var url = argss.Count() > 0 ? argss[0] : "http://kod-test.azurewebsites.net/";
			//var url = argss.Count() > 0 ? argss[0] : "http://localhost:3000";
			Console.WriteLine("Proxying to url: " + url);
			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				var ex = e.ExceptionObject as Exception;
				Console.WriteLine("Exited by unknown error: " + ex.Message);
				var p = System.Diagnostics.Process.GetCurrentProcess();
				p.Kill();
			};

			KnxServer.Run((gate, writer) =>
			{
				var funcis = new Funcis();
				var node = funcis.CreateNode("KNX", new string[] { "Knx" });
				SignalExtender.Register(funcis);
				node["Listen"] = new FuncEx(new Action<SignalContext, JArray, Action<JArray>>((sig, args, cb) =>
				{
					if (args.Count < 1)
						return;
					EnmxAddress address = (string)args[0];
					Console.WriteLine("Listen to " + address.Address);
					gate.ConstructGate<int>(address, new Action<int, GroupTelegram>((val, telegram) =>
					{
						Console.WriteLine("Received from " + address.Address + " value of " + val);
						cb(new JArray(val, telegram.Received));
					}));
					sig.OnStop(new Action(() =>
					{
						Console.WriteLine("Stop listen to " + address.Address);
						gate.RemoveGate(address);
					}));
				}));

				node["Send"] = new FuncEx((sig, args, cb) =>
				{
					if (args.Count < 2)
						return;
					EnmxAddress address = (string)args[0];
					int val = (int)args[1];
					Console.WriteLine("Sending to: " + address + " the value of " + val);
					writer.Write(address, val);
				});
				funcis.AddProxy(url);
				funcis.AddRemoteNode(url, "Central", new string[0]);
				funcis.AddRemoteNode(url, "Me", new string[] { "Extender" });
				var t = funcis.Start();
				t.Wait();
				Console.WriteLine("KnxNode is running");
				funcis.BeginListen();
				while (true)
				{
					Thread.Sleep(1000);
				}

			}, true);
			return;
		}
	}
}
