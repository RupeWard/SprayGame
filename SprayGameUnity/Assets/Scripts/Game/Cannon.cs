using UnityEngine;
using System.Collections;

[RequireComponent (typeof(AudioSource))]
public class Cannon : MonoBehaviour
{
//	static private readonly bool DEBUG_CANNON = true;
	static private readonly bool DEBUG_CANNON_PTR = false;
	static private readonly bool DEBUG_CANNON_FORCE = true;

	#region inspector hooks

	public MeshRenderer turretRenderer;

	public Transform trace0;
	public Transform trace1;

	public float traceLength = 8f;

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
	private Material cachedTrace0Material_ = null;

	#endregion private hooks

	#region private data

	private bool isControlled_ = false;

	private bool shouldFire_ = false;

//	private bool canFire_ = true;

	private Blob loadedBlob_ = null;

	#endregion private data

	static private readonly Color defaultColour_ = new Color( 0.4f, 0.4f, 0.4f );

	private void Awake( )
	{
		cachedTransform_ = transform;
		cachedAudioSource_ = GetComponent<AudioSource>( );

		cachedTrace0Material_ = new Material( trace0.GetComponent<MeshRenderer>().sharedMaterial);
		trace0.GetComponent<MeshRenderer>( ).sharedMaterial = cachedTrace0Material_;

        MeshRenderer renderer = GetComponent<MeshRenderer>( );
        cachedMaterial_ = new Material( renderer.sharedMaterial);
		renderer.sharedMaterial= cachedMaterial_;
		turretRenderer.sharedMaterial = cachedMaterial_;

		SetColour( );
		force = SettingsStore.retrieveSetting<float>( SettingsIds.cannonSpeed );
		trace0.gameObject.SetActive( false );
		trace1.gameObject.SetActive( false );
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
	}

	public void StartGame()
	{
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

	private readonly Vector3 heightOffset = new Vector3( 0f, 0f, -0.1f );
	private void PointAt(Vector2 v)
	{
		float angle = Mathf.Rad2Deg * Mathf.Atan2( v.y, v.x );
		if (DEBUG_CANNON_PTR)
		{
			Debug.Log( "Cannon Angle is " + angle );
		}
		cachedTransform_.rotation = Quaternion.Euler( new Vector3( 0f,0f,angle - 90f ) );
		if (traceLength > 0f && loadedBlob_ != null && isControlled_)
		{
			float lengthSoFar = traceLength;
			Vector3 direction = v.normalized;
			Vector3 traceEnd = cachedTransform_.position + direction * traceLength;
			Ray ray = new Ray( cachedTransform.position, direction );
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo, traceLength, GameManager.layerMask(GameManager.ELayer.Default)))
			{
				Debug.Log( "Ray hit " + hitInfo.collider.gameObject.name +" "+hitInfo.ToString());
				traceEnd = hitInfo.point;
				lengthSoFar = Vector3.Distance( traceEnd, cachedTransform_.position );
			}
			trace0.transform.position = 0.5f * (cachedTransform_.position + traceEnd) + heightOffset;
			trace0.transform.localScale = new Vector3( lengthSoFar / cachedTransform_.localScale.x, 0.1f, 0.1f ) ;
			trace0.transform.rotation = Quaternion.Euler( new Vector3( 0f, 0f, angle ) ); ;// Quaternion.Euler( direction );
			trace0.gameObject.SetActive( true );
			cachedTrace0Material_.mainTextureScale = new Vector2( 2 * lengthSoFar, 0f );
		}
		else
		{
			trace0.gameObject.SetActive( false );
			trace1.gameObject.SetActive( false );
		}
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
			trace0.gameObject.SetActive( false );
			trace1.gameObject.SetActive( false );
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
			if (/*canFire_ && */ loadedBlob_ != null)
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
			trace0.gameObject.SetActive( false );
			trace1.gameObject.SetActive( false );
		}
		else
		{
			Debug.LogWarning( "HandlePointerUp when not controlled" );
		}
	}

	private float minAngle = 15f;

	public void HandlePointerMove( Vector2 v )
	{
		if (isControlled_)
		{
			float angle = Mathf.Rad2Deg * Mathf.Atan2( v.y, v.x );
			if (angle > minAngle && angle < 180f-minAngle)
			{
				PointAt( v );
			}
			else
			{
				Debug.LogWarning( "Ptr move too low: "+v+" gives "+angle );
			}
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
		//canFire_ = true;
	}

	private void FixedUpdate()
	{
		if (shouldFire_)
		{
			shouldFire_ = false;
			//canFire_ = false;

			if (loadedBlob_ != null)
			{
				cachedAudioSource_.clip = fireClip;
				cachedAudioSource_.Play( );

				loadedBlob_.Init( this );

				Vector3 forceVector = cachedTransform.up * force;
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
