﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RJWard.Core;

abstract public class Blob : MonoBehaviour
{
	static private readonly bool DEBUG_BLOB = true;
	static private int nextNum_ = 0;

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


	#endregion private hooks

	#region private data

	private bool directlyConnectedToWall_ = false;

	private float radius_ = 0.5f;
	public float radius
	{
		get { return radius_; }
	}

	private int id_ = 0;
	public int id
	{
		get { return id_; }
	}

	private List<Blob> connections_ = new List<Blob>( );

	private BlobType blobType_ = null;
	public BlobType blobType
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
		PostAwake( );
		if (DEBUG_BLOB)
		{
			Debug.Log( "Created Blob " + gameObject.name );
		}
	}

	protected virtual void PostAwake()
	{

	}

	public void Init(Cannon cannon)
	{
		cachedTransform.position = cannon.cachedTransform.position;
		cachedTransform.rotation = cannon.cachedTransform.rotation;

	}

	protected abstract void SetAppearanceByType( BlobType t );

	public void SetType(BlobType t)
	{
		blobType_ = t;
		SetAppearanceByType( blobType_ );
	}

	public void AddConnection(Blob b)
	{
		if (false == connections_.Contains( b ))
		{
			connections_.Add( b );
		}
	}

	private bool isFired_ = false;
	public void SetFired()
	{
		isFired_ = true;
	}

	private void FixedUpdate()
	{
		Vector3 velocity = cachedRB_.velocity;
		float speed = velocity.magnitude;
        if (speed > 0f)
		{
			if (isFired_)
			{
				Ray ray = new Ray( cachedTransform_.position, velocity.normalized );
				RaycastHit hitInfo;
				bool hit = Physics.Raycast( ray, out hitInfo, speed * Time.fixedDeltaTime * GameManager.Instance.blobSlowDistance );
				if (hit)
				{
					BlobSlower blobSlower = hitInfo.collider.gameObject.GetComponent<BlobSlower>( );
					if (blobSlower != null)
					{
						Debug.Log( "Blob Encountering " + blobSlower.gameObject.name + "... Slowing" );
						cachedRB_.velocity = velocity * GameManager.Instance.blobSlowFactor * blobSlower.slowFactor;
					}
				}
				else
				{
					//				Debug.Log( "No hit" );
				}
			}
			else if (directlyConnectedToWall_)
			{
				Debug.Log( "Applying drag" );
				cachedRB_.AddForce( cachedRB_.velocity * -1f * wallDrag, ForceMode.VelocityChange );
			}
		}
	}

	static private readonly float wallDrag = 0.5f;
	private void OnCollisionEnter( Collision c)
	{
		Wall wall = c.gameObject.GetComponent<Wall>( );
		if (wall != null)
		{
			if (DEBUG_BLOB)
			{
				Debug.Log( "Blob " + gameObject.name + "Collision with Wall " + c.gameObject.name );
			}
			if (wall.stickiness != UnityExtensions.ETriBehaviour.Never)
			{
				cachedRB_.velocity = Vector3.zero;
//				cachedRB_.constraints = cachedRB_.constraints | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY;
				cachedRB_.constraints = cachedRB_.constraints | RigidbodyConstraints.FreezePositionY;

				MessageBus.instance.sendBlobFinishedAction( this );

				directlyConnectedToWall_ = true;
				isFired_ = false;
			}
		}
		else // NOT WALL
		{
			Blob blob = c.gameObject.GetComponent<Blob>( );
			if (blob != null)
			{
				if (DEBUG_BLOB)
				{
					Debug.Log( "Blob "+gameObject.name+" Collision with blob " + c.gameObject.name );
				}
				//cachedRB_.velocity = Vector3.zero;

				isFired_ = false;

				if (!connections_.Contains(blob))
				{
					SpringJoint joint = gameObject.AddComponent<SpringJoint>( );
					joint.anchor = Vector3.zero;
					joint.connectedAnchor = Vector3.zero;

					float distance = 0.5f * (this.radius + blob.radius);
					joint.minDistance = distance;
					joint.maxDistance = distance;
					joint.tolerance = 0.01f;// FIXME magic
					joint.spring = 40000f;// FIXME magic
					joint.damper= 10000f;// FIXME magic
					joint.autoConfigureConnectedAnchor = false;
					joint.connectedBody = blob.cachedRB;
					joint.anchor = Vector3.zero;
					joint.connectedAnchor = Vector3.zero;

					AddConnection( blob );
					blob.AddConnection( this );

					BlobConnector_Base connection = BlobConnector_Base.CreateConnection( this, blob );
				}

				//				cachedRB_.isKinematic = true;
				MessageBus.instance.sendBlobFinishedAction( this );

			}
			else // NOT BLOB
			{
				if (c.gameObject.name == "BlobKillZone")
				{
					if (DEBUG_BLOB)
					{
						Debug.Log( "Blob " + gameObject.name + " hit kill zone " + c.gameObject.name );
					}
					MessageBus.instance.sendBlobFinishedAction( this );
					GameObject.Destroy( this );
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
}
