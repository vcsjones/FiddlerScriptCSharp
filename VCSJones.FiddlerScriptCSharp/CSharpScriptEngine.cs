namespace VCSJones.FiddlerScriptCSharp
{
    public class CSharpScriptEngine : IScriptEngine
    {
        public virtual string Extension { get; } = "*.csx";
        public virtual IFiddlerScript CreateScript(string path)
        {
            return new FiddlerCSharpScript(path);
        }
    }
}