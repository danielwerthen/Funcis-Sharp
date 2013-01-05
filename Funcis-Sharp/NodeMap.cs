using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FuncisSharp
{
	public class NodeMap<N> where N : Node
	{
		private Dictionary<string, N> _nodes = new Dictionary<string, N>();

		public NodeMap()
		{
		}

		public void AddNode(N node)
		{
			_nodes[node.Name] = node;
		}

		public void RemoveNode(string name)
		{
			_nodes[name] = null;
		}

		public IEnumerable<N> Nodes
		{
			get
			{
				return _nodes.Values;
			}
		}

		private static string Name(string str)
		{
			var cap = Regex.Match(str, @"^(\w*)");
			if (cap.Success)
				return cap.Groups[1].Value;
			return null;
		}

		private static List<string> Classes(string str)
		{
			Match cap;
			string cp = str;
			List<string> res = new List<string>();
			do
			{
				cap = Regex.Match(cp, @"^\w*\.(\w+)");
				if (cap.Success)
				{
					cp = cp.Substring(cap.Groups[0].Length);
					res.Add(cap.Groups[1].Value);
				}
			} while (cap.Success);
			return res;
		}

		private Func<N, bool> Match(string name)
		{
			return (node) =>
				{
					return node.Name == name;
				};
		}

		private Func<N, bool> Match(List<string> classes)
		{
			return (node) =>
				{
					return classes.All(row => node.Classes.Contains(row));
				};
		}

		private Func<N, bool> Match(string name, List<string> classes)
		{
			List<Func<N, bool>> matches = new List<Func<N, bool>>();

			if (!string.IsNullOrEmpty(name))
				matches.Add(Match(name));
			if (classes.Count > 0)
				matches.Add(Match(classes));
			return (node) => matches.All(row => row(node));
		}

		public IEnumerable<N> Resolve(string p)
		{
			var name = Name(p);
			var classes = Classes(p);
			return _nodes.Values.Where(Match(name, classes)).ToList();
				
		}
	}
}
