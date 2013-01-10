using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Knx.Infrastructure.DataTypes;
using System.Collections.Concurrent;
using Knx.Infrastructure;

namespace Knx.Router
{
    public class ConcurrentTelegramGateway
    {
        private ConcurrentDictionary<EnmxAddress, ConcurrentBag<TelegramGateActor>> _gates;

        public ConcurrentTelegramGateway(int workerCount = 3)
        {
            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 2;
            _gates = new ConcurrentDictionary<EnmxAddress, ConcurrentBag<TelegramGateActor>>(concurrencyLevel, 1024);
        }

        public void Shutdown(bool waitForWorkers = true)
        {
            return;
        }

        public void ClearGates()
        {
            _gates.Clear();
        }

        private void addGate(EnmxAddress address, TelegramGateActor gate)
        {
            _gates.AddOrUpdate(address, new ConcurrentBag<TelegramGateActor>() { gate }, (a, bag) =>
            {
                bag.Add(gate);
                return bag;
            });
        }

        public TelegramGateActor<PROPTYPE> ConstructGate<PROPTYPE>(EnmxAddress address, Action<PROPTYPE, GroupTelegram> valueChanged)
        {
            var gate = new TelegramGateActor<PROPTYPE>(address, valueChanged);
            addGate(address, gate);
            return gate;
        }

        public void RouteGroupTelegram(GroupTelegram telegram)
        {
            ConcurrentBag<TelegramGateActor> gates;
            if (_gates.TryGetValue(telegram.Address, out gates))
            {
                gates.Aggregate(new object(), (obj, gate) =>
                {
                    gate.ReceiveTelegram(telegram);
                    return obj;
                });
            }
        }
    }
}
