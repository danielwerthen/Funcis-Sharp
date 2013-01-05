using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FuncisSharp;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ParserTests
{
	[TestClass]
	public class FuncisTest
	{
		[TestMethod]
		public void TestIt()
		{
			List<object> results = new List<object>();
			NodeMap<LocalNode> locals = new NodeMap<LocalNode>();
			NodeMap<RemoteNode> remotes = new NodeMap<RemoteNode>();
			var NodeA = new LocalNode("NodeA", new string[] { "Calculator", "Printer" });
			locals.AddNode(NodeA);
			NodeA["Add"] = new FuncEx((sig, args, cb) =>
			{
				cb(new JArray(args[0].Value<int>() + args[1].Value<int>()));
			});
			NodeA["Print"] = new FuncEx((sig, args, cb) =>
			{
				results.Add(args[0].Value<int>());
			});
			Signal s = new Signal(locals, remotes);
			s.Load("NodeA.Calculator.Add(4,4)\n\t(res) => \n\t\tNodeA.Prinaster.Print(res)");
			s.Start();
			Assert.IsTrue(results.Count == 1 && (int)results[0] == 8);
		}

	}
}
