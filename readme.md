FiddlerScript C#
========

FiddlerScript C# is an extension for Telerik Fiddler that lets you use C# for scripting instead of the build-in JScript.NET.

FiddlerScript C# is powered by Roslyn Scripting.

![Screenshot](https://vcsjones.com/wp-content/uploads/Screen-Shot-2015-10-26-at-1.47.26-PM.png)

#Installation
To install, compile the project and copy the contents of the output into `~/Documents/Fiddler2/Scripts`.

#Use
Create a file with the `.crx` extension in `~/Documents/Fiddler2/Scripts` and begin writing code, that's it. You can create
as many scripts in this directory as you would like, however they cannot be guaranteed to run in any particular order. 
A simple example that alternates the colors of sessions in the user interface:

```csharp
using Fiddler;

public void OnBeforeRequest(Session oSession)
{
	oSession["ui-color"] = oSession.id % 2 == 0 ? "red" : "green";
}
FiddlerApplication.Log.LogString("Script has been initialized.");
```

That's all you need in the file. Unlike full C#, Roslyn scripting doesn't require a full class declaration.

#Comparison with JScript.NET Engine
Currently the FiddlerScript C# bindings are more limited than the JScript.NET ones, however some of the major features
have been implemented.

#Namespaces and assemblies
FiddlerScript C# automatically imports the following assemblies:

* mscorlib
* System
* System.Core
* Microsoft.CSharp
* Fiddler

Additional references can be supplied using the `#r` syntax. For example, at the top of your script:

```csharp
#r "System.Drawing"
#r "System.Windows.Forms"
using System.Windows.Forms;
```

Assemblies are searched for in the `~/Documents/Fiddler2/Scripts` directory and the installation directory
of Fiddler.

By default, the following namespaces are automatically imported:

* System
* Fiddler


#API

###OnBeforeRequest (AutoTamperRequestBefore)

```csharp
//The "AutoTamperRequestBefore" name is also acceptable
public void OnBeforeRequest(Session session)
{
	//Access the session before the client has sent the request
}
```


###OnAfterRequest (AutoTamperRequestAfter)

```csharp
//The "AutoTamperRequestAfter" name is also acceptable
public void OnAfterRequest(Session session)
{
	//Access the session after the request has been sent to the server
}
```
	
###OnBeforeResponse (AutoTamperResponseBefore)

```csharp
//The "AutoTamperResponseBefore" name is also acceptable
public void OnBeforeResponse(Session session)
{
	//Access the session after the request has been sent to the server but
	//before the client has received the response
}
```
	
###OnAfterResponse (AutoTamperResponseAfter)

```csharp
//The "AutoTamperResponseAfter" name is also acceptable
public void OnAfterResponse(Session session)
{
	//Access the session after the client has received the response.
}
```
	
###OnPeekAtRequestHeaders

```csharp
public void OnPeekAtRequestHeaders(Session session)
{
	//Access the request headers before the request body is available
}
```	

###OnPeekAtResponseHeaders

```csharp
public void OnPeekAtResponseHeaders(Session session)
{
	//Access the response headers before the response body is available
}
```

###OnReturningError (OnBeforeReturningError)

```csharp
//The "OnBeforeReturningError" name is also acceptable
public void OnReturningError(Session session)
{
	//Occures when Fiddler itself returns an error during the request / response.
}
```


###OnBeforeShutdown

```csharp
public bool OnBeforeShutdown()
{
	//Return false to prevent fiddler from closing
}
```

###Main

```csharp
public void Main()
{
	//Executed immediately after the script is loaded.
}
```

#Globals

FiddlerScript C# supports the same global properties that JScript.NET does.

###UI

Supports the same UI helpers that JScript.NET does, such as:

```csharp
UI.actRemoveAllSessions();
```

###FiddlerObject

Supports a subset of helpers that JScript.NET does, currently:

* `void alert(string message)`
* `void utilIssueRequest(string request)`
* `string prompt(string message, string defaultValue = "FiddlerScript C# Prompt", string windowTitle = "FiddlerScript C# Prompt")`
* `void playSound(string soundname)`
* `void log(string sMessage)`