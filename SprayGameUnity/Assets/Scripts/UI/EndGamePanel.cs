using UnityEngine;
using System.Collections;

public class EndGamePanel : MonoBehaviour
{
	public void HandleRestartButton()
	{
		SceneManager.Instance.ReloadScene( SceneManager.EScene.Game );
	}

	public void HandleCloseButton()
	{
		Close();
	}

	public void Init( )
	{
		gameObject.SetActive( true );
	}

	private void Close( )
	{
		gameObject.SetActive( false );
	}

	private void Awake()
	{
		Close( );
	}
}
