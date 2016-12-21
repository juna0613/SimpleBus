using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBus
{
    public interface IReceiver : IDisposable
    {
        void Start(Action<MessageInfo> callback);
    }
}
