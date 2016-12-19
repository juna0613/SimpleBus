using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBus
{
    public interface INotifier : IDisposable
    {
        void Notify(string target, Func<string, string> callbackFunc = null, string body = null);
    }
}
