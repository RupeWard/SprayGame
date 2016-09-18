using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

	private Dictionary<string, Material> cachedMaterials_ = new Dictionary<string, Material>( );
	private Material AddCachedMaterial( string id, Material m )
	{
		if (cachedMaterials_ == null)
		{
			cachedMaterials_ = new Dictionary<string, Material>( );
		}
		Material newMat = new Material( m );
		if (cachedMaterials_.ContainsKey( id ))
		{
			cachedMaterials_[id] = newMat;
		}
		else
		{
			cachedMaterials_.Add( id, newMat );
		}
		return newMat;
	}


	public BlobGroupSameType( Blob b): base(b.blobType.DebugDescribe(), b)
	{
//		b.CacheAndSetMaterials( cachedMaterials_ );
		blobType_ = b.blobType;
		Init( b );
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
		bool warn = blobType.name != "FIXED" && blobType.ShouldDeleteGroupOfNum( blobs.Count +1);
        for (int i = 0; i < blobs.Count; i++)
		{
			blobs[i].SetWarningState( warn );
		}
		GameManager.Instance.blobManager.SetAllConnectorsWarningState( this, warn );
	}
}
