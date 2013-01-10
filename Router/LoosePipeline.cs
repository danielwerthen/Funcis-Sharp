using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

namespace Knx.Router
{
    public class LoosePipeline<DATA> where DATA : class
    {
        Thread[] _workers;
        ConcurrentQueue<DATA> _items;
        Action<DATA> _outbound;
        readonly object _signal = new object();

        public LoosePipeline(Action<DATA> outbound, int workerCount = 2)
        {
            if (workerCount <= 0)
                throw new ArgumentException("LoosePipeline requires a positive amounts of workers");
            _workers = new Thread[workerCount];
            _items = new ConcurrentQueue<DATA>();
            _outbound = outbound;

            for (int i = 0; i < workerCount; i++)
            {
                (_workers[i] = new Thread(Pipe)).Start();
            }
        }

        void Pipe()
        {
            while (true)
            {
                DATA item;
                if (!_items.TryDequeue(out item))
                {
                    lock (_signal)
                    {
                        Monitor.Wait(_signal);
                    }
                    continue;
                }
                if (item == null) return;
                _outbound(item);
            }
        }

        public void Insert(DATA item)
        {
            _items.Enqueue(item);
            lock (_signal)
            {
                Monitor.Pulse(_signal);
            }
        }

        public void Shutdown(bool waitForWorkers = true)
        {
            foreach (var worker in _workers)
                Insert(null);

            if (waitForWorkers)
            {
                foreach (var worker in _workers)
                    worker.Join();
            }
        }
    }
}
