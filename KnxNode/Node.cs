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

namespace KnxNode
{
	class Node
	{
		static void Main(string[] argss)
		{
			KnxServer.Run((gate, writer) =>
			{
				var funcis = new Funcis();
				var node = funcis.CreateNode("Kv", new string[] { "Knx" });
				node["Listen"] = new FuncEx(new Action<SignalContext, JArray, Action<JArray>>((sig, args, cb) =>
				{
					if (args.Count < 1)
						return;
					EnmxAddress address = (string)args[0];
					gate.ConstructGate<int>(address, new Action<int,GroupTelegram>((val, telegram) =>
					{
						cb(new JArray(val, telegram.Received));
					}));
                    //Random r = new Random();
                    //Timer t = new Timer((s) =>
                    //    {
                    //        cb(new JArray(r.Next(0, 255), DateTime.Now));
                    //    }, null, 1000, 1000);
					sig.OnStop(new Action(() =>
					{
                        //t.Change(Timeout.Infinite, Timeout.Infinite);
                        //t.Dispose();
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
				funcis.AddProxy("http://localhost:3000");
				funcis.AddRemoteNode("http://localhost:3000", "Central", new string[0]);
				funcis.Start();
				while (true)
				{
					var t = funcis.Listen();
					t.Wait();
				}
				
			}, false);
		}
	}
}
