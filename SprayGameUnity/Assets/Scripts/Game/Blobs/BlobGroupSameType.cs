using UnityEngine;
using System.Collections;

public class BlobGroupSameType: BlobGroup
{
	private BlobType_Base blobType_ = null;
	public BlobType_Base blobType
	{
		get { return blobType_;  }
	}

	private void SetTypeName( )
	{
		SetName( );
		name_ = name_ + "_" + blobType_.name;
	}

	public BlobGroupSameType( Blob b): base(b.blobType.DebugDescribe(), b)
	{
		blobType_ = b.blobType;
		SeedFrom( b );
		SetTypeName( );
	}

	override protected bool shouldConnectedBlobBeAdded( Blob b)
	{
		return b.blobType == blobType_;
	}

	public void ChangeType(BlobType_Base newType)
	{
		if (blobType_ == newType)
		{
			Debug.LogError( "Request to change grroup type when it's already " + blobType_.DebugDescribe( ) );
		}
		blobType_ = newType;
		SetTypeName( );
		for (int i = 0; i < blobs.Count; i++)
		{
			blobs[i].ChangeType( newType);
		}
	}

	public void DisplayWarningState()
	{
		for (int i = 0; i < blobs.Count; i++)
		{
			blobs[i].SetWarningState( blobType.name != "FIXED" &&  blobs.Count == GameManager.Instance.levelSettings.numBlobs - 1 );
		}
	}
}
