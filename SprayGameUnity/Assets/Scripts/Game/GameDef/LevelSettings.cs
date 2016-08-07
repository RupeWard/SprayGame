using UnityEngine;
using System.Collections;

[System.Serializable]
public class LevelSettings
{
	public string name= "Default";
	public float minPending = 4;
	public int numBlobs = 4;
	public float groupDeleteCountdown = 6f;
	public float topWallAnimSpeed = 1f;
	public float topWallDistPerBlob = 0.005f;
	public float topWallDistPerSecond = 0.002f;
	public float jointTolerance = 0.005f;
	public string[] blobTypes = new string[0];
}
