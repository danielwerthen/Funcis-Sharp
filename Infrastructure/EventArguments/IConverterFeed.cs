using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Knx.Infrastructure.EventArguments
{
    public interface IConverterFeed<T>
    {
        void Feed(T value, DateTime received);
    }
}
