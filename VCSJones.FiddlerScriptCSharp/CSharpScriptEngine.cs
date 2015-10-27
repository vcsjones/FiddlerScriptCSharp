namespace VCSJones.FiddlerScriptCSharp
{
    public class CSharpScriptEngine : ScriptEngineBase
    {
        public override string Extension { get; } = "*.csx";
        public override FiddlerRoslynScriptBase CreateScript(string path)
        {
            return new FiddlerCSharpScript(path);
        }
    }
}