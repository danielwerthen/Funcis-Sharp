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
			var nodeA = funcis.CreateNode("NodeA", new string[] { "Calculator", "Printer" });
			nodeA["Add"] = new FuncEx((signal, args, cb) =>
			{
				cb(new JArray(args[0].Value<int>() + args[1].Value<int>()));
			});
			nodeA["Print"] = new FuncEx((signal, args, cb) =>
			{
				Console.WriteLine(args[0].Value<int>());
			});
			funcis.Start();
			Console.ReadLine();
		}
	}
}
