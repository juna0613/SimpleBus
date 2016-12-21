using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBus
{
    public class Receiver : IReceiver
    {
        private readonly string _recId, _baseDir;
        private readonly ITopicGenerator _topicGen;
        private readonly FileSystemWatcher _watcher;
        private Action<MessageInfo> _callback;
        private readonly bool _ack;
        public Receiver(string receiverId, string baseDir, bool makeAcknolege = true, ITopicGenerator topicGen = null)
        {
            _topicGen = topicGen ?? new FromTopicGenerator(receiverId);
            _recId = receiverId;
            _baseDir = baseDir;
            _watcher = new FileSystemWatcher(_baseDir, _topicGen.GenerateReceiveTopicPattern());
            _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;
            _watcher.Changed += _watcher_Changed;
            _watcher.Created += _watcher_Created;
            _ack = makeAcknolege;
        }
        private void Callback(FileSystemEventArgs e)
        {
            if (_callback == null) return;

            var filename = e.Name;
            var parsed = _topicGen.Parse(filename);
            var from = parsed["from"];
            var msg = File.ReadAllText(e.FullPath);
            _callback(new MessageInfo
            {
                From = from,
                To = parsed["to"],
                RecivedAt = DateTime.ParseExact(parsed["timestamp"], "yyyyMMddHHmmssfff", null),
                Message = msg
            });
            if(_ack)
            {
                var topic = _topicGen.Generate(from);
                if (!Directory.Exists(_baseDir))
                {
                    Directory.CreateDirectory(_baseDir);
                }
                using (var file = new StreamWriter(Path.Combine(_baseDir, topic)))
                {
                    file.WriteLine("[Acknoledged] {0} {1}", DateTime.Now, _recId);
                }
            }
        }
        private void _watcher_Created(object sender, FileSystemEventArgs e)
        {
            Callback(e);
        }

        private void _watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Callback(e);
        }

        public void Dispose()
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
        }

        public void Start(Action<MessageInfo> callback)
        {
            _callback = callback;
            _watcher.EnableRaisingEvents = true;
        }
    }
}
