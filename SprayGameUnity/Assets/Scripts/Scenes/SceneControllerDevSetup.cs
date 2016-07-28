using UnityEngine;
using System.Collections;

public class SceneControllerDevSetup: SceneController_Base 
{
	#region inspector hooks

	public UnityEngine.UI.Text versionText;

	#endregion inspector hooks

	#region event handlers

	public void HandleGameButtonPressed( )
	{
		SceneManager.Instance.SwitchScene( SceneManager.EScene.Game );
	}

	public void HandleQuitButtonPressed( )
	{
		Application.Quit();
	}

	#endregion event handlers

	#region SceneController_Base

	override public SceneManager.EScene Scene ()
	{
		return SceneManager.EScene.DevSetup;
	}

	override protected void PostStart()
	{
		versionText.text = RJWard.Core.Version.versionNumber.DebugDescribe( );
	}

	override protected void PostAwake()
	{
	}

#endregion SceneController_Base

}
