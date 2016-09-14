using UnityEngine;
using System.Collections;
using System;

public class BlobConnector_SimpleCylinder : BlobConnector_Base
{
	#region private hooks

	private MeshRenderer cachedRenderer_ = null;
	private Material cachedMaterial_ = null;
	public Material cachedMaterial
	{
		get { return cachedMaterial_;  }
	}

	static private GameObject cachedPrefab_ = null;
	static private GameObject cachedPrefab
	{
		get
		{
			if (cachedPrefab_ == null)
			{
				cachedPrefab_ = Resources.Load<GameObject>( "Prefabs/BlobConnectors/BlobConnector_SimpleCylinder" );
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

	#region private data

	static readonly Color defaultColor_ = new Color( 1f, 1f, 1f, 0.2f );

	#endregion private data


	private void Init( Blob_SimpleCylinder b0, Blob_SimpleCylinder b1 )
	{
		parentBlob_ = b0;
		childBlob_ = b1;

		Reposition( );

		if (sameBlobType())
		{
			parentBlob_.blobType.SetConnectorAppearance( this );
		}
		else
		{
			BlobType_Base.SetConnectorAppearance( parentBlob_.blobType, childBlob_.blobType, this );
		}
	}

	private bool sameBlobType()
	{
		return (parentBlob_ != null && parentBlob_.blobType == childBlob_.blobType);
	}

	protected override void PostAwake( )
	{
		heightOffset = new Vector3( 0f, 0f, 0.5f * Blob_SimpleCylinder.s_thickness );

		cachedRenderer_ = GetComponent<MeshRenderer>( );
		cachedMaterial_ = new Material( srcMaterial );
		cachedRenderer_.material = cachedMaterial_;

		Show( GameManager.Instance.showConnectors );
    }

#if UNITY_EDITOR
	private void Update()
	{
		if (cachedRenderer_.enabled != GameManager.Instance.showConnectors)
		{
			Show( GameManager.Instance.showConnectors );
		}
	}
#endif

	private Vector3 heightOffset = new Vector3( 0f, 0f, 1f );

	private static readonly float s_defaultThickWidth = 0.15f;
	private static readonly float s_defaultThinWidth = 0.05f;

	protected override void Reposition()
	{
		Vector3 start = parentBlob_.cachedTransform.position - heightOffset;
		Vector3 end = childBlob_.cachedTransform.position - heightOffset;
		Vector3 sum = start + end;
		Vector3 diff = end - start;
		float length = 0.9f * diff.magnitude;
		cachedTransform.position = 0.5f * sum;
		cachedTransform.localScale = new Vector3( (sameBlobType()?(s_defaultThickWidth):( s_defaultThinWidth)), length, 0.1f );
		float angleDegs = Mathf.Rad2Deg * Mathf.Atan2( diff.y, diff.x ) - 90f;
		cachedTransform.rotation = Quaternion.Euler( 0f, 0f, angleDegs );
	}

	public static BlobConnector_SimpleCylinder CreateConnection(Transform t, Blob_SimpleCylinder b0, Blob_SimpleCylinder b1 )
	{
		GameObject go = GameObject.Instantiate( cachedPrefab ) as GameObject;
		go.name =b0.gameObject.name+ "To_" + b1.gameObject.name;
		go.transform.parent = t;

		BlobConnector_SimpleCylinder result = go.GetComponent<BlobConnector_SimpleCylinder>();
		result.Init( b0, b1 );
		return result;
	}

	public override void Show( bool b )
	{
		cachedRenderer_.enabled = b;
	}

}
