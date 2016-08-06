using UnityEngine;
using System.Collections;

[System.Serializable]
public class LevelSettings
{
	public float minPending = 4;
	public int numBlobs = 4;
	public float groupDeleteCountdown = 6f;
	public float topWallAnimSpeed = 1f;
	public float topWallDistPerBlob = 0.005f;

}
