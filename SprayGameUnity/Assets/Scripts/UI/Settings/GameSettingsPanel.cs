using UnityEngine;
using System.Collections;

public class GameSettingsPanel : MonoBehaviour
{
	public UnityEngine.UI.Text titleText;
	public UnityEngine.UI.Text messageText;

	public UnityEngine.UI.Text speedButtonText;
	public UnityEngine.UI.Text blobSlowDistanceButtonText;
	public UnityEngine.UI.Text blobSlowFactorButtonText;
	public UnityEngine.UI.Text numBlobsButtonText;

	public FloatSettingPanel floatSettingPanel;
	public IntSettingPanel intSettingPanel;

	public void Init()
	{
		SetSpeedText( );
		SetBlobSlowDistanceText( );
		SetBlobSlowFactorText( );
		SetNumBlobsText( );
		titleText.text = "Game Options";
		floatSettingPanel.gameObject.SetActive( false );
		intSettingPanel.gameObject.SetActive( false );
		gameObject.SetActive( true );
		GameManager.Instance.PauseGame( );
	}

	private void SetNumBlobsText( )
	{
		numBlobsButtonText.text = "NumBlobs: " + GameManager.Instance.numBlobs.ToString( );
	}

	private void SetSpeedText()
	{
		speedButtonText.text = "Speed: " + GameManager.Instance.cannon.force.ToString();
	}

	private void SetBlobSlowDistanceText( )
	{
		blobSlowDistanceButtonText.text = "Dist: " + GameManager.Instance.blobSlowDistance.ToString( );
	}

	private void SetBlobSlowFactorText( )
	{
		blobSlowFactorButtonText.text = "Fact: " + GameManager.Instance.blobSlowFactor.ToString( );
	}

	public void HandleDoneButton()
	{
		GameManager.Instance.ResumeGame( );
		gameObject.SetActive( false );
	}

	public void HandleNumBlobsButton( )
	{
		intSettingPanel.Init( "Num Blobs", GameManager.Instance.numBlobs, new int[] { 0, 10 }, OnNumBlobsChanged );
	}

	public void HandleSpeedButton()
	{
		floatSettingPanel.Init( "Speed", GameManager.Instance.cannon.force, new Vector2( 0f, 40f), OnSpeedChanged);
	}

	public void HandleBlobSlowDistanceButton( )
	{
		floatSettingPanel.Init( "Blob Slow Distance", GameManager.Instance.blobSlowDistance, new Vector2( 0f, 10f ), OnBlobSlowDistanceChanged );
	}

	public void HandleBlobSlowFactorButton( )
	{
		floatSettingPanel.Init( "Blob Slow Factor", GameManager.Instance.blobSlowFactor, new Vector2( 0f, 10f ), OnBlobSlowFactorChanged );
	}

	public void OnNumBlobsChanged( int i)
	{
		{
			SettingsStore.storeSetting( SettingsIds.numBlobs, i );
			GameManager.Instance.numBlobs = i;
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
			GameManager.Instance.blobSlowDistance = f;
			SetBlobSlowDistanceText( );
		}
	}

	public void OnBlobSlowFactorChanged( float f )
	{
		{
			SettingsStore.storeSetting( SettingsIds.blobSlowFactor, f );
			GameManager.Instance.blobSlowFactor = f;
			SetBlobSlowFactorText( );
		}
	}


}

