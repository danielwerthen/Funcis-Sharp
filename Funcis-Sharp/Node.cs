using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuncisSharp
{
	public abstract class Node
	{
		public string Name { get; set; }
		public string[] Classes { get; set; }
	}

	public class LocalNode : Node
	{
		private Dictionary<string, FuncEx> _funcs = new Dictionary<string, FuncEx>();
		public FuncEx this[string name]
		{
			get
			{
				return _funcs[name];
			}
			set
			{
				_funcs[name] = value;
			}
		}

		public LocalNode(string name, string[] classes)
		{
			this.Name = name;
			this.Classes = classes;
		}

		public void Call(SignalContext context, string fname, JArray paras, Action<JArray> callback)
		{
			var f = _funcs[fname];
			f.Call(context, paras, callback);
		}
	}

	public class RemoteNode : Node
	{
		private Pipe pipe;
		public RemoteNode(Pipe pipe, string name, string[] classes)
		{
			this.Name = name;
			this.Classes = classes;
			this.pipe = pipe;
		}

		public async Task Send(JObject data)
		{
			await pipe.Send(data);
		}
	}
}
