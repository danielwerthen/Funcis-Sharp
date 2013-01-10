using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Knx.Infrastructure.DataTypes;
using System.Collections.Concurrent;
using Knx.Infrastructure;

namespace Knx.Router
{
    public class TelegramGateway
    {
        private Dictionary<EnmxAddress, TelegramGateActor> _gates;

        public TelegramGateway()
        {
            _gates = new Dictionary<EnmxAddress, TelegramGateActor>();
        }

        public void Shutdown(bool waitForWorkers = true)
        {
            return;
        }

        public void ClearGates()
        {
            _gates.Clear();
        }

        public TelegramGateActor<PROPTYPE> ConstructGate<PROPTYPE>(EnmxAddress address, Action<PROPTYPE, GroupTelegram> valueChanged)
        {
            var gate = new TelegramGateActor<PROPTYPE>(address, valueChanged);
						_gates[address] = gate;
            return gate;
        }

				public void RemoveGate(EnmxAddress address)
				{
					_gates.Remove(address);
				}

        public void RouteGroupTelegram(GroupTelegram telegram)
        {
						if (_gates.ContainsKey(telegram.Address))
						{
							_gates[telegram.Address].ReceiveTelegram(telegram);
						}
        }
    }
}
