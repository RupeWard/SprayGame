using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameSettingsPanel : MonoBehaviour
{
	public UnityEngine.UI.Text titleText;
	public UnityEngine.UI.Text messageText;

	public UnityEngine.UI.Text speedButtonText;
	public UnityEngine.UI.Text blobSlowDistanceButtonText;
	public UnityEngine.UI.Text blobSlowFactorButtonText;
	public UnityEngine.UI.Text numBlobsButtonText;
	public UnityEngine.UI.Text showConnectorsButtonText;
	public UnityEngine.UI.Text showFPSButtonText;

	public UnityEngine.UI.Text modeButtonText;

	public FloatSettingPanel floatSettingPanel;
	public IntSettingPanel intSettingPanel;

	public GameObject debugPanel;
	public GameObject environmentPanel;
	public GameObject levelPanel;

	private Dictionary<EMode, GameObject> modePanels_ = null;
	private Dictionary< EMode, GameObject> modePanels
	{
		get
		{
			if (modePanels_ == null)
			{
				modePanels_ = new Dictionary<EMode, GameObject>( )
				{
					{ EMode.Debug, debugPanel },
					{ EMode.Env, environmentPanel },
					{ EMode.Level, levelPanel },
				};
			}
			return modePanels_;
		}
	}
	private enum EMode
	{
		Env,
		Debug,
		Level
	}
	private EMode mode_ = EMode.Debug;

	private void Awake()
	{
	}

	private void SetUpForMode()
	{
		titleText.text = mode_.ToString( ) + " Settings";
		foreach (KeyValuePair<EMode, GameObject> kvp in modePanels)
		{
			kvp.Value.SetActive( kvp.Key == mode_ );
		}
		modeButtonText.text = mode_.ToString( );
	}

	public void HandleModeButton()
	{
		switch (mode_)
		{
			case EMode.Debug:
				mode_ = EMode.Env;
				break;
			case EMode.Env:
				mode_ = EMode.Level;
				break;
			case EMode.Level:
				mode_ = EMode.Debug;
				break;
		}
		SetUpForMode( );
	}

	public void Init()
	{
		SetUpForMode( );
		SetupShowConnectorsButton( );
		SetupShowFPSButton( );
		SetSpeedText( );
		SetBlobSlowDistanceText( );
		SetBlobSlowFactorText( );
		SetNumBlobsText( );
	
		titleText.text = "Game Options";
		floatSettingPanel.gameObject.SetActive( false );
		intSettingPanel.gameObject.SetActive( false );
		gameObject.SetActive( true );
		if (GameManager.Instance.isPlaying && !GameManager.Instance.isPaused)
		{
			GameManager.Instance.PauseGame( );
		}
	}

	private void SetNumBlobsText( )
	{
		numBlobsButtonText.text = "NumBlobs: " + GameManager.Instance.levelSettings.numBlobs.ToString( );
	}

	private void SetSpeedText()
	{
		speedButtonText.text = "Speed: " + GameManager.Instance.cannon.force.ToString();
	}

	private void SetBlobSlowDistanceText( )
	{
		blobSlowDistanceButtonText.text = "Dist: " + GameManager.Instance.gameWorldSettings.blobSlowDistance.ToString( );
	}

	private void SetBlobSlowFactorText( )
	{
		blobSlowFactorButtonText.text = "Fact: " + GameManager.Instance.gameWorldSettings.blobSlowFactor.ToString( );
	}

	public void HandleDoneButton()
	{
		gameObject.SetActive( false );
	}

	public void HandleNumBlobsButton( )
	{
		intSettingPanel.Init( "Num Blobs", GameManager.Instance.levelSettings.numBlobs, new int[] { 0, 10 }, OnNumBlobsChanged );
	}

	public void HandleSpeedButton()
	{
		floatSettingPanel.Init( "Speed", GameManager.Instance.cannon.force, new Vector2( 0f, 40f), OnSpeedChanged);
	}

	public void HandleBlobSlowDistanceButton( )
	{
		floatSettingPanel.Init( "Blob Slow Distance", GameManager.Instance.gameWorldSettings.blobSlowDistance, new Vector2( 0f, 10f ), OnBlobSlowDistanceChanged );
	}

	public void HandleBlobSlowFactorButton( )
	{
		floatSettingPanel.Init( "Blob Slow Factor", GameManager.Instance.gameWorldSettings.blobSlowFactor, new Vector2( 0f, 10f ), OnBlobSlowFactorChanged );
	}

	public void OnNumBlobsChanged( int i)
	{
		{
			SettingsStore.storeSetting( SettingsIds.numBlobs, i );
			GameManager.Instance.levelSettings.numBlobs = i;
			SetNumBlobsText( );
		}
	}

	public void OnSpeedChanged( float f)
	{
		{
			SettingsStore.storeSetting( SettingsIds.cannonSpeed, f );
			GameManager.Instance.cannon.force = f;
			SetSpeedText( );
		}
	}

	public void OnBlobSlowDistanceChanged( float f )
	{
		{
			SettingsStore.storeSetting( SettingsIds.blobSlowDistance, f );
			GameManager.Instance.gameWorldSettings.blobSlowDistance = f;
			SetBlobSlowDistanceText( );
		}
	}

	public void OnBlobSlowFactorChanged( float f )
	{
		{
			SettingsStore.storeSetting( SettingsIds.blobSlowFactor, f );
			GameManager.Instance.gameWorldSettings.blobSlowFactor = f;
			SetBlobSlowFactorText( );
		}
	}

	public void HandleShowConnectorsButton()
	{
		GameManager.Instance.ToggleShowConnectors( );
		SetupShowConnectorsButton( );
	}

	private void SetupShowConnectorsButton()
	{
		if (GameManager.Instance.showConnectors)
		{
			showConnectorsButtonText.text = "Hide connectors";
		}
		else
		{
			showConnectorsButtonText.text = "Show connectors";
		}
	}

	public void HandleShowFPSButton( )
	{
		GameManager.Instance.ToggleShowFPS( );
		SetupShowFPSButton( );
	}

	private void SetupShowFPSButton( )
	{
		if (GameManager.Instance.showFPS)
		{
			showFPSButtonText.text = "Hide FPS";
		}
		else
		{
			showFPSButtonText.text = "Show FPS";
		}
	}

}

