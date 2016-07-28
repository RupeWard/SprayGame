using UnityEngine;
using System.Collections;

public class GameSettingsPanel : MonoBehaviour
{
	public UnityEngine.UI.Text speedButtonText;
	public UnityEngine.UI.Text titleText;
	public UnityEngine.UI.Text messageText;

	public FloatSettingPanel floatSettingPanel;

	public void Init()
	{
		SetSpeedText( );
		titleText.text = "Game Options";
		floatSettingPanel.gameObject.SetActive( false );
		gameObject.SetActive( true );
		GameManager.Instance.PauseGame( );
	}

	private void SetSpeedText()
	{
		speedButtonText.text = "Speed: " + GameManager.Instance.cannon.force.ToString();
	}

	public void HandleDoneButton()
	{
		GameManager.Instance.ResumeGame( );
		gameObject.SetActive( false );
	}

	public void HandleSpeedButton()
	{
		floatSettingPanel.Init( "Speed", GameManager.Instance.cannon.force, new Vector2( 0f, 40f), OnSpeedChanged);
	}

	public void OnSpeedChanged( float f)
	{
		{
			SettingsStore.storeSetting( SettingsIds.cannonSpeed, f );
			GameManager.Instance.cannon.force = f;
			SetSpeedText( );
		}
	}

}

