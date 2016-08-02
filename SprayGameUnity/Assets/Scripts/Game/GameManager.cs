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
	public GameObject simplecylinderBlobPrefab;

	#endregion inspector prefabs

	#region gameSettings

	public float blobSlowDistance = 5f;
	public float blobSlowFactor = 0.5f;
	public float minPending = 4;
	public int numBlobs = 4;

	public Blob.EType blobType = Blob.EType.SimpleSphere;

	#endregion gameSettings

	#region private objects

	private Controller_Base controller_ = null;

	private List<Blob> activeBlobs_ = new List<Blob>( );
	private Queue<Blob> pendingBlobs_ = new Queue<Blob>( );

	private BlobManager blobManager_ = null;

	#endregion private objects

	protected override void PostAwake( )
	{
		blobSlowDistance = SettingsStore.retrieveSetting<float>( SettingsIds.blobSlowDistance);
		blobSlowFactor = SettingsStore.retrieveSetting<float>( SettingsIds.blobSlowFactor);
		numBlobs = SettingsStore.retrieveSetting<int>( SettingsIds.numBlobs );

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
		controller_.gameObject.SetActive( false );
	}

	public void Start()
	{
		playPauseButtonText.text = "Play";
	}

	public UnityEngine.UI.Text playPauseButtonText;

	private float savedTimeScale_ = 0f;
	private Blob GetNewBlob()
	{
		Blob blob = null;

		GameObject prefab = null;
		switch (blobType)
		{
			case Blob.EType.SimpleSphere:
				prefab = simplesphereBlobPrefab;
				break;
			case Blob.EType.SimpleCylinder:
				prefab = simplecylinderBlobPrefab;
				break;
		}
		blob = (GameObject.Instantiate<GameObject>( prefab ) as GameObject).GetComponent<Blob>( );
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

	private bool isPlaying_ = false;
	public bool isPlaying
	{
		get { return isPlaying_; }
	}


	private bool isPaused_ = false;
	public bool isPaused
	{
		get { return isPaused_;  }
	}

	public void PlayGame( )
	{
		if (isPlaying_ == false)
		{
			blobManager_ = (new GameObject( "BlobManager" )).AddComponent<BlobManager>( );
			isPlaying_ = true;
			playPauseButtonText.text = "Pause";
			cannon.StartGame( );
			controller_.gameObject.SetActive( true );
		}
	}

	public void PauseGame()
	{
		if (!isPlaying_)
		{
			Debug.LogError( "Pause when not playing" );
		}
		else
		{
			if (isPaused_)
			{
				Debug.LogWarning( "Already paused" );
			}
			else
			{
				isPaused_ = true;
				playPauseButtonText.text = "Continue";
				savedTimeScale_ = Time.timeScale;
				Time.timeScale = 0f;
				if (controller_ != null)
				{
					controller_.gameObject.SetActive( false );
				}
			}
		}
	}

	public void ResumeGame()
	{
		if (!isPlaying_)
		{
			Debug.LogError( "Resume when not playing" );
		}
		else
		{
			if (!isPaused_)
			{
				Debug.LogWarning( "Not paused" );
			}
			else
			{
				isPaused_ = false;
				playPauseButtonText.text = "Pause";
				Time.timeScale = savedTimeScale_;
				if (controller_ != null)
				{
					controller_.gameObject.SetActive( true );
				}
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

		if (pendingFlashGroups.Count > 0)
		{
			if (isFlashingGroup_ == false)
			{
				BlobGroup bg = pendingFlashGroups.Dequeue( );
				StartCoroutine( FlashBlobGroupCR( bg, 0.6f ) );
			}
		}
	}

	private void PositionPendingBlobs()
	{
		Vector3 basePosition = cannon.cachedTransform.position - 0.25f * Vector3.up;
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

	private bool isFlashingGroup_ = false;

	public IEnumerator FlashBlobGroupCR(BlobGroup blobs, float duration)
	{
		Debug.LogWarning( "START Flashing blob group " + blobs.name + " " + blobs.blobs.Count );
		isFlashingGroup_ = true;
		float elapsed = 0f;
		while (elapsed < duration)
		{
			float tFraction = elapsed / duration;
			float sFraction = Mathf.Sin( tFraction * Mathf.PI );
			foreach (Blob b in blobs.blobs)
			{
				b.SetFlashState( sFraction );
			}
			elapsed += Time.deltaTime;
			yield return null;
		}
		foreach (Blob b in blobs.blobs)
		{
			b.SetFlashState( 0f );
		}
		isFlashingGroup_ = false;
		Debug.LogWarning( "END Flashing blob group " + blobs.name );
	}

	private Queue<BlobGroup> pendingFlashGroups = new Queue<BlobGroup>( );
	public void FlashBlobGroup(BlobGroup b)
	{
		if (!pendingFlashGroups.Contains( b ))
		{
			Debug.LogWarning( "QUEUE Flashing blob group " + b.name );
			pendingFlashGroups.Enqueue( b );
		}
	}

	public void HandlePlayPauseButton()
	{
		if (!isPlaying_)
		{
			PlayGame( );
		}
		else
		{
			if (isPaused_)
			{
				ResumeGame( );
			}
			else
			{
				PauseGame( );
			}
		}
	}
}
