using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Knx.Infrastructure.DataTypes;

namespace Knx.Infrastructure.EventArguments
{
    public class WriteGroupTelegramEventArgs : EventArgs
    {
        public int Address { get; set; }
        public int Data { get; set; }

        public WriteGroupTelegramEventArgs(int address, int data)
        {
            Address = address;
            Data = data;
        }
    }
}
