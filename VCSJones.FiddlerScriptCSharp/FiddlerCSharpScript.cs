using System;
using System.IO;
using Fiddler;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;

namespace VCSJones.FiddlerScriptCSharp
{
    public class FiddlerCSharpScript : IEquatable<FiddlerCSharpScript>, IComparable<FiddlerCSharpScript>
    {
        private readonly string _path;
        private Action<Session> AutoTamperRequestBeforeDelegate, AutoTamperRequestAfterDelegate, AutoTamperResponseAfterDelegate, AutoTamperResponseBeforeDelegate;
        private Action<Session> OnPeekAtRequestHeadersDelegate, OnPeekAtResponseHeadersDelegate;
        private Action<Session> OnBeforeReturningErrorDelegate, OnDoneDelegate;
        private Action OnFiddlerAttachDelegate, OnFiddlerDetachDelegate;
        private Func<bool> OnFiddlerBeforeShutdownDelegate;
        private Action OnFiddlerBootDelegate, OnFiddlerShutdownDelegate;
        private Action<WebSocketMessage> OnWebSocketMessageDelegate;

        public FiddlerCSharpScript(string path)
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
                script = CSharpScript.Run(text, options, new FiddlerCSharpSCriptGlobals());
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

        public int CompareTo(FiddlerCSharpScript other) => StringComparer.OrdinalIgnoreCase.Compare(_path, other._path);

        public bool Equals(FiddlerCSharpScript other) => CompareTo(other) == 0;

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

        public override int GetHashCode() => _path.GetHashCode();
    }
}