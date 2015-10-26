using Fiddler;

public void AutoTamperRequestBefore(Session oSession)
{
	oSession["ui-color"] = "red";
}

public void OnBeforeResponse(Session oSession)
{
}

FiddlerApplication.Log.LogString("Script has been initialized.");
