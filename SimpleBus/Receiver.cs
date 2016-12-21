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
        public Receiver(string receiverId, string baseDir, ITopicGenerator topicGen = null)
        {
            _topicGen = topicGen ?? new FromTopicGenerator(receiverId);
            _recId = receiverId;
            _baseDir = baseDir;
            _watcher = new FileSystemWatcher(_baseDir, _topicGen.GenerateReceiveTopicPattern());
            _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;
            _watcher.Changed += _watcher_Changed;
            _watcher.Created += _watcher_Created;
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
                RecivedAt = DateTime.ParseExact(parsed["timestamp"], "yyyyMMddhhmmssfff", null),
                Message = msg
            });
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
