using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBus
{
    public class AckInfo
    {
        public string From { get; set; }
        public string To { get; set; }
        public DateTime? AckAt { get; set; }
        public string Message { get; set; }
    }

    public interface INotifier : IDisposable
    {
        void Notify(string target, Action<AckInfo> callbackFunc = null, string body = null);
    }
}
