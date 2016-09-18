using UnityEngine;
using System.Collections;

public class SceneControllerGame : SceneController_Base 
{
	//	static private readonly bool DEBUG_LOCAL = false;

	#region inspector hooks

	public GameSettingsPanel gameSettingsPanel;
	public GameObject settingsButton;
	public UnityEngine.UI.Button playButton;

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
		gameSettingsPanel.gameObject.SetActive( false );
		settingsButton.SetActive( false );
		playButton.interactable = false;
	}

	override protected void PostAwake()
	{
	}

	protected override void OnDatabasesLoaded( )
	{
		SqliteUtils.Instance.databaseLoadComplete -= OnDatabasesLoaded;
		StartCoroutine( OnDBSLoadedCR( ) );
	}

	private IEnumerator OnDBSLoadedCR()
	{
		while (false == GameManager.IsInitialised())
		{
			Debug.Log( "Waiting for GM" );
			yield return new WaitForSeconds( 0.2f );
		}
		GameManager.Instance.Init( );
		settingsButton.SetActive( true );
		playButton.interactable = true;
	}

	#endregion SceneController_Base

	public void HandleBackButton()
	{
		Time.timeScale = 1f;
		SceneManager.Instance.SwitchScene( SceneManager.EScene.DevSetup );
	}

	public void HandleSettingsButton()
	{
		Time.timeScale = 1f;
		gameSettingsPanel.Init( );
	}
}

