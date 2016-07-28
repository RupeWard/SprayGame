using UnityEngine;
using System.Collections;

public class GameManager : RJWard.Core.Singleton.SingletonSceneLifetime<GameManager>
{
	static readonly bool DEBUG_GAME = true;

	#region inspector hooks

	public Cannon cannon;

	#endregion inspector hooks

	#region private objects

	Controller_Base controller_ = null;

	#endregion private objects

	protected override void PostAwake( )
	{
		if (cannon == null)
		{
			Debug.LogError( "No cannon" );
		}

		if (Application.platform == RuntimePlatform.Android)
		{
			Debug.LogError( "Android not implemented" );
		}
		else
		{
			if (DEBUG_GAME)
			{
				Debug.Log( "Creating mouse controller" );
			}
			controller_ = Controller_Mouse.Create( cannon );
		}
	}

}
