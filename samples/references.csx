#r "System.Drawing"
#r "System.Windows.Forms"
using System.Windows.Forms;

public bool OnBeforeShutdown()
{
	return MessageBox.Show("Are you sure you want to exit?", "FiddlerScript C#",  MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
}

public void Main()
{
	FiddlerObject.log("Script has been initialized in Main.");
}

//#equivalent to "Main"
FiddlerObject.log("Script has been initialized.");