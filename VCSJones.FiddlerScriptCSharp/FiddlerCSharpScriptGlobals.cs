using Fiddler;

namespace VCSJones.FiddlerScriptCSharp
{
    public class FiddlerCSharpSCriptGlobals
    {
        private readonly FiddlerScriptObjectProxy _proxyObject = new FiddlerScriptObjectProxy();
        public frmViewer UI { get; } = FiddlerApplication.UI;
        public FiddlerScriptObjectProxy FiddlerObject => _proxyObject;
        public FiddlerScriptObjectProxy FiddlerScript => _proxyObject;
    }
}