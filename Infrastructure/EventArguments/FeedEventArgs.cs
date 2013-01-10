using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Knx.Infrastructure.EventArguments
{
    public class FeedEventArgs<T> : EventArgs, IMemorableEventArgs, IValueEventArgs<T>
    {
        public T Value { get; set; }
        public DateTime Received { get; set; }
    }
}
