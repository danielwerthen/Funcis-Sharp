using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Knx.Infrastructure.EventArguments;
using Knx.Infrastructure.DataTypes;

namespace Knx.Router
{
    public class KnxWriter
    {
        public KnxWriter()
        {

        }

        public void Write(EnmxAddress address, int data)
        {
            if (WriteGroupTelegram != null)
                WriteGroupTelegram(this, new WriteGroupTelegramEventArgs(address, data));
        }

        public void Write(int address, int data)
        {
            if (WriteGroupTelegram != null)
                WriteGroupTelegram(this, new WriteGroupTelegramEventArgs(address, data));
        }

        public event EventHandler<WriteGroupTelegramEventArgs> WriteGroupTelegram;
    }
}
