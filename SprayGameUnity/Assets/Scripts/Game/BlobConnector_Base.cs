﻿using UnityEngine;
using System.Collections;

abstract public class BlobConnector_Base : MonoBehaviour
{
	#region private hooks

	private Transform cachedTransform_ = null;
	public Transform cachedTransform
	{
		get { return cachedTransform_; }
	}

	#endregion private hooks

	#region private data

	protected Blob_SimpleSphere parentBlob_ = null;
	protected Blob_SimpleSphere childBlob_ = null;

	#endregion private data

	private void Awake()
	{
		cachedTransform_ = transform;
		PostAwake( );
	}

	protected abstract void PostAwake( );
	protected abstract void Reposition( );

	private void FixedUpdate( )
	{
		if (parentBlob_ != null && childBlob_ != null)
		{
			Reposition( );
		}
	}

	static public BlobConnector_Base CreateConnection( Blob b0, Blob b1)
	{
		BlobConnector_Base result = null;
		Blob_SimpleSphere bs0 = b0 as Blob_SimpleSphere;
		Blob_SimpleSphere bs1 = b1 as Blob_SimpleSphere;
		if (bs0 != null && bs1 != null)
		{
			result = BlobConnector_SimpleSphere.CreateConnection( bs0, bs1 );
		}
		return result;
	}
}
