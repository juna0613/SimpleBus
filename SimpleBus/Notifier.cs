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
        private readonly FileSystemWatcher _watcher;
        private Dictionary<string, Action<MessageInfo>> _callbacks = new Dictionary<string, Action<MessageInfo>>();
        private readonly ITopicGenerator _topicGen;
        public Notifier(string from, string baseDir, ITopicGenerator topicGenerator = null)
        {
            _baseDir = baseDir;
            _from = from;
            _topicGen = topicGenerator ?? new FromTopicGenerator(from);
            _watcher = new FileSystemWatcher(baseDir, _topicGen.GenerateReceiveTopicPattern());
            _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;
            _watcher.Created += _watcher_Created;
            _watcher.Changed += _watcher_Changed;
            _watcher.EnableRaisingEvents = true;
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
            var parsed = _topicGen.Parse(filename);
            var from = parsed["from"];
            if (_callbacks.ContainsKey(from))
            {
                var msg = File.ReadAllText(e.FullPath);
                _callbacks[from](new MessageInfo
                {
                    From = from,
                    To = parsed["to"],
                    RecivedAt = DateTime.ParseExact(parsed["timestamp"], "yyyyMMddHHmmssfff", null),
                    Message = msg
                });
            }
        }

        public void Dispose()
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
        }

        public void Notify(string target, Action<MessageInfo> callback = null, string body = null)
        {
            var topic = _topicGen.Generate(target);
            var ackPattern = _topicGen.GenerateReceiveTopicPattern();

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
