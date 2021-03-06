using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;

namespace CppCheckAddIn
  {
  /// <summary>The object for implementing an Add-in.</summary>
  /// <seealso class='IDTExtensibility2' />
  public class Connect : IDTExtensibility2, IDTCommandTarget
    {
    /// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
    public Connect()
      {
      }

    /// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
    /// <param term='application'>Root object of the host application.</param>
    /// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
    /// <param term='addInInst'>Object representing this Add-in.</param>
    /// <seealso class='IDTExtensibility2' />
    public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
      {
      _applicationObject = (DTE2)application;
      _addInInstance = (AddIn)addInInst;

      InitializeAddIn();
      
      switch (connectMode)
        {
        case ext_ConnectMode.ext_cm_UISetup:
          break;
        case ext_ConnectMode.ext_cm_Startup:
          break;
        case ext_ConnectMode.ext_cm_AfterStartup:
          mUIHandler.SetupUI();
          break;
        }
      }

    private void InitializeAddIn()
      {
      mOutputHandler = new OutputHandler(_applicationObject);
      mErrorHandler = new ErrorHandler(_applicationObject);
      mUIHandler = new UIHandler(_applicationObject, _addInInstance);
      mSolutionParser = new SolutionParser(_applicationObject);
      }

    /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
    /// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
    /// <param term='custom'>Array of parameters that are host application specific.</param>
    /// <seealso class='IDTExtensibility2' />
    public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
      {
      switch (disconnectMode)
        {
        case ext_DisconnectMode.ext_dm_HostShutdown:
          mUIHandler.ClearUI();
          break;
        default:
          break;
        }
      }

    /// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
    /// <param term='custom'>Array of parameters that are host application specific.</param>
    /// <seealso class='IDTExtensibility2' />		
    public void OnAddInsUpdate(ref Array custom)
      {
      }

    /// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
    /// <param term='custom'>Array of parameters that are host application specific.</param>
    /// <seealso class='IDTExtensibility2' />
    public void OnStartupComplete(ref Array custom)
      {
      mUIHandler.SetupUI();
      }

    /// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
    /// <param term='custom'>Array of parameters that are host application specific.</param>
    /// <seealso class='IDTExtensibility2' />
    public void OnBeginShutdown(ref Array custom)
      {
      }

    /// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
    /// <param term='commandName'>The name of the command to determine state for.</param>
    /// <param term='neededText'>Text that is needed for the command.</param>
    /// <param term='status'>The state of the command in the user interface.</param>
    /// <param term='commandText'>Text requested by the neededText parameter.</param>
    /// <seealso class='Exec' />
    public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
      {
      if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
        {
        foreach(string cmdName in mUIHandler.CommandNames)
          if (commandName == "CppCheckAddIn.Connect." + cmdName)
            {
            status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
            return;
            }
        }
      }

    /// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
    /// <param term='commandName'>The name of the command to execute.</param>
    /// <param term='executeOption'>Describes how the command should be run.</param>
    /// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
    /// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
    /// <param term='handled'>Informs the caller if the command was handled or not.</param>
    /// <seealso class='Exec' />
    public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
      {
      handled = false;
      if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
        {
        // Tools -> Check Solution
        if (commandName == "CppCheckAddin.Connect." + mUIHandler.CommandNames[0])
          {

          }
        // Solution explorer : file -> contect menu -> CppCheck it
        else if (commandName == "CppCheckAddIn.Connect." + mUIHandler.CommandNames[6])
          {

          mSolutionParser.ParseSelection(SolutionParser.ESelectionKind.selectionKindItem);

          mOutputHandler.OutputMessage(mSolutionParser.PathArgument);

          handled = true;
          }
        }
      }

    private void OutputLineReceivedHandler(object iSender, string iMessage)
      {
      mOutputHandler.OutputMessage("---> " + iMessage);
      }

    private void ErrorLineReceivedHandler(object iSender, string iMessage)
      {
      Regex r = new Regex("^(?<file>.+):::(?<line>.+):::(?<message>.+)$");
      Match m = r.Match(iMessage);

      if (!m.Success || m.Groups.Count != 4)
        return;

      string file = m.Groups["file"].Value;
      string line = m.Groups["line"].Value;
      string message = m.Groups["message"].Value;

      //if (file == null || line == null || message == null)
      //  return;

      mOutputHandler.OutputMessage(file + "(" + line + "): warning: " + message);

      mErrorHandler.PostWarning(file, Int32.Parse(line)-1, message);
      }

    OutputHandler mOutputHandler;
    ErrorHandler mErrorHandler;
    UIHandler mUIHandler;
    SolutionParser mSolutionParser;

    private DTE2 _applicationObject;
    private AddIn _addInInstance;
    }
  }