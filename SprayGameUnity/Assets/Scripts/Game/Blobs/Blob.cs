using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RJWard.Core;

abstract public class Blob : MonoBehaviour
{
	static private readonly bool DEBUG_BLOB = false;
	static private readonly bool DEBUG_COLLISIONS = false;
	static private int nextNum_ = 0;

	public enum EState
	{
		Pending,
		Loaded,
		Fired,
		Hit
	}

	public BlobGroupConnected connectedGroup = null;
	public BlobGroupSameType typeGroup = null;

	#region private hooks

	private Transform cachedTransform_ = null;
	public Transform cachedTransform
	{
		get { return cachedTransform_;  }
	}

	private Rigidbody cachedRB_ = null;
	public Rigidbody cachedRB
	{
		get { return cachedRB_; }
	}

	/*
	protected Material cachedBottomDiscMaterial_ = null;
	public Material cachedMaterial
	{
		get { return cachedBottomDiscMaterial_; }
	}
	*/

	#endregion private hooks

	#region private data

	private bool isDead_ = false;
	public virtual void HandleDeath()
	{
		isDead_ = true;
	}

	private bool inKillZone_ = false;
	public bool IsInKillZone
	{
		get { return inKillZone_;  }
	}

	public void HandleEnterKillZone()
	{
		if (!inKillZone_)
		{
			if (BlobKillZone.DEBUG_KILLZONE)
			{
				Debug.Log( "Blob " + gameObject.name + " enter killzone with state "+state_ );
			}
			inKillZone_ = true;
			if (state_ == EState.Hit )
			{
				this.HandleDeath( );
				MessageBus.instance.sendHitBlobHitKillZoneAction( this );
			}
		}
		else
		{
			Debug.LogWarning( "HandleEntreKillzone when flag already set on " + gameObject.name );
		}
	}

	public void HandleExitKillZone( )
	{
		if (inKillZone_)
		{
			if (BlobKillZone.DEBUG_KILLZONE)
			{
				Debug.Log( "Blob " + gameObject.name + " exit killzone" );
			}
			inKillZone_ = false;
		}
		else
		{
			Debug.LogWarning( "HandleExitKillzone when flag not set on " + gameObject.name );
		}
	}

	private bool directlyConnectedToWall_ = false;

	private EState state_ = EState.Pending;
	public EState state
	{
		get { return state_; }
	}

	public void SetHitState()
	{
		state_ = EState.Hit;
	}

	public void Fire()
	{
		if (state_ != EState.Loaded)
		{
			Debug.LogError( "State = " + state_ + " on firing" );
		}
		else
		{
			state_ = EState.Fired;
//			isFired_ = true;
			
		}
	}

	public void Load()
	{
		if (state_ != EState.Pending)
		{
			Debug.LogError( "State = " + state_ + " on loading" );
		}
		else
		{
			state_ = EState.Loaded;
		}
	}

	protected float radius_ = 0.5f;
	public float radius
	{
		get { return radius_; }
	}

	private int id_ = 0;
	public int id
	{
		get { return id_; }
	}

	private List<Blob> connectedBlobs_ = new List<Blob>( );
	public List<Blob> connectedBlobs
	{
		get { return connectedBlobs_;  }
	}

	private BlobType_Base blobType_ = null;
	public BlobType_Base blobType
	{
		get { return blobType_; }
	}

	#endregion private data

	private void Awake()
	{
		id_ = nextNum_;
		nextNum_++;
		gameObject.name = "Blob_" + id_.ToString( );

		cachedTransform_ = transform;
		cachedRB_ = GetComponent<Rigidbody>( );
		defaultConstraints_ = cachedRB_.constraints;
		PostAwake( );
		if (DEBUG_BLOB)
		{
//			Debug.Log( "Created Blob " + gameObject.name );
		}
	}

	protected virtual void PostAwake()
	{

	}

	abstract public void Init( Cannon cannon );

	protected abstract void SetAppearanceByType( BlobType_Base t );

	public void SetType(BlobType_Base t)
	{
		blobType_ = t;
		SetAppearanceByType( blobType_ );
	}

	public void AddConnection(Blob b)
	{
		if (false == connectedBlobs_.Contains( b ))
		{
			connectedBlobs_.Add( b );
		}
	}

	//private bool isFired_ = false;
	//public void SetFired()
	//{
	//	isFired_ = true;
	//}

	private bool HasBeenFired()
	{
		return (state_ == EState.Fired || state_ == EState.Hit);

	}
	private void FixedUpdate()
	{
		if (!HasBeenFired())
		{
			return;
		}
		Vector3 velocity = cachedRB_.velocity;
		float speed = velocity.magnitude;
        if (speed > 0f)
		{
			if (state_ == EState.Fired)
			{
				Ray ray = new Ray( cachedTransform_.position, velocity.normalized );
				RaycastHit hitInfo;
				bool hit = Physics.Raycast( ray, out hitInfo, speed * Time.fixedDeltaTime * GameManager.Instance.gameWorldSettings.blobSlowDistance );
				if (hit)
				{
					BlobSlower blobSlower = hitInfo.collider.gameObject.GetComponent<BlobSlower>( );
					if (blobSlower != null)
					{
//						Debug.Log( "Blob Encountering " + blobSlower.gameObject.name + "... Slowing" );
						cachedRB_.velocity = velocity * GameManager.Instance.gameWorldSettings.blobSlowFactor * blobSlower.slowFactor;
					}
				}
				else
				{
					//				Debug.Log( "No hit" );
				}
			}
			else if (directlyConnectedToWall_)
			{
//				Debug.Log( "Applying drag" );
//				cachedRB_.AddForce( cachedRB_.velocity * -1f * GameManager.Instance.gameWorldSettings.wallDrag, ForceMode.VelocityChange );
			}
			if (directlyConnectedToWall_)
			{
				cachedRB_.velocity = new Vector3( velocity.x * (1f- GameManager.Instance.gameWorldSettings.wallDrag), 0f, 0f );
			}

		}
	}

	private void OnTriggerEnter(Collider c)
	{
		if (c.gameObject.GetComponent<BlobKillZone>() != null)
		{
			HandleEnterKillZone( );
		}
	}

	private void OnTriggerExit( Collider c )
	{
		if (c.gameObject.GetComponent<BlobKillZone>( ) != null)
		{
			HandleExitKillZone( );
		}
	}

	private void OnCollisionEnter( Collision c)
	{
		if (!HasBeenFired())
		{
			return;
		}
		TopWall wall = c.gameObject.GetComponent<TopWall>( );
		if (wall != null)
		{
			if (DEBUG_COLLISIONS && !directlyConnectedToWall_)
			{
				Debug.Log( "Blob " + gameObject.name + "Collision with TopWall " + c.gameObject.name );
			}
			if (!directlyConnectedToWall_)
			{
				{
					state_ = EState.Hit;
					directlyConnectedToWall_ = true;

//					cachedRB_.velocity = Vector3.zero;
					//					cachedRB_.constraints = cachedRB_.constraints | RigidbodyConstraints.FreezePositionY;
					
//					cachedRB_.isKinematic = true;

					MessageBus.instance.onWallMoveAction += HandleWallMove;
					MessageBus.instance.sendBlobHitWallAction( this, wall.gameObject.GetComponent<Wall>( ) );
					MessageBus.instance.sendBlobFinishedAction( this );
				}
			}
			else
			{
//				Debug.Log( "Blob " + gameObject.name + " already connected to wall" );
			}
		}
		else // NOT WALL
		{
			Blob blob = c.gameObject.GetComponent<Blob>( );
			if (blob != null)
			{
				if (DEBUG_COLLISIONS && connectedBlobs.Contains(blob)== false)
				{
					Debug.Log( "Blob " + gameObject.name + " Collision with new blob " + c.gameObject.name );
				}
//				state_ = EState.Hit;
				MessageBus.instance.sendBlobHitBlobAction( this, blob );
				MessageBus.instance.sendBlobFinishedAction( this );
				if (this.directlyConnectedToWall_ )
				{
					if (DEBUG_COLLISIONS)
					{
						Debug.Log( "Blob " + gameObject.name + " hit by " + blob.gameObject.name + " while connected to wall. v=" + cachedRB_.velocity );
					}
				}
			}
			else // NOT BLOB
			{
				if (c.gameObject.name == "BlobKillZone")
				{
					if (DEBUG_BLOB)
					{
						Debug.Log( "Blob " + gameObject.name + " hit kill zone " + c.gameObject.name );
					}
					// TODO remove from groups etc
					MessageBus.instance.sendBlobFinishedAction( this );
					GameObject.Destroy( this.gameObject );
				}
				else
				{
					if (DEBUG_BLOB)
					{
						Debug.Log( "Blob " + gameObject.name + "Collision with unhandled " + c.gameObject.name );
					}
				}
			}
		}
	}
	
	private RigidbodyConstraints defaultConstraints_;

	public void HandleWallMove(float f)
	{
		if (directlyConnectedToWall_)
		{
			if (GameManager.DEBUG_WALLS)
			{
				Debug.Log( gameObject.name + " receiving wall move by " + f );
			}

//			cachedRB_.constraints = defaultConstraints_;
			cachedTransform_.position = new Vector3( cachedTransform_.position.x, cachedTransform_.position.y + f, cachedTransform_.position.z );
//			cachedRB_.constraints = cachedRB_.constraints | RigidbodyConstraints.FreezePositionY;
		}
		else
		{
			Debug.LogWarning( "Blob " + gameObject.name + " receving wall move moessge when not connected to it" );
		}
	}
	private void OnDestroy()
	{
		if (directlyConnectedToWall_)
		{
			if (MessageBus.exists)
			{
				MessageBus.instance.onWallMoveAction -= HandleWallMove;
			}
		}
	}

	abstract public void SetCountdownState( float fraction01 );
}
