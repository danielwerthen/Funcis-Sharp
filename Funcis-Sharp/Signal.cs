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
		public NodeMap<LocalNode> Locals { get; set; }
		public NodeMap<RemoteNode> Remotes { get; set; }
		public string Signature { get; set; }
		public List<Function> Funcs { get; set; }
		private SignalContext Context { get; set; }

		public Signal(NodeMap<LocalNode> locals, NodeMap<RemoteNode> remotes)
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
			
			return hash.Aggregate(new StringBuilder(), (sb, b) => sb.Append(b.ToString("X2"))).ToString().ToLower();
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

		public async Task Execute(int[] pos, JObject scope, bool local)
		{
			var f = this.Traverse(pos);
			var self = this;
			if (f == null) return;
			foreach (LocalNode exe in this.Locals.Resolve(f.Selector)) 
			{
				var paras = f.Parameters.Build(scope);
				exe.Call(this.Context, f.Name, paras, async (args) =>
				{
					var newScope = Interpolate(f.CallbackParameters, args, scope);
					for (var i = 0; i < f.CallbackParameters.Count; i++)
					{
						await Execute(pos.Concat(new int[1] { i }).ToArray(), newScope, false);
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
				await exe.Send(data);
			}
		}

		public async Task HandleCall(JObject data)
		{
			JToken signature;
			if (!data.TryGetValue("signature", out signature)) return;
			if (signature.Value<string>() != this.Signature) return;
			JToken pos, scope;
			if (data.TryGetValue("pos", out pos) && pos is JArray && data.TryGetValue("scope", out scope) && scope is JObject)
			{
				await this.Execute(pos.Values().Select(row => row.Value<int>()).ToArray(), (JObject)scope, true);
			}
		}

		public async Task Start()
		{
			for (var i = 0; i < Funcs.Count; i++)
			{
				await Execute(new int[1] { i }, new JObject(), true);
			}
		}

		public void Stop()
		{
			this.Context.EmitStop();
		}
	}
}
