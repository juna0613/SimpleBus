using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SimpleBus
{

    public class Notifier : INotifier
    {
        private readonly string _baseDir, _from;
        private readonly System.Text.RegularExpressions.Regex _matcher;
        private readonly FileSystemWatcher _watcher;
        private Dictionary<string, Action<AckInfo>> _callbacks = new Dictionary<string, Action<AckInfo>>();
        private readonly ITopicGenerator _topicGen;
        public Notifier(string from, string baseDir, ITopicGenerator topicGenerator = null)
        {
            _baseDir = baseDir;
            _from = from;
            _watcher = new FileSystemWatcher(baseDir, "*.sbm");
            _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;
            _watcher.Created += _watcher_Created;
            _watcher.Changed += _watcher_Changed;
            _topicGen = topicGenerator ?? new FromTopicGenerator(from);
            _matcher = new System.Text.RegularExpressions.Regex(_topicGen.GenerateAckTopicPattern().Replace("*", @".*"), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        // search ack is back
        private void _watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Callback(e);
        }

        private void _watcher_Created(object sender, FileSystemEventArgs e)
        {
            Callback(e);
        }

        private void Callback(FileSystemEventArgs e)
        {
            var filename = e.Name;
            if (_matcher.IsMatch(filename))
            {
                var parsed = _topicGen.Parse(filename);
                var from = parsed["from"];
                if (_callbacks.ContainsKey(from))
                {
                    var msg = File.ReadAllText(e.FullPath);
                    _callbacks[from](new AckInfo
                    {
                        From = from,
                        To = parsed["to"],
                        AckAt = DateTime.ParseExact(parsed["timestamp"], "yyyyMMddhhmmssfff", null),
                        Message = msg
                    });
                }
            }
        }

        public void Dispose()
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
        }

        public void Notify(string target, Action<AckInfo> callback = null, string body = null)
        {
            var topic = _topicGen.Generate(target);
            var ackPattern = _topicGen.GenerateAckTopicPattern();

            if (callback != null) _callbacks[target] = callback;

            if(!Directory.Exists(_baseDir))
            {
                Directory.CreateDirectory(_baseDir);
            }
            using (var file = new StreamWriter(Path.Combine(_baseDir, topic)))
            {
                if(!string.IsNullOrEmpty(body)) file.Write(body);
            }
            
        }
    }
}
