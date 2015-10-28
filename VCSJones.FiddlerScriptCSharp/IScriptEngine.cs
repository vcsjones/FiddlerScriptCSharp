namespace VCSJones.FiddlerScriptCSharp
{
    public interface IScriptEngine
    {
        string Extension { get; }
        IFiddlerScript CreateScript(string path);
    }
}