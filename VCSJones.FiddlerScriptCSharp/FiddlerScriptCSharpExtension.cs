using System.Collections.Generic;
using System.Linq;
using Fiddler;

namespace VCSJones.FiddlerScriptCSharp
{
    public class FiddlerScriptCSharpExtension : IAutoTamper3
    {
        private readonly List<FiddlerScriptRepository> _roslynScriptRepositories;

        public void AutoTamperRequestAfter(Session oSession) => _roslynScriptRepositories.ForEach(r => r.ExecuteAllAutoTamperRequestAfter(oSession));
        public void AutoTamperRequestBefore(Session oSession) => _roslynScriptRepositories.ForEach(r => r.ExecuteAllAutoTamperRequestBefore(oSession));
        public void AutoTamperResponseAfter(Session oSession) => _roslynScriptRepositories.ForEach(r => r.ExecuteAllAutoTamperResponseAfter(oSession));
        public void AutoTamperResponseBefore(Session oSession) => _roslynScriptRepositories.ForEach(r => r.ExecuteAllAutoTamperResponseBefore(oSession));
        public void OnPeekAtRequestHeaders(Session oSession) => _roslynScriptRepositories.ForEach(r => r.ExecuteAllOnPeekAtRequestHeaders(oSession));
        public void OnPeekAtResponseHeaders(Session oSession) => _roslynScriptRepositories.ForEach(r => r.ExecuteAllOnPeekAtResponseHeaders(oSession));
        public void OnBeforeReturningError(Session oSession) => _roslynScriptRepositories.ForEach(r => r.ExecuteAllOnBeforeReturningError(oSession));

        public void OnBeforeUnload()
        {
        }

        public void OnLoad()
        {
        }

        public FiddlerScriptCSharpExtension()
        {
            var scriptPath = CONFIG.GetPath("Scripts");
            _roslynScriptRepositories = new List<FiddlerScriptRepository> {
                new FiddlerScriptRepository(new CSharpScriptEngine(), scriptPath),
                new FiddlerScriptRepository(new VisualBasicScriptEngine(), scriptPath)
            };
            FiddlerApplication.FiddlerBoot += () => _roslynScriptRepositories.ForEach(r => r.ExecuteAllOnBoot());
            FiddlerApplication.FiddlerShutdown += () => _roslynScriptRepositories.ForEach(r => r.ExecuteAllOnShutdown());
            FiddlerApplication.BeforeFiddlerShutdown += (_, cancel) => cancel.Cancel = !_roslynScriptRepositories.Aggregate(true, (v, r) => v & r.ExecuteAllOnBeforeShutdown());
            FiddlerApplication.FiddlerAttach += () => _roslynScriptRepositories.ForEach(r => r.ExecuteAllOnAttach());
            FiddlerApplication.FiddlerDetach += () => _roslynScriptRepositories.ForEach(r => r.ExecuteAllOnDetach());
            FiddlerApplication.AfterSessionComplete += session => _roslynScriptRepositories.ForEach(r => r.ExecuteAllOnDone(session));
            FiddlerApplication.OnWebSocketMessage += (_, args) => _roslynScriptRepositories.ForEach(r => r.ExecuteAllOnWebSocketMessage(args.oWSM));
        }
    }
}
