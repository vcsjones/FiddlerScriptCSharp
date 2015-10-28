using System;
using System.IO;
using Fiddler;
using Microsoft.CodeAnalysis.Scripting;

namespace VCSJones.FiddlerScriptCSharp
{
    public abstract class FiddlerRoslynScriptBase : IFiddlerScript
    {
        private readonly string _path;
        private Action<Session> AutoTamperRequestBeforeDelegate;
        private Action<Session> AutoTamperRequestAfterDelegate;
        private Action<Session> AutoTamperResponseAfterDelegate;
        private Action<Session> AutoTamperResponseBeforeDelegate;
        private Action<Session> OnPeekAtRequestHeadersDelegate;
        private Action<Session> OnPeekAtResponseHeadersDelegate;
        private Action<Session> OnBeforeReturningErrorDelegate;
        private Action<Session> OnDoneDelegate;
        private Action OnFiddlerAttachDelegate;
        private Action OnFiddlerDetachDelegate;
        private Func<bool> OnFiddlerBeforeShutdownDelegate;
        private Action OnFiddlerBootDelegate;
        private Action OnFiddlerShutdownDelegate;
        private Action<WebSocketMessage> OnWebSocketMessageDelegate;

        protected FiddlerRoslynScriptBase(string path)
        {
            _path = path;
        }

        public void Initialize()
        {
            var options = ScriptOptions.Default
                .WithIsInteractive(false)
                .AddSearchPaths(CONFIG.GetPath("App"), CONFIG.GetPath("Scripts"))
                .AddReferences("mscorlib.dll", "System.dll", "System.Core.dll", "Microsoft.CSharp.dll", "Fiddler.exe")
                .AddNamespaces("Fiddler");
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
                script = ExecuteScript(text, options);
            }
            catch (Exception e)
            {
                FiddlerApplication.Log.LogString(e.ToString());
                return;
            }
            script.CreateDelegate<Action>("Main")?.Invoke();
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
            OnDoneDelegate = script.CreateDelegate<Action<Session>>("OnDone");
            OnWebSocketMessageDelegate = script.CreateDelegate<Action<WebSocketMessage>>("OnWebSocketMessage");
        }

        protected abstract ScriptState ExecuteScript(string text, ScriptOptions options);
        public void AutoTamperRequestBefore(Session session) => AutoTamperRequestBeforeDelegate?.Invoke(session);
        public void AutoTamperRequestAfter(Session session) => AutoTamperRequestAfterDelegate?.Invoke(session);
        public void AutoTamperResponseAfter(Session session) => AutoTamperResponseAfterDelegate?.Invoke(session);
        public void AutoTamperResponseBefore(Session session) => AutoTamperResponseBeforeDelegate?.Invoke(session);
        public void OnPeekAtResponseHeaders(Session session) => OnPeekAtResponseHeadersDelegate?.Invoke(session);
        public void OnPeekAtRequestHeaders(Session session) => OnPeekAtRequestHeadersDelegate?.Invoke(session);
        public void OnBeforeReturningError(Session session) => OnBeforeReturningErrorDelegate?.Invoke(session);
        public void OnDone(Session session) => OnDoneDelegate?.Invoke(session);
        public void OnBoot() => OnFiddlerBootDelegate?.Invoke();
        public void OnShutdown() => OnFiddlerShutdownDelegate?.Invoke();
        public bool? OnBeforeShutdown() => OnFiddlerBeforeShutdownDelegate?.Invoke();
        public void OnAttach() => OnFiddlerAttachDelegate?.Invoke();
        public void OnDetach() => OnFiddlerDetachDelegate?.Invoke();
        public void OnWebSocketMessage(WebSocketMessage message) => OnWebSocketMessageDelegate?.Invoke(message);
    }
}