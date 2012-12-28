using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FuncisSharp
{
	public class SignalContext
	{
		private List<Action> _onStop = new List<Action>();
		private object _sync = new object();
		public void OnStop(Action act)
		{
			lock (_sync)
			{
				_onStop.Add(act);
			}
		}

		public void EmitStop()
		{
			lock (_sync)
			{
				foreach (var act in _onStop)
				{
					act();
				}
				_onStop.Clear();
			}
		}
	}
}
