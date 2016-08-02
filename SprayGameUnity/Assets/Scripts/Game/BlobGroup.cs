using UnityEngine;
using System.Collections;
using System.Collections.Generic;

abstract public class BlobGroup: RJWard.Core.IDebugDescribable
{
	private static readonly bool DEBUG_GROUPS = true;

	private HashSet<Blob> blobs_ = new HashSet<Blob>( );
	public HashSet<Blob> blobs
	{
		get { return blobs_; }
	}

	private string name_ = "UNNAMED";
	public string name
	{
		get { return name_;  }
	}
	public bool isConnectedToWall = false;
	
	protected BlobGroup(string n, Blob seedBlob)
	{
		name_ = n;
	}

	public bool ContainsBlob(Blob b)
	{
		return blobs_.Contains( b );
	}

	public void SeedFrom(Blob seedBlob)
	{
		blobs_.Clear( );

		Queue<Blob> blobsToCheck = new Queue<Blob>( );
		HashSet<Blob> blobsNotFollowed = new HashSet<Blob>( );

		blobsToCheck.Enqueue( seedBlob );

		while (blobsToCheck.Count > 0)
		{
			Blob blobTocheck = blobsToCheck.Dequeue( );
			if (!blobs_.Contains(blobTocheck) && ! blobsNotFollowed.Contains(blobTocheck))
			{
				if (shouldConnectedBlobBeAdded( blobTocheck ))
				{
					blobs_.Add( blobTocheck );
					foreach (Blob cBlob in blobTocheck.connectedBlobs)
					{
						if (!blobs_.Contains( cBlob ) && !blobsNotFollowed.Contains( cBlob ))
						{
							blobsToCheck.Enqueue( cBlob );
						}
					}
				}
				else
				{
					blobsNotFollowed.Add( blobTocheck );
				}
			}
		}
		if (DEBUG_GROUPS)
		{
			Debug.Log( "BlobGroup " + name_ + " has " + blobs_.Count + " after seeding with " + seedBlob.gameObject.name );
		}

	}

	abstract protected bool shouldConnectedBlobBeAdded( Blob b );

	public void DebugDescribe(System.Text.StringBuilder sb)
	{
		sb.Append( "  BlobGroup " + name );
		if (isConnectedToWall)
		{
			sb.Append( " (connected to wall)" );
		}
		sb.Append( " " + blobs.Count + " blobs" );
		foreach (Blob b in blobs)
		{
			sb.Append( "\n    " + b.gameObject.name);
		}
	}
}
