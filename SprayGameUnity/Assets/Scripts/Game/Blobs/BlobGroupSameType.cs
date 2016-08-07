using UnityEngine;
using System.Collections;

public class BlobGroupSameType: BlobGroup
{
	private BlobType_Base blobType_ = null;
	public BlobType_Base blobType
	{
		get { return blobType_;  }
	}

	public BlobGroupSameType( Blob b): base(b.blobType.DebugDescribe(), b)
	{
		blobType_ = b.blobType;
		SeedFrom( b );
	}

	override protected bool shouldConnectedBlobBeAdded( Blob b)
	{
		return b.blobType == blobType_;
	}

}
