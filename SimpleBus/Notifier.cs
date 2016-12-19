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
        private Dictionary<string, bool> _matchPatterns = new Dictionary<string, bool>();
        private Dictionary<string, Func<string, string>> _callbacks = new Dictionary<string, Func<string, string>>();
        private Dictionary<string, ITopicGenerator> _topicGens = new Dictionary<string, ITopicGenerator>();
        public Notifier(string from, string baseDir)
        {
            _baseDir = baseDir;
            _from = from;
            _watcher = new FileSystemWatcher(baseDir, "*.sbm");
            _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;
            _watcher.Created += _watcher_Created;
            _watcher.Changed += _watcher_Changed;
        }

        private void _watcher_Changed(object sender, FileSystemEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void _watcher_Created(object sender, FileSystemEventArgs e)
        {

        }

        public void Dispose()
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
        }

        public void Notify(string target, Func<string, string> callbackFunc = null, string body = null)
        {
            var gen = new FromToTopicGenerator(_from, target);
            var topic = gen.Generate();
            var ackPattern = gen.GenerateAckTopicPattern();

            _matchPatterns[ackPattern] = true;
            _topicGens[target] = gen;
            if (callbackFunc != null) _callbacks[target] = callbackFunc;

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
