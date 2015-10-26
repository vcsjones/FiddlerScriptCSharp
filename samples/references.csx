#r "System.Drawing"
#r "System.Windows.Forms"
using System.Windows.Forms;

public bool OnBeforeShutdown()
{
	return MessageBox.Show("Are you sure you want to exit?", "FiddlerScript C#",  MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
}

//#equivalent to JScript.NET Engine "main" 
FiddlerApplication.Log.LogString("Script has been initialized.");