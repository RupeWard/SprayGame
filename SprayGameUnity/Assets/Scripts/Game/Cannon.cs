using UnityEngine;
using System.Collections;

[RequireComponent (typeof(AudioSource))]
public class Cannon : MonoBehaviour
{
	static private readonly bool DEBUG_CANNON = true;
	static private readonly bool DEBUG_CANNON_PTR = false;
	static private readonly bool DEBUG_CANNON_FORCE = true;

	#region inspector hooks
	#endregion inspector hooks

	#region inspector data

	public float force = 1f;

	public AudioClip pointerDownClip;
	public AudioClip fireClip;
	public AudioClip abortClip;

	#endregion inspector data

	#region private hooks

	private Transform cachedTransform_ = null;
	public Transform cachedTransform
	{
		get { return cachedTransform_; }
	}

	private AudioSource cachedAudioSource_ = null;

	#endregion private hooks

	#region private data

	bool isControlled_ = false;

	bool shouldFire_ = false;

	#endregion private data

	private void Awake( )
	{
		cachedTransform_ = transform;
		cachedAudioSource_ = GetComponent<AudioSource>( );
    }

	private void Start()
	{
		addListeners( );
	}

	private bool isQuitting_ = false;

	private void OnApplicationQuit()
	{
		isQuitting_ = true;
	}

	private void OnDestroy()
	{
		if (!isQuitting_)
		{
			removeListeners( );
		}
	}

	private void addListeners()
	{
		MessageBus.instance.pointerDownAction += HandlePointerDown;
		MessageBus.instance.pointerUpAction += HandlePointerUp;
		MessageBus.instance.pointerMoveAction += HandlePointerMove;
		MessageBus.instance.pointerAbortAction += HandlePointerAbort;
	}

	private void removeListeners( )
	{
		MessageBus.instance.pointerDownAction -= HandlePointerDown;
		MessageBus.instance.pointerUpAction -= HandlePointerUp;
		MessageBus.instance.pointerMoveAction -= HandlePointerMove;
		MessageBus.instance.pointerAbortAction -= HandlePointerAbort;
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
			shouldFire_ = true;

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

	private void FixedUpdate()
	{
		if (shouldFire_)
		{
			shouldFire_ = false;

			cachedAudioSource_.clip = fireClip;
			cachedAudioSource_.Play( );

			Blob blob = GameManager.Instance.GetNewBlob( );
			if (blob != null)
			{
				blob.Init( this );
				Vector3 forceVector = blob.cachedTransform.up * force;
				blob.cachedRB.AddForce( forceVector, ForceMode.VelocityChange );
				if (DEBUG_CANNON_FORCE)
				{
					Debug.Log( "Cannon applying force of " + forceVector );
				}
			}
			else
			{
				cachedAudioSource_.clip = abortClip;
				cachedAudioSource_.Play( );
				Debug.LogWarning( "Failed to get blob from GameManager" );
			}
		}
	}
}
