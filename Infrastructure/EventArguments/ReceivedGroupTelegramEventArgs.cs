using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Knx.Infrastructure.EventArguments
{
    public class ReceivedGroupTelegramEventArgs : EventArgs
    {
        public GroupTelegram Telegram
        {
            get;
            set;
        }

        public ReceivedGroupTelegramEventArgs(GroupTelegram telegram)
        {
            this.Telegram = telegram;
        }
    }
}
