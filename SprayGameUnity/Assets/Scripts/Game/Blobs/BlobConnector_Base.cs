using UnityEngine;
using System.Collections;

abstract public class BlobConnector_Base : MonoBehaviour
{
	#region private hooks

	public SpringJoint joint = null;

	private Transform cachedTransform_ = null;
	public Transform cachedTransform
	{
		get { return cachedTransform_; }
	}

	#endregion private hooks

	#region private data

	protected Blob parentBlob_ = null;
	protected Blob childBlob_ = null;

	#endregion private data

	private void Awake()
	{
		cachedTransform_ = transform;
		PostAwake( );
	}

	protected abstract void PostAwake( );
	protected abstract void Reposition( );

	public  abstract void Show( bool b );

	private void FixedUpdate( )
	{
		if (parentBlob_ != null && childBlob_ != null)
		{
			Reposition( );
		}
	}

	static public BlobConnector_Base CreateConnection(Transform t, Blob b0, Blob b1)
	{
		BlobConnector_Base result = null;
		{
			Blob_SimpleCylinder bc0 = b0 as Blob_SimpleCylinder;
			Blob_SimpleCylinder bc1 = b1 as Blob_SimpleCylinder;
			if (bc0 != null && bc1 != null)
			{
				result = BlobConnector_SimpleCylinder.CreateConnection(t, bc0, bc1 );
			}
			else
			{
				Debug.LogError( "Can't connect blob types" );
			}
		}

		return result;
	}

	private void OnDestroy()
	{
		if (joint != null)
		{
			Component.Destroy( joint );
			joint = null;
		}
		else
		{
//			Debug.LogWarning( "No joint on destroying connection " + gameObject.name );
		}
		if (parentBlob_ != null)
		{
			parentBlob_.connectedBlobs.Remove( childBlob_ );
			childBlob_.connectedBlobs.Remove( parentBlob_ );
		}
	}
}
