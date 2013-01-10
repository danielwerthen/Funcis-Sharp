using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Knx.Infrastructure.EventArguments
{
    public interface IMemorableEventArgs
    {
        DateTime Received { get; set; }
    }
}
