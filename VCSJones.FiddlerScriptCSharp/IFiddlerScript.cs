using Fiddler;

namespace VCSJones.FiddlerScriptCSharp
{
    public interface IFiddlerScript
    {
        void Initialize();
        void AutoTamperRequestBefore(Session session);
        void AutoTamperRequestAfter(Session session);
        void AutoTamperResponseAfter(Session session);
        void AutoTamperResponseBefore(Session session);
        void OnPeekAtResponseHeaders(Session session);
        void OnPeekAtRequestHeaders(Session session);
        void OnBeforeReturningError(Session session);
        void OnDone(Session session);
        void OnBoot();
        void OnShutdown();
        bool? OnBeforeShutdown();
        void OnAttach();
        void OnDetach();
        void OnWebSocketMessage(WebSocketMessage message);
    }
}