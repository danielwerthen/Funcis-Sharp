using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Knx.Infrastructure;
using System.Collections.Concurrent;
using Knx.Infrastructure.DataTypes;
using Knx.Infrastructure.EventArguments;

namespace Knx.Router
{
    public abstract class TelegramGateActor
    {
        internal abstract void ReceiveTelegram(GroupTelegram telegram);
    }

    public class TelegramGateActor<PROPTYPE> : TelegramGateActor
    {
        EnmxAddress _address;
        Action<PROPTYPE, GroupTelegram> _valueChanged;
        public TelegramGateActor(EnmxAddress address, Action<PROPTYPE, GroupTelegram> valueChanged)
        {
            _address = address;
            _valueChanged = valueChanged;
        }

        internal override void ReceiveTelegram(GroupTelegram args)
        {
            PROPTYPE newVal = KnxValueParser.Parse<PROPTYPE>(args.Data);
            _valueChanged(newVal, args);
        }
    }
}
