using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Knx.Infrastructure.IO
{
    public interface ITransmitter
    {
        void Transmit(int address, int data);
        IEnumerable ToTransmit();
    }
}
