using UnityEngine;
using System.Collections;

[System.Serializable]
public class LevelSettings
{
	public string name = "Default";
	public float minPending = 4;
	public int numBlobs = 4;
	public float groupDeleteCountdown = 6f;
	public float groupTypeChangeCountdown = 2f;
	public float topWallAnimSpeed = 1f;
	public float topWallDistPerBlob = 0.005f;
	public float topWallDistPerSecond = 0.002f;
	public float jointTolerance = 0.005f;

	public BlobTypeFrequency[] blobTypeFrequencies = new BlobTypeFrequency[0];

	[System.Serializable]
	public class BlobTypeFrequency
	{
		public string blobType = string.Empty;
		public float frequency = 0f;
	}

	public string GetRandomBlobTypeString()
	{
		float sum = 0f;
		foreach (BlobTypeFrequency f in blobTypeFrequencies)
		{
			sum += f.frequency;
		}
		float rand = UnityEngine.Random.Range( 0f, sum );
		foreach (BlobTypeFrequency f in blobTypeFrequencies)
		{
			if (rand <= f.frequency)
			{
				return f.blobType;
			}
			rand -= f.frequency;
		}
		Debug.LogWarning( "GetRandomBlobTypeString falling back to default" );
		return blobTypeFrequencies[0].blobType;
    }
}
