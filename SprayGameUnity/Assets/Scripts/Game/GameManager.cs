using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : RJWard.Core.Singleton.SingletonSceneLifetime<GameManager>
{
	static readonly bool DEBUG_GAME = true;

	#region inspector data

	public BlobType[] blobTypes = new BlobType[0];

	#endregion inspector data

	#region inspector hooks

	public Cannon cannon;
	public Transform gameWorld;

	#endregion inspector hooks

	#region inspector prefabs

	public GameObject simplesphereBlobPrefab;

	#endregion inspector prefabs

	#region gameSettings

	public float blobSlowDistance = 5f;
	public float blobSlowFactor = 0.5f;
	public float minPending = 4; 

	#endregion gameSettings

	#region private objects

	private Controller_Base controller_ = null;

	private List<Blob> activeBlobs_ = new List<Blob>( );
	private Queue<Blob> pendingBlobs_ = new Queue<Blob>( );
	
	
	#endregion private objects

	protected override void PostAwake( )
	{
		blobSlowDistance = SettingsStore.retrieveSetting<float>( SettingsIds.blobSlowDistance);
		blobSlowFactor = SettingsStore.retrieveSetting<float>( SettingsIds.blobSlowFactor);

		if (cannon == null)
		{
			Debug.LogError( "No cannon" );
		}

		if (Application.platform == RuntimePlatform.Android)
		{
			if (DEBUG_GAME)
			{
				Debug.Log( "Creating touch controller" );
			}
			controller_ = Controller_Touch.Create( cannon );
		}
		else
		{
			if (DEBUG_GAME)
			{
				Debug.Log( "Creating mouse controller" );
			}
			controller_ = Controller_Mouse.Create( cannon );
		}
	}

	private Blob GetNewBlob()
	{
		Blob blob = null;

		blob = (GameObject.Instantiate<GameObject>( simplesphereBlobPrefab ) as GameObject).GetComponent<Blob>( );
		blob.cachedTransform.parent = gameWorld;
		if (blobTypes.Length == 0)
		{
			Debug.LogWarning( "No blobTyoes defined, using default" );
		}
		else
		{
			BlobType t = blobTypes[ UnityEngine.Random.Range(0, blobTypes.Length) ];
			blob.SetType( t );
		}
		return blob;
	}

	private bool isPaused_ = false;
	public bool isPaused
	{
		get { return isPaused_;  }
	}

	public void PauseGame()
	{
		if (isPaused_)
		{
			Debug.LogWarning( "Already paused" );
		}
		else
		{
			isPaused_ = true;
			if (controller_ != null)
			{
				controller_.gameObject.SetActive( false );
			}
		}
	}

	public void ResumeGame()
	{
		if (!isPaused_)
		{
			Debug.LogWarning( "Not paused" );
		}
		else
		{
			isPaused_ = false;
			if (controller_ != null)
			{
				controller_.gameObject.SetActive( true );
			}
		}

	}

	public void Update()
	{
		if (pendingBlobs_.Count < minPending)
		{
			Blob newBlob = GetNewBlob( );
			pendingBlobs_.Enqueue( newBlob );
			PositionPendingBlobs( );
		}
	}

	private void PositionPendingBlobs()
	{
		Vector3 basePosition = cannon.cachedTransform.position - 1f * Vector3.up;
		foreach (Blob b in pendingBlobs_)
		{
			basePosition += 0.5f * b.radius * Vector3.left;
			b.cachedTransform.position = basePosition - b.radius * Vector3.up;
			basePosition += 0.5f * b.radius * Vector3.left;
		}
	}

	public Blob ReleaseBlobFromPending()
	{
		Blob result = null;
		if (pendingBlobs_.Count > 0)
		{
			result = pendingBlobs_.Dequeue( );
			PositionPendingBlobs( );
		}
		return result;
	}
}
