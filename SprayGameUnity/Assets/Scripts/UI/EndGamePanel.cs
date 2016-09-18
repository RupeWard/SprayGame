using UnityEngine;
using System.Collections;

public class EndGamePanel : MonoBehaviour
{
	static private readonly bool DEBUG_LOCAL = true;

	public void HandleRestartButton()
	{
		if (DEBUG_LOCAL)
		{
			Debug.Log( "EndgamePanel: RestartButton" );
		}
		Time.timeScale = 1f;
		SceneManager.Instance.ReloadScene( SceneManager.EScene.Game );
	}

	public void HandleCloseButton()
	{
		if (DEBUG_LOCAL)
		{
			Debug.Log( "EndgamePanel: Close" );
		}
		Close( );
	}

	public void Init( )
	{
		if (DEBUG_LOCAL)
		{
			Debug.Log( "EndgamePanel: RestartButton" );
		}

		gameObject.SetActive( true );
	}

	public void Close( )
	{
		gameObject.SetActive( false );
	}

}
