using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBus
{
    public class MessageInfo
    {
        public string From { get; set; }
        public string To { get; set; }
        public DateTime? RecivedAt { get; set; }
        public string Message { get; set; }
    }
}