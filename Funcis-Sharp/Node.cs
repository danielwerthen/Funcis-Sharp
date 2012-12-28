using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FuncisSharp
{
	public abstract class Node
	{
	}

	public class LocalNode : Node
	{
		public void Call(SignalContext context, string fname, JArray paras, Action<JArray> callback)
		{
		}
	}

	public class RemoteNode : Node
	{
		public void Send(JObject data)
		{

		}
	}
}
