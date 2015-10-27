using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.VisualBasic;

namespace VCSJones.FiddlerScriptCSharp
{
    public class FiddlerVisualBasicScript : FiddlerRoslynScriptBase
    {
        public FiddlerVisualBasicScript(string path) : base(path)
        {
            
        }

        protected override ScriptState ExecuteScript(string text, ScriptOptions options)
        {
            return VisualBasicScript.Run(text, options, new FiddlerScriptGlobals());
        }
    }
}