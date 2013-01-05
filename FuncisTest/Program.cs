using FuncisSharp;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuncisTest
{
	class Program
	{
		static void Main(string[] argss)
		{
			var funcis = new Funcis();
			var nodeA = funcis.CreateNode("NodeA", new string[] { "Calculator" });
			nodeA["Add"] = new FuncEx((signal, args, cb) =>
			{
				Console.WriteLine("Add");
				cb(new JArray(args[0].Value<int>() + args[1].Value<int>()));
			});
			nodeA["Print"] = new FuncEx((signal, args, cb) =>
			{
				Console.WriteLine(args[0].Value<int>());
			});
			funcis.AddProxy("http://localhost:3000");
			funcis.AddRemoteNode("http://localhost:3000", "NodeB", new string[] { "Printer" });
			//funcis.Start();
			var t = funcis.Listen();
			t.Wait();
			Console.ReadLine();
		}
	}
}
