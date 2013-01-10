using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Knx.Infrastructure.EventArguments
{
    public interface IValueEventArgs<T>
    {
        T Value { get; set; }
    }
}
