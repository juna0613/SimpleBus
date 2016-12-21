using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBus
{
    public interface ITopicGenerator
    {
        string Generate(string to, DateTime? timestamp = null);
        string GenerateReceiveTopicPattern();
        Dictionary<string, string> Parse(string input);
    }

    public class FromTopicGenerator : ITopicGenerator
    {
        private readonly string _from;
        public FromTopicGenerator(string from)
        {
            _from = from;
        }

        public string Generate(string to, DateTime? timestamp = default(DateTime?))
        {
            return string.Join(".", new[] { _from, to, (timestamp ?? DateTime.Now).ToString("yyyyMMddHHmmssfff"), "sbm" });
        }

        public string GenerateReceiveTopicPattern()
        {
            return string.Join(".", new[] { "*" , _from, "*", "sbm" });
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
