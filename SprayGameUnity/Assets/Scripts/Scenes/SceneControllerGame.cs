using UnityEngine;
using System.Collections;

public class SceneControllerGame : SceneController_Base 
{
//	static private readonly bool DEBUG_LOCAL = false;

	#region inspector hooks

	#endregion inspector hooks

	#region event handlers

	#endregion event handlers

	#region SceneController_Base

	override public SceneManager.EScene Scene ()
	{
		return SceneManager.EScene.Game;
	}

	override protected void PostStart()
	{
	}

	override protected void PostAwake()
	{
	}

#endregion SceneController_Base

	public void HandleBackButton()
	{
		SceneManager.Instance.SwitchScene( SceneManager.EScene.DevSetup );
	}
}

