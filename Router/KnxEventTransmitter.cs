using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Knx.Infrastructure.DataTypes;
using System.Collections.Concurrent;
using Knx.Infrastructure.IO;

namespace Knx.Router
{
    public struct KnxEvent
    {
        public int Address;
        public int Data;
    }

    public class KnxEventTransmitter : ITransmitter
    {
        private ConcurrentQueue<KnxEvent> _transferQueue;

        public KnxEventTransmitter()
        {
            _transferQueue = new ConcurrentQueue<KnxEvent>();
        }

        public void Transmit(int address, int data)
        {
            _transferQueue.Enqueue(new KnxEvent() { Address = address, Data = data });
        }

        public void SendEvents(KnxWriter writer)
        {
            KnxEvent ev;
            while (_transferQueue.TryDequeue(out ev))
            {
                writer.Write(ev.Address, ev.Data);
            }
        }

        public System.Collections.IEnumerable ToTransmit()
        {
            return _transferQueue;
        }

        public void Clear()
        {
            KnxEvent dq;
            while (_transferQueue.TryDequeue(out dq))
            {

            }
        }
    }
}
