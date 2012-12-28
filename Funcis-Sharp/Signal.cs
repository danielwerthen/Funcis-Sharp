using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FuncisSharp
{
	public class Signal
	{
		public NodeMap Locals { get; set; }
		public NodeMap Remotes { get; set; }
		public string Signature { get; set; }
		public List<Function> Funcs { get; set; }
		private SignalContext Context { get; set; }

		public Signal(NodeMap locals, NodeMap remotes)
		{
			this.Locals = locals;
			this.Remotes = remotes;
			this.Signature = "";
			this.Funcs = new List<Function>();
			this.Context = new SignalContext();
		}

		private static string Hash(string str)
		{
			MD5 md5 = MD5.Create();
			byte[] inputBytes = Encoding.UTF8.GetBytes(str);
			byte[] hash = md5.ComputeHash(inputBytes);
			return hash.Aggregate(new StringBuilder(), (sb, b) => sb.Append(b.ToString("X2"))).ToString();
		}

		public void Load(string str)
		{
			var p = new Parser(str);
			this.Funcs = p.Signalify().ToList();
			this.Signature = Hash(this.Funcs.Print());
		}

		private Function Traverse(int[] pos)
		{
			var fs = this.Funcs;
			Function f = null;
			foreach (var idx in pos)
			{
				f = fs[idx];
				fs = f.Callbacks;
			}
			return f;
		}

		public JObject Interpolate(List<CallbackParameter> paras, JArray args, JObject scope)
		{
			var nscope = scope.DeepClone() as JObject;
			for (var i = 0; i < paras.Count(); i++)
			{
				nscope[paras[i].Name] = args[i];
			}
			return nscope;
		}

		public void Execute(int[] pos, JObject scope, bool local)
		{
			var f = this.Traverse(pos);
			var self = this;
			if (f == null) return;
			foreach (LocalNode exe in this.Locals.Resolve(f.Selector)) 
			{
				var paras = f.Parameters.Build(scope);
				exe.Call(this.Context, f.Name, paras, (args) =>
				{
					var newScope = Interpolate(f.CallbackParameters, args, scope);
					for (var i = 0; i < f.CallbackParameters.Count; i++)
					{
						self.Execute(pos.Concat(new int[1] { i }).ToArray(), newScope, false);
					}
				});
			}
			if (local) return;
			foreach (RemoteNode exe in this.Remotes.Resolve(f.Selector))
			{
				var data = new JObject();
				data["signature"] = self.Signature;
				data["pos"] = new JArray(pos);
				data["scope"] = scope;
				exe.Send(data);
			}
		}
	}
}
