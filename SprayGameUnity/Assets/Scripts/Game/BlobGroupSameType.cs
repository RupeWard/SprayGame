using UnityEngine;
using System.Collections;

public class BlobGroupSameType: BlobGroup
{
	private BlobType blobType_ = null;

	public BlobGroupSameType( Blob b): base("Col="+b.blobType.colour.ToString(), b)
	{
		blobType_ = b.blobType;
		SeedFrom( b );
	}

	override protected bool shouldConnectedBlobBeAdded( Blob b)
	{
		return b.blobType == blobType_;
	}

}
