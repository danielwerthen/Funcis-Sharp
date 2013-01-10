using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Knx.Infrastructure.EventArguments;

namespace Knx.Router
{
    public class KnxServer
    {
        public static void Run(Action<ConcurrentTelegramGateway, KnxWriter> eventLoop, bool useDefault = true)
        {
            using (var router = new RouterActor(useDefault))
            {
                ConcurrentTelegramGateway gateway = new ConcurrentTelegramGateway();
                LoosePipeline<ReceivedGroupTelegramEventArgs> pipe = new LoosePipeline<ReceivedGroupTelegramEventArgs>((args) =>
                {
                    gateway.RouteGroupTelegram(args.Telegram);
                });
                EventHandler<ReceivedGroupTelegramEventArgs> ReceivedGroupTelegram = (o, args) =>
                    {
                        pipe.Insert(args);
                    };
                KnxWriter writer = new KnxWriter();
                EventHandler<WriteGroupTelegramEventArgs> writeTelegram = (o, args) =>
                    {
                        router.Write(args.Address, args.Data);
                    };
                router.ReceivedGroupTelegram += ReceivedGroupTelegram;
                writer.WriteGroupTelegram += writeTelegram;
                eventLoop(gateway, writer);
                writer.WriteGroupTelegram -= writeTelegram;
                router.ReceivedGroupTelegram -= ReceivedGroupTelegram;
                pipe.Shutdown(true);
            }
        }
    }
}
