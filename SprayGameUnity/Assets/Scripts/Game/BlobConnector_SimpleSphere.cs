using UnityEngine;
using System.Collections;
using System;

public class BlobConnector_SimpleSphere : BlobConnector_Base
{
	#region private hooks

	private MeshRenderer cachedRenderer_ = null;
	private Material cachedMaterial_ = null;

	static private GameObject cachedPrefab_ = null;
	static private GameObject cachedPrefab
	{
		get
		{
			if (cachedPrefab_ == null)
			{
				cachedPrefab_ = Resources.Load<GameObject>( "Prefabs/BlobConnector_SimpleSphere" );
				if (cachedPrefab_ == null)
				{
					Debug.LogError( "Failed to load src prefab for blob connectors" );
				}
			}
			return cachedPrefab_;
		}
	}

	static private Material cachedSrcMaterial_ = null;
	static private Material srcMaterial
	{
		get
		{
			if (cachedSrcMaterial_ == null)
			{
				cachedSrcMaterial_ = Resources.Load<Material>( "Materials/BlobConnectorMaterial" );
				if (cachedSrcMaterial_ == null)
				{
					Debug.LogError( "Failed to load src Material for blob connectors" );
				}
			}
			return cachedSrcMaterial_;
		}
	}
	
	#endregion private hooks



	private void Init( Blob_SimpleSphere b0, Blob_SimpleSphere b1 )
	{
		parentBlob_ = b0;
		childBlob_ = b1;

		Reposition( );

		if (parentBlob_.blobType == childBlob_.blobType)
		{
			cachedMaterial_.color = parentBlob_.blobType.colour;
		}
	}

	protected override void PostAwake( )
	{
		cachedRenderer_ = GetComponent<MeshRenderer>( );
		cachedMaterial_ = new Material( srcMaterial );
		cachedRenderer_.material = cachedMaterial_;
    }

	private readonly Vector3 heightOffset = new Vector3( 0f, 0f, 1f );

	protected override void Reposition()
	{
		Vector3 start = parentBlob_.cachedTransform.position - parentBlob_.radius * heightOffset;
		Vector3 end = childBlob_.cachedTransform.position - childBlob_.radius * heightOffset;
		Vector3 sum = start + end;
		Vector3 diff = end - start;
		float length = diff.magnitude;
		cachedTransform.position = 0.5f * sum;
		cachedTransform.localScale = new Vector3( 0.1f, length, 0.1f );
		float angleDegs = Mathf.Rad2Deg * Mathf.Atan2( diff.y, diff.x ) - 90f;
		cachedTransform.rotation = Quaternion.Euler( 0f, 0f, angleDegs );
	}

	public static BlobConnector_SimpleSphere CreateConnection( Blob_SimpleSphere b0, Blob_SimpleSphere b1 )
	{
		GameObject go = GameObject.Instantiate( cachedPrefab ) as GameObject;
		go.name = "To_" + b1.gameObject.name;
		go.transform.parent = b0.gameObject.transform;

		BlobConnector_SimpleSphere result = go.GetComponent<BlobConnector_SimpleSphere>();
		result.Init( b0, b1 );
		return result;
	}
}
