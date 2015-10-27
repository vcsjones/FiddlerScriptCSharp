namespace VCSJones.FiddlerScriptCSharp
{
    public class VisualBasicScriptEngine : ScriptEngineBase
    {
        public override string Extension { get; } = "*.vbx";
        public override FiddlerRoslynScriptBase CreateScript(string path)
        {
            return new FiddlerVisualBasicScript(path);
        }
    }
}