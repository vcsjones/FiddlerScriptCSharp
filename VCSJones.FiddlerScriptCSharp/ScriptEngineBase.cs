namespace VCSJones.FiddlerScriptCSharp
{
    public abstract class ScriptEngineBase
    {
        public abstract string Extension { get; }
        public abstract FiddlerRoslynScriptBase CreateScript(string path);
    }
}