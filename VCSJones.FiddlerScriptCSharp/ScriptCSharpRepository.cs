using Fiddler;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;
using System;
using System.Collections.Concurrent;
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

        public void ExecuteAllAutoTamperRequestBefore(Session session)
        {
            foreach (var script in _activeScripts.Values)
            {
                script.AutoTamperRequestBefore(session);
            }
        }

        public void ExecuteAllAutoTamperRequestAfter(Session session)
        {
            foreach (var script in _activeScripts.Values)
            {
                script.AutoTamperRequestAfter(session);
            }
        }

        public void ExecuteAllAutoTamperResponseAfter(Session session)
        {
            foreach (var script in _activeScripts.Values)
            {
                script.AutoTamperResponseAfter(session);
            }
        }

        public void ExecuteAllAutoTamperResponseBefore(Session session)
        {
            foreach (var script in _activeScripts.Values)
            {
                script.AutoTamperResponseBefore(session);
            }
        }

        public void ExecuteAllOnPeekAtRequestHeaders(Session session)
        {
            foreach (var script in _activeScripts.Values)
            {
                script.OnPeekAtRequestHeaders(session);
            }
        }


        public void ExecuteAllOnPeekAtResponseHeaders(Session session)
        {
            foreach (var script in _activeScripts.Values)
            {
                script.OnPeekAtResponseHeaders(session);
            }
        }

        public void ExecuteAllOnBeforeReturningError(Session session)
        {
            foreach (var script in _activeScripts.Values)
            {
                script.OnBeforeReturningError(session);
            }
        }

        public void ExecuteAllOnBoot()
        {
            foreach (var script in _activeScripts.Values)
            {
                script.OnBoot();
            }
        }

        public void ExecuteAllOnAttach()
        {
            foreach (var script in _activeScripts.Values)
            {
                script.OnAttach();
            }
        }

        public void ExecuteAllOnDetach()
        {
            foreach (var script in _activeScripts.Values)
            {
                script.OnDetach();
            }
        }

        public void ExecuteAllOnShutdown()
        {
            foreach (var script in _activeScripts.Values)
            {
                script.OnShutdown();
            }
        }

        public bool ExecuteAllOnBeforeShutdown()
        {
            bool continueShutDown = true;
            foreach (var script in _activeScripts.Values)
            {
                continueShutDown &= script.OnBeforeShutdown() ?? true;
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

    public class FiddlerCSharpScript : IEquatable<FiddlerCSharpScript>, IComparable<FiddlerCSharpScript>
    {
        private readonly string _path;
        private Action<Session> AutoTamperRequestBeforeDelegate, AutoTamperRequestAfterDelegate, AutoTamperResponseAfterDelegate, AutoTamperResponseBeforeDelegate;
        private Action<Session> OnPeekAtRequestHeadersDelegate, OnPeekAtResponseHeadersDelegate;
        private Action<Session> OnBeforeReturningErrorDelegate;
        private Action OnFiddlerAttachDelegate, OnFiddlerDetachDelegate;
        private Func<bool> OnFiddlerBeforeShutdownDelegate;
        private Action OnFiddlerBootDelegate, OnFiddlerShutdownDelegate;


        public FiddlerCSharpScript(string path)
        {
            _path = path;
        }

        public void Initialize()
        {
            var options = ScriptOptions.Default
                            .WithIsInteractive(false)
                            .WithReferences(
                                typeof(IAutoTamper3).Assembly,
                                typeof(string).Assembly,
                                typeof(System.Dynamic.DynamicObject).Assembly,
                                typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly,
                                typeof(Uri).Assembly
                            );
            string text;
            using (var fs = new FileStream(_path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(fs))
                {
                    text = reader.ReadToEnd();
                }
            }

            ScriptState script;
            try
            {
                script = CSharpScript.Run(text, options);
            }
            catch (Exception e)
            {
                FiddlerApplication.Log.LogString(e.ToString());
                return;
            }
            AutoTamperRequestBeforeDelegate = script.CreateDelegate<Action<Session>>("OnBeforeRequest") ?? script.CreateDelegate<Action<Session>>(nameof(IAutoTamper3.AutoTamperRequestBefore));
            AutoTamperRequestAfterDelegate = script.CreateDelegate<Action<Session>>("OnAfterRequest") ?? script.CreateDelegate<Action<Session>>(nameof(IAutoTamper3.AutoTamperRequestAfter));
            AutoTamperResponseBeforeDelegate = script.CreateDelegate<Action<Session>>("OnBeforeResponse") ?? script.CreateDelegate<Action<Session>>(nameof(IAutoTamper3.AutoTamperResponseBefore));
            AutoTamperResponseAfterDelegate = script.CreateDelegate<Action<Session>>("OnAfterResponse") ?? script.CreateDelegate<Action<Session>>(nameof(IAutoTamper3.AutoTamperResponseAfter));
            OnBeforeReturningErrorDelegate = script.CreateDelegate<Action<Session>>("OnReturningError") ?? script.CreateDelegate<Action<Session>>(nameof(IAutoTamper3.OnBeforeReturningError));
            OnPeekAtRequestHeadersDelegate = script.CreateDelegate<Action<Session>>(nameof(IAutoTamper3.OnPeekAtRequestHeaders));
            OnPeekAtResponseHeadersDelegate = script.CreateDelegate<Action<Session>>(nameof(IAutoTamper3.OnPeekAtResponseHeaders));
            OnFiddlerAttachDelegate = script.CreateDelegate<Action>("OnAttach");
            OnFiddlerDetachDelegate = script.CreateDelegate<Action>("OnDetach");
            OnFiddlerBeforeShutdownDelegate = script.CreateDelegate<Func<bool>>("OnBeforeShutdown");
            OnFiddlerBootDelegate = script.CreateDelegate<Action>("OnBoot");
            OnFiddlerShutdownDelegate = script.CreateDelegate<Action>("OnShutdown");
        }

        public int CompareTo(FiddlerCSharpScript other) => StringComparer.OrdinalIgnoreCase.Compare(_path, other._path);

        public bool Equals(FiddlerCSharpScript other) =>  CompareTo(other) == 0;

        public void AutoTamperRequestBefore(Session session) => AutoTamperRequestBeforeDelegate?.Invoke(session);
        public void AutoTamperRequestAfter(Session session) => AutoTamperRequestAfterDelegate?.Invoke(session);
        public void AutoTamperResponseAfter(Session session) => AutoTamperResponseAfterDelegate?.Invoke(session);
        public void AutoTamperResponseBefore(Session session) => AutoTamperResponseBeforeDelegate?.Invoke(session);
        public void OnPeekAtResponseHeaders(Session session) => OnPeekAtResponseHeadersDelegate?.Invoke(session);
        public void OnPeekAtRequestHeaders(Session session) => OnPeekAtRequestHeadersDelegate?.Invoke(session);
        public void OnBeforeReturningError(Session session) => OnBeforeReturningErrorDelegate?.Invoke(session);

        public void OnBoot() => OnFiddlerBootDelegate?.Invoke();
        public void OnShutdown() => OnFiddlerShutdownDelegate?.Invoke();
        public bool? OnBeforeShutdown() => OnFiddlerBeforeShutdownDelegate?.Invoke();
        public void OnAttach() => OnFiddlerAttachDelegate?.Invoke();
        public void OnDetach() => OnFiddlerDetachDelegate?.Invoke();

        public override int GetHashCode() => _path.GetHashCode();
    }
}
