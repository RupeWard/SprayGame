using UnityEngine;
using System.Collections;

[RequireComponent (typeof(AudioSource))]
public class Cannon : MonoBehaviour
{
//	static private readonly bool DEBUG_CANNON = true;
	static private readonly bool DEBUG_CANNON_PTR = false;
	static private readonly bool DEBUG_CANNON_FORCE = true;

	#region inspector hooks
	#endregion inspector hooks

	#region inspector data

	public float force = 1f;

	public AudioClip pointerDownClip;
	public AudioClip fireClip;
	public AudioClip abortClip;
	public AudioClip loadClip;

	#endregion inspector data

	#region private hooks

	private Transform cachedTransform_ = null;
	public Transform cachedTransform
	{
		get { return cachedTransform_; }
	}

	private AudioSource cachedAudioSource_ = null;

	private Material cachedMaterial_ = null;

	#endregion private hooks

	#region private data

	private bool isControlled_ = false;

	private bool shouldFire_ = false;

	private bool canFire_ = true;

	private Blob loadedBlob_ = null;

	#endregion private data

	static private readonly Color defaultColour_ = new Color( 0.4f, 0.4f, 0.4f );

	private void Awake( )
	{
		cachedTransform_ = transform;
		cachedAudioSource_ = GetComponent<AudioSource>( );
		cachedMaterial_ = GetComponent<MeshRenderer>( ).sharedMaterial;
		SetColour( );
		force = SettingsStore.retrieveSetting<float>( SettingsIds.cannonSpeed );
    }

	private void SetColour( )
	{
		SetColour( defaultColour_ );
	}

	private void SetColour(Color c)
	{
		cachedMaterial_.color = c;
	}

	private void Start()
	{
		addListeners( );

		StartCoroutine( LoadBlobCR( ) );
	}

	private void addListeners()
	{
		MessageBus.instance.pointerDownAction += HandlePointerDown;
		MessageBus.instance.pointerUpAction += HandlePointerUp;
		MessageBus.instance.pointerMoveAction += HandlePointerMove;
		MessageBus.instance.pointerAbortAction += HandlePointerAbort;
		MessageBus.instance.blobFinishedAction += HandleFiredBlobFinished;
	}

	private void removeListeners( )
	{
		if (MessageBus.exists)
		{
			MessageBus.instance.pointerDownAction -= HandlePointerDown;
			MessageBus.instance.pointerUpAction -= HandlePointerUp;
			MessageBus.instance.pointerMoveAction -= HandlePointerMove;
			MessageBus.instance.pointerAbortAction -= HandlePointerAbort;
		}
	}

	private void PointAt(Vector2 v)
	{
		float angle = Mathf.Rad2Deg * Mathf.Atan2( v.y, v.x );
		if (DEBUG_CANNON_PTR)
		{
			Debug.Log( "Cannon Angle is " + angle );
		}
		cachedTransform_.rotation = Quaternion.Euler( new Vector3( 0f,0f,angle - 90f ) );
	}

	public void HandlePointerAbort( Vector2 v )
	{
		if (isControlled_)
		{
			cachedAudioSource_.clip = abortClip;
			cachedAudioSource_.Play( );

			if (DEBUG_CANNON_PTR)
			{
				Debug.Log( "Cannon: Ptr ABORT at " + v );
			}
			cachedTransform_.rotation = Quaternion.identity;
			isControlled_ = false;
		}
		else
		{
//			Debug.LogWarning( "HandlePointerAbort when not controlled" );
		}
	}


	public void HandlePointerDown(Vector2 v)
	{
		if (!isControlled_)
		{
			cachedAudioSource_.clip = pointerDownClip;
			cachedAudioSource_.Play( );

			if (DEBUG_CANNON_PTR)
			{
				Debug.Log( "Cannon: Ptr DOWN at " + v );
			}
			PointAt( v );
			isControlled_ = true;
		}
		else
		{
			Debug.LogWarning( "HandlePointerDown when already controlled" );
		}
	}

	public void HandlePointerUp( Vector2 v )
	{
		if (isControlled_)
		{
			if (canFire_ && loadedBlob_ != null)
			{
				shouldFire_ = true;
			}
			else
			{
				cachedAudioSource_.clip = abortClip;
				cachedAudioSource_.Play( );
			}

			PointAt( v );

			if (DEBUG_CANNON_PTR)
			{
				Debug.Log( "Cannon: Ptr UP at " + v );
			}

			isControlled_ = false;
		}
		else
		{
			Debug.LogWarning( "HandlePointerUp when not controlled" );
		}
	}

	public void HandlePointerMove( Vector2 v )
	{
		if (isControlled_)
		{
			PointAt( v );
			if (DEBUG_CANNON_PTR)
			{
				Debug.Log( "Cannon: Ptr MOVE at " + v );
			}
		}
		else
		{
			Debug.LogWarning( "HandlePointerMove when not controlled" );
		}
	}

	public void HandleFiredBlobFinished(Blob b)
	{
		canFire_ = true;
	}

	private void FixedUpdate()
	{
		if (shouldFire_)
		{
			shouldFire_ = false;
			canFire_ = false;

			if (loadedBlob_ != null)
			{
				cachedAudioSource_.clip = fireClip;
				cachedAudioSource_.Play( );

				loadedBlob_.Init( this );

				Vector3 forceVector = loadedBlob_.cachedTransform.up * force;
				loadedBlob_.cachedRB.AddForce( forceVector, ForceMode.VelocityChange );
				if (DEBUG_CANNON_FORCE)
				{
					Debug.Log( "Cannon applying force of " + forceVector + " to blob "+ loadedBlob_.gameObject.name );
				}

				MessageBus.instance.sendFiredBlobAction( loadedBlob_ );
				loadedBlob_.Fire( );
				loadedBlob_ = null;
				SetColour( );
				StartCoroutine( LoadBlobCR( ) );
			}
			else
			{
				cachedAudioSource_.clip = abortClip;
				cachedAudioSource_.Play( );
				Debug.LogWarning( "No blob loaded" );
			}
		}
	}

	public float blobLoadDelay = 2f;

	private IEnumerator LoadBlobCR()
	{
		while (loadedBlob_ == null)
		{
			yield return new WaitForSeconds( blobLoadDelay );
			Blob blob = GameManager.Instance.ReleaseBlobFromPending( );
			if (blob != null)
			{
				LoadBlob( blob );
			}
			else
			{
				Debug.LogWarning( "Cannin failed to get blob from GM" );
			}
		}
	}

	public bool LoadBlob(Blob b)
	{
		bool result = false;
		if (loadedBlob_ == null)
		{
			loadedBlob_ = b;
			loadedBlob_.Load( );
			loadedBlob_.Init( this );
			SetColour( loadedBlob_.blobType.colour );

			cachedAudioSource_.clip = loadClip;
			cachedAudioSource_.Play( );
		}
		else
		{
			Debug.LogWarning( "Can;'t load second blob" );
		}
		return result;
	}
}
