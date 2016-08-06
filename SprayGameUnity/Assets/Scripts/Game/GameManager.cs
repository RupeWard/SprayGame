using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : RJWard.Core.Singleton.SingletonSceneLifetime<GameManager>
{
	static public readonly bool DEBUG_GAME = true;
	static public readonly bool DEBUG_WALLS = true;

	#region inspector data

	public BlobTypeStandard[] blobTypes = new BlobTypeStandard[0];

	#endregion inspector data

	#region inspector hooks

	public Cannon cannon;
	public Transform gameWorld;
	
	public EndGamePanel endGamePanel;

	public Transform topWall;

	#endregion inspector hooks

	#region inspector prefabs

	public GameObject simplesphereBlobPrefab;
	public GameObject simplecylinderBlobPrefab;

	#endregion inspector prefabs

	#region gameSettings

	public AudioClip deleteClip;

	public GameWorldSettings gameWorldSettings = new GameWorldSettings();
	public LevelSettings levelSettings = new LevelSettings( );

	public enum ELayer
	{
		Default,
		Cannon,
		Trace
	}

	public readonly static Dictionary<ELayer, int> layerIndices = new Dictionary<ELayer, int>( )
	{
		{ ELayer.Default, 0 },
		{ ELayer.Cannon, 8 },
		{ ELayer.Trace, 9 }
	};

	public static int layerMask( ELayer l)
	{
		return 1 >> layerIndices[l];
	}

	#endregion gameSettings

	#region private objects

	private bool isGameOver_ = false;

	private Controller_Base controller_ = null;

	private List<Blob> activeBlobs_ = new List<Blob>( );
	private Queue<Blob> pendingBlobs_ = new Queue<Blob>( );

	private BlobManager blobManager_ = null;
	private AudioSource cachedAudioSource_ = null;

	private float topWallTargetHeight_ = 0f;
	private float topWallStartingHeight_ = 0f;

	#endregion private objects

	#region flow

	protected override void PostAwake( )
	{
		topWallStartingHeight_ = topWall.position.y;
		topWallTargetHeight_ = topWallStartingHeight_;

		if (DEBUG_WALLS)
		{
			Debug.Log( "Starting/target = " + topWallStartingHeight_ + "/" + topWallTargetHeight_ );
		}

		cachedAudioSource_ = GetComponent<AudioSource>( );

		gameWorldSettings.blobSlowDistance = SettingsStore.retrieveSetting<float>( SettingsIds.blobSlowDistance);
		gameWorldSettings.blobSlowFactor = SettingsStore.retrieveSetting<float>( SettingsIds.blobSlowFactor);
		levelSettings.numBlobs = SettingsStore.retrieveSetting<int>( SettingsIds.numBlobs );

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

	private void registerHandlers()
	{
		MessageBus.instance.blobHitInKillZoneAction += HandleBlobHitInKillZone;
		MessageBus.instance.hitBlobInKillZoneAction += HandleHitBlobInKillZone;
		MessageBus.instance.firedBlobAction += HandleBlobFired;
	}

	public void Start( )
	{
		playPauseButtonText.text = "Play";
		registerHandlers( );
	}

	#endregion flow

	public UnityEngine.UI.Text playPauseButtonText;

	private float savedTimeScale_ = 0f;
	private Blob GetNewBlob()
	{
		Blob blob = null;

		GameObject prefab = simplecylinderBlobPrefab;
		blob = (GameObject.Instantiate<GameObject>( prefab ) as GameObject).GetComponent<Blob>( );
		blob.cachedTransform.parent = gameWorld;
		if (blobTypes.Length == 0)
		{
			Debug.LogWarning( "No blobTyoes defined, using default" );
		}
		else
		{
			BlobType_Base t = blobTypes[ UnityEngine.Random.Range(0, blobTypes.Length) ];
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
		if (isGameOver_)
		{
			Debug.LogError( "PlayGanme when game over" );
		}
		else if (isPlaying_ == false)
		{
			blobManager_ = (new GameObject( "BlobManager" )).AddComponent<BlobManager>( );
			isPlaying_ = true;
			isPaused_ = false;
			playPauseButtonText.text = "Pause";
			cannon.StartGame( );
			controller_.gameObject.SetActive( true );
		}
	}

	public void PauseGame()
	{
		if (isGameOver_)
		{
			Debug.LogError( "Pause when gae over" );
		}
		else
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
	}

	public void ResumeGame()
	{
		if (isGameOver_)
		{
			Debug.LogError( "Pause when gae over" );
		}
		else
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
	}

	public void Update()
	{
		if (isGameOver_)
		{ }
		else if (isPlaying_)
		{
			if (pendingBlobs_.Count < levelSettings.minPending)
			{
				Blob newBlob = GetNewBlob( );
				pendingBlobs_.Enqueue( newBlob );
				PositionPendingBlobs( );
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


	public void HandlePlayPauseButton()
	{
		if (isGameOver_)
		{
			SceneManager.Instance.ReloadScene( SceneManager.EScene.Game );
		}
		else
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

	public bool ShouldMergeTypeGroups(BlobGroupSameType g0, BlobGroupSameType g1)
	{
		bool result = false;
		if (g0 == null || g1 == null)
		{
			Debug.LogWarning( "Null type group passed to ShouldMergeTypeGroups" );
		}
		else if (g0 != g1 )
		{
			result = (g0.blobType == g1.blobType);
		}
		return result; 
	}

	public void PlayDeleteClip()
	{
		cachedAudioSource_.clip = deleteClip;
		cachedAudioSource_.Play( );
	}

	public void HandleHitBlobInKillZone(Blob b)
	{
		if (DEBUG_GAME)
		{
			Debug.Log( "Blob " + b.gameObject.name + " in killzone" );
		}
		EndGame( );
	}

	public void HandleBlobHitInKillZone(Blob b0, Blob b1)
	{
		if (DEBUG_GAME)
		{
			Debug.Log( "Blob " + b0.gameObject.name + " hit "+b1.gameObject.name+ " in killzone" );
		}
		EndGame( );
	}

	public void EndGame()
	{
		if (isPlaying_)
		{
			if (!isGameOver_)
			{
				isGameOver_ = true;
				playPauseButtonText.text = "Restart";
				if (controller_ != null)
				{
					controller_.gameObject.SetActive( false );
				}
				ShowEndGamePanel( );
			}
			else
			{
//				Debug.LogError( "EndGame when game already over!" );
			}
		}
		else
		{
			Debug.LogError( "EndGame when not playing!" );
		}
	}

	private void ShowEndGamePanel()
	{
		endGamePanel.Init( );
	}

	private void FixedUpdate()
	{
		if (!isGameOver_)
		{
			if (isPlaying_)
			{
				if (!isPaused_)
				{
					float currentTopWallHeight = topWall.position.y;
					float oldTopWallHeight = currentTopWallHeight;
					if (topWallTargetHeight_ > currentTopWallHeight)
					{
						currentTopWallHeight = currentTopWallHeight + Time.fixedDeltaTime * levelSettings.topWallAnimSpeed;
						if (currentTopWallHeight > topWallTargetHeight_)
						{
							currentTopWallHeight = topWallTargetHeight_;
						}
						topWall.position = new Vector3( topWall.position.x, currentTopWallHeight, topWall.position.z );
						if (DEBUG_WALLS)
						{
							Debug.Log( "Walls moved up to " + topWall.position );
						}
					}
					else if (topWallTargetHeight_ < currentTopWallHeight)
					{
						currentTopWallHeight = currentTopWallHeight - Time.fixedDeltaTime * levelSettings.topWallAnimSpeed;
						if (currentTopWallHeight < topWallTargetHeight_)
						{
							currentTopWallHeight = topWallTargetHeight_;
						}
						topWall.position = new Vector3( topWall.position.x, currentTopWallHeight, topWall.position.z );
						if (DEBUG_WALLS)
						{
							Debug.Log( "Walls moved down to " + topWall.position );
						}
					}
					if (oldTopWallHeight != currentTopWallHeight)
					{
						if (DEBUG_WALLS)
						{
							Debug.Log( "Sending WallMove" );
						}
						MessageBus.instance.sendWallMoveAction( currentTopWallHeight - oldTopWallHeight );
					}
				}
			}
		}
	}

	public void HandleBlobFired(Blob b)
	{
		topWallTargetHeight_ -= levelSettings.topWallDistPerBlob;
		if (DEBUG_WALLS)
		{
			Debug.Log( "Target height now " + topWallTargetHeight_ );
		}
	}
}
