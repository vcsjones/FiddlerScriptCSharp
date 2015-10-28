namespace VCSJones.FiddlerScriptCSharp
{
    public class VisualBasicScriptEngine : IScriptEngine
    {
        public virtual string Extension { get; } = "*.vbx";
        public virtual IFiddlerScript CreateScript(string path)
        {
            return new FiddlerVisualBasicScript(path);
        }
    }
}