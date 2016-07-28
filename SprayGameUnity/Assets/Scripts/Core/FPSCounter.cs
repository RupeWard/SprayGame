using UnityEngine;
using System.Collections.Generic;

namespace RJWard.Core.Unity
{
	public class FPSCounter : MonoBehaviour
	{
		public UnityEngine.UI.Text fpsText;
		public UnityEngine.UI.Text minMaxText;

		private Queue<float> intervals_ = new Queue<float>( );
		public int maxIntervals = 100;
		public int minIntervals = 50;
		public float accum = 0f;
		public int displayInterval = 40;
		private int sinceDisplay = 0;

		private Vector2 minMax = new Vector2( float.MaxValue, float.MinValue );
		private bool active_ = true;
		public void ToggleActive( )
		{
			SetActive( !active_ );
		}

		public void Reset()
		{
			intervals_.Clear();
			minMax = new Vector2( float.MaxValue, float.MinValue );
			if (fpsText != null)
			{
				fpsText.text = "FPS";
			}
			if (minMaxText != null)
			{
				minMaxText.text = "(range)";
			}
		}

		public void SetActive( bool b)
		{
			if (active_ != b)
			{
				active_ = b;
				if (!active_)
				{
					intervals_.Clear( );
					if (fpsText != null)
					{
						fpsText.text = "FPS";
					}
				}
			}
		}

		public System.Action<float> SendFPS;

		private void Awake()
		{
			SetActive( SettingsStore.retrieveSetting<bool>( SettingsIds.showFPS ) );
			MessageBus.instance.onShowFPSChanged += SetActive;
			MessageBus.instance.onResetFPS += Reset;
		}

		private void Update( )
		{
			if (active_ && Time.timeScale > 0f)
			{
				intervals_.Enqueue( Time.deltaTime );
				accum += Time.deltaTime;

				bool doMinMax = (minMax.x == float.MaxValue || minMax.y == float.MinValue);
				while (intervals_.Count > maxIntervals)
				{
					float f = intervals_.Dequeue( );
					accum -= f;
					if (!doMinMax && f >= minMax.x || f <= minMax.y)
					{
						doMinMax = true;
					}
				}
				if (doMinMax)
				{
					recomputeMinMax( );
				}
				if (intervals_.Count >= minIntervals)
				{
					sinceDisplay--;
					if (sinceDisplay < 0)
					{
						sinceDisplay = displayInterval;
						float meanDeltaTime = (accum / intervals_.Count);
						float fps = 1f / meanDeltaTime;
						if (SendFPS != null)
						{
							SendFPS( fps );
						}
						if (fpsText != null)
						{
							fpsText.text = fps.ToString( "F1" );
						}
					}
				}
				if (doMinMax)
				{
					if (minMaxText != null)
					{
						int low = Mathf.RoundToInt(1f / minMax.y);
						int high = Mathf.RoundToInt(1f / minMax.x);
						
                        minMaxText.text = "("+  low.ToString() + ", "+ high.ToString( )+")";
                    }
				}
			}

		}

		private void recomputeMinMax()
		{
			minMax.x = float.MaxValue;
			minMax.y = float.MinValue;
			foreach (float i in intervals_)
			{
				if (i < minMax.x)
				{
					minMax.x = i;
				}
				if (i > minMax.y)
				{
					minMax.y = i;
				}
			}
		}
	}



}

public partial class MessageBus : MonoBehaviour
{
	public System.Action<bool> onShowFPSChanged;
	public void dispatchOnShowFPSChanged( bool b )
	{
		if (onShowFPSChanged != null)
		{
			onShowFPSChanged( b );
		}
	}
	public System.Action onResetFPS;
	public void dispatchResetFPS( )
	{
		if (onResetFPS != null)
		{
			onResetFPS( );
		}
	}
}

