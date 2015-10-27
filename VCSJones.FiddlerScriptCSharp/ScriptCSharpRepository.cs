using Fiddler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;

namespace VCSJones.FiddlerScriptCSharp
{
    public class ScriptCSharpRepository : IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        private readonly Dictionary<string, FiddlerCSharpScript> _activeScripts = new Dictionary<string, FiddlerCSharpScript>(StringComparer.OrdinalIgnoreCase);
        private const string _filter = "*.csx";
        private readonly object _sync = new object();

        public ScriptCSharpRepository(string path)
        {
            _watcher = new FileSystemWatcher(path, _filter);
            FirstTimeInitialization();
            Observable.FromEventPattern<FileSystemEventArgs>(_watcher, nameof(_watcher.Changed))
                .Throttle(TimeSpan.FromMilliseconds(250))
                .Subscribe(_watcher_Changed);
            _watcher.EnableRaisingEvents = true;
        }

        private void _watcher_Changed(EventPattern<FileSystemEventArgs> args)
        {
            var e = args.EventArgs;
            switch(e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    lock (_sync)
                    {
                        FiddlerApplication.Log.LogString($"Initializing new script file \"{e.FullPath}\".");
                        var script = new FiddlerCSharpScript(e.FullPath);
                        script.Initialize();
                        _activeScripts.Add(e.FullPath, script);
                    }
                    break;
                case WatcherChangeTypes.Changed:
                    lock (_sync)
                    {
                        FiddlerApplication.Log.LogString($"Re-initializing script file \"{e.FullPath}\".");
                        _activeScripts[e.FullPath]?.Initialize();
                    }
                    break;
                case WatcherChangeTypes.Deleted:
                    lock (_sync)
                    {
                        FiddlerApplication.Log.LogString($"Removing script file \"{e.FullPath}\".");
                        _activeScripts.Remove(e.FullPath);
                    }
                    break;
            }
        }

        private static void ExecuteSafely(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                FiddlerApplication.Log.LogString($"An exception occured while executing part of a script:\r\n{e}");
            }
        }

        private static T ExecuteSafely<T>(Func<T> action)
        {
            try
            {
                return action();
            }
            catch (Exception e)
            {
                FiddlerApplication.Log.LogString($"An exception occured while executing part of a script:\r\n{e}");
                return default(T);
            }
        }

        public void ExecuteAllAutoTamperRequestBefore(Session session)
        {
            foreach (var script in _activeScripts.Values)
            {
                ExecuteSafely(() => script.AutoTamperRequestBefore(session));
            }
        }

        public void ExecuteAllAutoTamperRequestAfter(Session session)
        {
            foreach (var script in _activeScripts.Values)
            {
                ExecuteSafely(() => script.AutoTamperRequestAfter(session));
            }
        }

        public void ExecuteAllAutoTamperResponseAfter(Session session)
        {
            foreach (var script in _activeScripts.Values)
            {
                ExecuteSafely(() => script.AutoTamperResponseAfter(session));
            }
        }

        public void ExecuteAllAutoTamperResponseBefore(Session session)
        {
            foreach (var script in _activeScripts.Values)
            {
                ExecuteSafely(() => script.AutoTamperResponseBefore(session));
            }
        }

        public void ExecuteAllOnPeekAtRequestHeaders(Session session)
        {
            foreach (var script in _activeScripts.Values)
            {
                ExecuteSafely(() => script.OnPeekAtRequestHeaders(session));
            }
        }


        public void ExecuteAllOnPeekAtResponseHeaders(Session session)
        {
            foreach (var script in _activeScripts.Values)
            {
                ExecuteSafely(() => script.OnPeekAtResponseHeaders(session));
            }
        }

        public void ExecuteAllOnBeforeReturningError(Session session)
        {
            foreach (var script in _activeScripts.Values)
            {
                ExecuteSafely(() => script.OnBeforeReturningError(session));
            }
        }

        public void ExecuteAllOnBoot()
        {
            foreach (var script in _activeScripts.Values)
            {
                ExecuteSafely(() => script.OnBoot());
            }
        }

        public void ExecuteAllOnAttach()
        {
            foreach (var script in _activeScripts.Values)
            {
                ExecuteSafely(() => script.OnAttach());
            }
        }

        public void ExecuteAllOnDetach()
        {
            foreach (var script in _activeScripts.Values)
            {
                ExecuteSafely(() => script.OnDetach());
            }
        }

        public void ExecuteAllOnShutdown()
        {
            foreach (var script in _activeScripts.Values)
            {
                ExecuteSafely(() => script.OnShutdown());
            }
        }

        public void ExecuteAllOnDone(Session session)
        {
            foreach (var script in _activeScripts.Values)
            {
                ExecuteSafely(() => script.OnDone(session));
            }
        }

        public bool ExecuteAllOnBeforeShutdown()
        {
            var continueShutDown = true;
            foreach (var script in _activeScripts.Values)
            {
                continueShutDown &= ExecuteSafely(() => script.OnBeforeShutdown()) ?? true;
            }
            return continueShutDown;
        }

        private void FirstTimeInitialization()
        {
            lock (_sync)
            {
                var files = Directory.EnumerateFiles(_watcher.Path, _watcher.Filter);
                foreach (var file in files)
                {
                    FiddlerApplication.Log.LogString($"Initializing new script file \"{file}\".");
                    var script = new FiddlerCSharpScript(file);
                    script.Initialize();
                    _activeScripts.Add(file, script);
                }
            }
        }

        public void Dispose()
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
        }
    }
}
