using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Knx.Infrastructure.DataTypes;

namespace Knx.Infrastructure
{
    public class GroupTelegram
    {
        public EnmxAddress Address
        {
            get;
            set;
        }

        public int Routing
        {
            get;
            set;
        }

        public int Priority
        {
            get;
            set;
        }

        public object Data
        {
            get;
            set;
        }

        public GroupTelegramTypes TelegramType
        {
            get;
            set;
        }

        public DateTime Received
        {
            get;
            set;
        }

        public GroupTelegram(int address, object data, DateTime received, int routing, int priority, GroupTelegramTypes type)
        {
            this.Address = address;
            this.Routing = routing;
            this.Priority = priority;
            this.Data = data;
            this.TelegramType = type;
            this.Received = received;
        }
    }
}
