using Fiddler;

namespace VCSJones.FiddlerScriptCSharp
{
    public class FiddlerScriptCSharpExtension : IAutoTamper3
    {
        private ScriptCSharpRepository _scriptRepository;

        public void AutoTamperRequestAfter(Session oSession) => _scriptRepository.ExecuteAllAutoTamperRequestAfter(oSession);
        public void AutoTamperRequestBefore(Session oSession) => _scriptRepository.ExecuteAllAutoTamperRequestBefore(oSession);
        public void AutoTamperResponseAfter(Session oSession) => _scriptRepository.ExecuteAllAutoTamperResponseAfter(oSession);
        public void AutoTamperResponseBefore(Session oSession) => _scriptRepository.ExecuteAllAutoTamperResponseBefore(oSession);
        public void OnPeekAtRequestHeaders(Session oSession) => _scriptRepository.ExecuteAllOnPeekAtRequestHeaders(oSession);
        public void OnPeekAtResponseHeaders(Session oSession) => _scriptRepository.ExecuteAllOnPeekAtResponseHeaders(oSession);
        public void OnBeforeReturningError(Session oSession) => _scriptRepository.ExecuteAllOnBeforeReturningError(oSession);

        public void OnBeforeUnload()
        {
        }

        public void OnLoad()
        {
        }

        public FiddlerScriptCSharpExtension()
        {
            _scriptRepository = new ScriptCSharpRepository(CONFIG.GetPath("Scripts"));
            FiddlerApplication.FiddlerBoot += () => _scriptRepository.ExecuteAllOnBoot();
            FiddlerApplication.FiddlerShutdown += () => _scriptRepository.ExecuteAllOnShutdown();
            FiddlerApplication.BeforeFiddlerShutdown += (_, __) => _scriptRepository.ExecuteAllOnBeforeShutdown();
            FiddlerApplication.FiddlerAttach += () => _scriptRepository.ExecuteAllOnAttach();
            FiddlerApplication.FiddlerDetach += () => _scriptRepository.ExecuteAllOnDetach();
        }
    }
}
