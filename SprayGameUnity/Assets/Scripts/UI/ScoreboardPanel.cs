using UnityEngine;
using System.Collections;

public class ScoreboardPanel : MonoBehaviour
{
	public UnityEngine.UI.Text scoreText;
	public UnityEngine.UI.Text timerText;

	public void HandleScoreUpdate(string score)
	{
		scoreText.text = score;
	}

	public void InitForGame()
	{
		scoreText.text = "0";
	}

	public void HandleTimerUpdate( string time )
	{
		timerText.text = time;
	}

}
