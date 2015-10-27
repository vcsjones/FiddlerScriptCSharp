using System;
using System.IO;
using Fiddler;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;

namespace VCSJones.FiddlerScriptCSharp
{
    public class FiddlerCSharpScript : FiddlerRoslynScriptBase
    {
        public FiddlerCSharpScript(string path) : base(path)
        {
        }

        protected override ScriptState ExecuteScript(string text, ScriptOptions options)
        {
            return CSharpScript.Run(text, options, new FiddlerScriptGlobals());
        }
    }
}