using UnityEngine;
using System.Collections;

public class BlobGroupConnected : BlobGroup
{
	public BlobGroupConnected(Blob b): base("ConnectedGroup", b)
	{
		SeedFrom( b );

	}

	override protected bool shouldConnectedBlobBeAdded( Blob b)
	{
		return true;
	}
}
