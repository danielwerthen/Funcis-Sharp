using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuncisSharp
{
	public class FuncEx
	{
		private Action<SignalContext, JArray, Action<JArray>> _func;
		public FuncEx(Action<SignalContext, JArray, Action<JArray>> func)
		{
			_func = func;
		}

		public void Call(SignalContext context, JArray args, Action<JArray> cb)
		{
			_func(context, args, cb);
		}
	}
}
