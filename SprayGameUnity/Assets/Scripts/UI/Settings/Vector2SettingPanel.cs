using UnityEngine;
using System.Collections;

public class Vector2SettingPanel : MonoBehaviour
{
	#region inspector hooks

	public UnityEngine.UI.InputField xInputField;
	public UnityEngine.UI.InputField yInputField;
	public UnityEngine.UI.Text titleText;
	public UnityEngine.UI.Text messageText;
	public UnityEngine.UI.Text minText;
	public UnityEngine.UI.Text maxText;

	#endregion inspector hooks

	#region inspector data

	public float messageDuration = 5f;

	#endregion inspector data

	#region actions

	public System.Action<Vector2> onValueChangedAction;
	
	#endregion actions

	#region private data

	private Vector2 oldSetting_;
	private Vector2 currentSetting_;
	private Vector2 range_ = new Vector2( float.MinValue, float.MaxValue );

	#endregion private data

	private void Awake()
	{
		gameObject.SetActive( false );
	}

	public void Init( string title, Vector2 current, Vector2 range, System.Action<Vector2> changeAction)
	{
		titleText.text = title;
		oldSetting_ = current;
		currentSetting_ = current;
		range_ = range;

		onValueChangedAction = changeAction;
	
		if (range.x == float.MinValue)
		{
			minText.gameObject.SetActive( false );
		}
		else
		{
			minText.gameObject.SetActive( true);
			minText.text = range.x.ToString( );
		}

		if (range.y == float.MaxValue)
		{
			maxText.gameObject.SetActive( false );
		}
		else
		{
			maxText.gameObject.SetActive( true );
			maxText.text = range.y.ToString( );
		}

		messageText.text = string.Empty;
		messageText.gameObject.SetActive( false );

		SetValue( oldSetting_ );

		gameObject.SetActive( true );
	}

	private void SetValue(Vector2 v)
	{
		xInputField.text = v.x.ToString( );
		yInputField.text = v.y.ToString( );
	}

	private void SetMessage(string m)
	{
		SetMessage( m, messageDuration );
	}

	private void SetMessage(string m, float delay)
	{
		StartCoroutine( SetMessageCR( m, delay) );
	}

	private IEnumerator SetMessageCR(string m, float delay)
	{
		messageText.gameObject.SetActive( true );
		messageText.text = m;

		System.DateTime startTime = System.DateTime.UtcNow;
		float elapsed = 0f;
		do
		{
			System.TimeSpan span = System.DateTime.UtcNow - startTime;
			elapsed = (float)span.TotalSeconds;
			yield return null;
		} while (elapsed < delay);
		messageText.gameObject.SetActive( false );
	}

	#region handlers

	public void OnXInputFieldEndEdit(string s)
	{
		float f;
		if (float.TryParse(xInputField.text, out f))
		{
			if (range_.x != float.MinValue && f < range_.x)
			{
				SetMessage( "Too low!" );
				SetValue( currentSetting_);
			}
			else if (f > currentSetting_.y)
			{
				SetMessage( "Too high!" );
				SetValue( currentSetting_);
			}
			else
			{
				currentSetting_.x = f;
				if (onValueChangedAction != null)
				{
					onValueChangedAction( currentSetting_ );
				}
				SetValue( currentSetting_ );
			}
		}
		else
		{
			SetMessage( "Not a number!" );
			SetValue( currentSetting_);
		}
	}

	public void OnYInputFieldEndEdit( string s )
	{
		float f;
		if (float.TryParse( yInputField.text, out f ))
		{
			if (range_.y != float.MaxValue && f > range_.y)
			{
				SetMessage( "Too High!" );
				SetValue( currentSetting_);
			}
			else if (f < currentSetting_.x)
			{
				SetMessage( "Too low!" );
				SetValue( currentSetting_ );
			}
			else
			{
				currentSetting_.y = f;
				if (onValueChangedAction != null)
				{
					onValueChangedAction( currentSetting_ );
				}
				SetValue( currentSetting_ );
			}
		}
		else
		{
			SetMessage( "Not a number!" );
			SetValue( currentSetting_ );
		}
	}


	public void onDoneButtonClicked()
	{
		gameObject.SetActive( false );
	}

	public void onResetButtonClicked( )
	{
		SetValue( oldSetting_ );
		if (onValueChangedAction != null)
		{
			onValueChangedAction( oldSetting_ );
		}
	}

	#endregion handlers

	#region factory

	static public Vector2SettingPanel CreateFromRefab()
	{
		return Resources.Load<GameObject>( "Prefabs/UI/Vector2SettingPanel" ).GetComponent<Vector2SettingPanel>( );
	}
	#endregion factory

}
