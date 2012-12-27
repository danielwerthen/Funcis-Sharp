using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Linq;
using FuncisSharp;

namespace ParserTests
{
	[TestClass]
	public class ParseTest
	{
		[TestMethod]
		public void TestMethod1()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("NodeA.Add(1,2)");
			sb.AppendLine("\t(res) =>");
			sb.AppendLine("\t\tNodeB.Print(res)");
			sb.AppendLine("\t\t\t() =>");
			sb.AppendLine("\t\t\t\tNodeB.Print(35)");
			Parser p = new Parser(sb.ToString());
			var t = p.Signalify().ToList();
			Assert.IsTrue(t.Count == 1);
			var f1 = t.First();
			Assert.IsTrue(f1.Parameters.Count == 2);
			Assert.IsTrue(f1.Callbacks.Count == 1);
			Assert.IsTrue(f1.Callbacks.First().Callbacks.Count == 1);
			
		}
	}
}
