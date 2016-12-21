using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleBus;
namespace SimpleClient
{
    class Program
    {
        // type=s(end)|r(eceive) targetDir my_id to_id message
        static void Main(string[] args)
        {
            if(args.Length < 4)
            {
                Console.WriteLine("usage: type=s(end)|r(eceive) targetDir my_id to_id message");
                return;
            }
            var type = args[0];
            var dir = args[1];
            var myid = args[2];
            var toid = args[3];
            var msg = args.Length >= 5 ? args[4] : string.Empty;

            IReceiver rec = null;
            INotifier notif = null;

            if(type.ToUpper()[0] == 'S')
            {
                notif = new Notifier(myid, dir);
            }
            else 
            {
                rec = new Receiver(myid, dir);
                rec.Start(x =>
                {
                    Console.WriteLine("[Received]{0} {1} {2} {3}", x.RecivedAt ?? DateTime.Now, x.From, x.To, x.Message);
                });
            }

            while(true)
            {
                var input = Console.ReadLine();
                if(input.ToUpper() == "E")
                {
                    break;
                }
                if(notif != null)
                {
                    notif.Notify(toid, x =>
                    {
                        Console.WriteLine("[Acknoledged]{0} {1} {2} {3}", x.RecivedAt ?? DateTime.Now, x.From, x.To, x.Message);
                    }, "Hello " + toid);
                }
            }
        }
    }
}
