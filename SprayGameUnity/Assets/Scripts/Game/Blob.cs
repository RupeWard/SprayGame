﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RJWard.Core;

public class Blob : MonoBehaviour
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

	private int id_ = 0;
	public int id
	{
		get { return id_; }
	}

	private List<Blob> connections_ = new List<Blob>( );

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

	public void AddConnection(Blob b)
	{
		if (false == connections_.Contains( b ))
		{
			connections_.Add( b );
		}
	}

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
				cachedRB_.isKinematic = true;
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

				if (!connections_.Contains(blob))
				{
					HingeJoint hinge = gameObject.AddComponent<HingeJoint>( );
					hinge.connectedBody = blob.cachedRB;
					AddConnection( blob );
					blob.AddConnection( this );
				}

				//				cachedRB_.isKinematic = true;

			}
			else // NOT BLOB
			{
				if (DEBUG_BLOB)
				{
					Debug.Log( "Blob " + gameObject.name + "Collision with unhandled " + c.gameObject.name );
				}
			}
		}
	}
}
