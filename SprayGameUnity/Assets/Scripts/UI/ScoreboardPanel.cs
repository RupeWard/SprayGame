using UnityEngine;
using System.Collections;

public class ScoreboardPanel : MonoBehaviour
{
	public UnityEngine.UI.Text scoreText;

	public void HandleScoreUpdate(string score)
	{
		scoreText.text = score;
	}

	public void InitForGame()
	{
		scoreText.text = "0";
	}
}
