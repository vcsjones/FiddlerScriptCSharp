using System.Windows.Forms;
using Fiddler;

namespace VCSJones.FiddlerScriptCSharp
{
    //Fiddler's "FiddlerObject" supports all of these methods. We cannot use them because they are glued to the JScript engine.
    public class FiddlerScriptObjectProxy
    {
        // ReSharper disable once InconsistentNaming
        public void alert(string message)
        {
            if (CONFIG.QuietMode)
                return;
            MessageBox.Show(message, "FiddlerScript C#", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        // ReSharper disable once InconsistentNaming
        public void utilIssueRequest(string request) => FiddlerApplication.oProxy.SendRequest(request, null);

        // ReSharper disable once InconsistentNaming
        public string prompt(string message, string defaultValue = "FiddlerScript C# Prompt", string windowTitle = "FiddlerScript C# Prompt") => frmPrompt.GetUserString(windowTitle, message, defaultValue);

        // ReSharper disable once InconsistentNaming
        public void playSound(string soundname) => Utilities.PlaySoundFile(soundname);

        // ReSharper disable once InconsistentNaming
        public void log(string sMessage) => FiddlerApplication.Log.LogString(sMessage);
    }
}