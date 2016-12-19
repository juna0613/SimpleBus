using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBus
{
    public interface ITopicGenerator
    {
        string Generate();
        string GenerateAckTopicPattern();
        Dictionary<string, string> Parse(string input);
    }

    public class FromToTopicGenerator : ITopicGenerator
    {
        private readonly string _from, _to;
        private readonly DateTime? _time;
        public FromToTopicGenerator(string from, string to, DateTime? timestamp=null)
        {
            _from = from;
            _to = to;
            _time = timestamp;
        }
        public string Generate()
        {
            return string.Join(".", new[] { _from, _to, (_time ?? DateTime.Now).ToString("yyyyMMddhhmmssfff"), "sbm" });
        }

        public string GenerateAckTopicPattern()
        {
            return string.Join(".", new[] { _to, _from, "*", "sbm" });
        }

        public Dictionary<string, string> Parse(string input)
        {
            var split = input.Split('.');
            return new Dictionary<string, string>
            {
                { "from", split[0] },
                { "to", split[1] },
                { "timestamp", split[2] },
            };
        }
    }
}
