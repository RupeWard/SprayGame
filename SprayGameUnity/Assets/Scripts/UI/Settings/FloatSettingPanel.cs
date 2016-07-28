using UnityEngine;
using System.Collections;

public class FloatSettingPanel : MonoBehaviour
{
	#region inspector hooks

	public UnityEngine.UI.InputField inputField;
	public UnityEngine.UI.Text titleText;
	public UnityEngine.UI.Text messageText;
	public UnityEngine.UI.Text minText;
	public UnityEngine.UI.Text maxText;

	#endregion inspector hooks

	#region inspector data

	public float messageDuration = 5f;

	#endregion inspector data

	#region actions

	public System.Action<float> onValueChangedAction;
	
	#endregion actions

	#region private data

	private float oldSetting_;
	private float currentSetting_;
	private Vector2 range_ = new Vector2( float.MinValue, float.MaxValue );

	#endregion private data

	private void Awake()
	{
		gameObject.SetActive( false );
	}

	public void Init( string title, float current, Vector2 range, System.Action<float> changeAction)
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

	private void SetValue(float f)
	{
		inputField.text = f.ToString( );
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

	public void OnInputFieldEndEdit(string s)
	{
		float f;
		if (float.TryParse(inputField.text, out f))
		{
			if (range_.x != float.MinValue && f < range_.x)
			{
				SetMessage( "Too low!" );
				SetValue( currentSetting_);
			}
			else if (range_.y != float.MaxValue && f > range_.y)
			{
				SetMessage( "Too high!" );
				SetValue( currentSetting_);
			}
			else
			{
				currentSetting_ = f;
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

	public void onDoneButtonClicked()
	{
		gameObject.SetActive( false );
	}

	public void onResetButtonClicked( )
	{
		currentSetting_ = oldSetting_;
		SetValue( oldSetting_ );
		if (onValueChangedAction != null)
		{
			onValueChangedAction( oldSetting_ );
		}
	}

	#endregion handlers

	#region factory

	static public FloatSettingPanel CreateFromPrefab()
	{
		return Resources.Load<GameObject>( "Prefabs/UI/FloatSettingPanel" ).GetComponent<FloatSettingPanel>( );
	}
	#endregion factory

}
