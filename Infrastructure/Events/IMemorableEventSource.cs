using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Knx.Infrastructure.Events
{
    public interface IMemorableEventSource<T> where T : EventArgs
    {
        event EventHandler<T> Memorable;
    }
}
