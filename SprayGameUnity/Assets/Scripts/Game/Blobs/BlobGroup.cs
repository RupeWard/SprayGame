using UnityEngine;
using System.Collections;
using System.Collections.Generic;

abstract public class BlobGroup: RJWard.Core.IDebugDescribable
{
	private static readonly bool DEBUG_GROUPS = false;

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

	static private int s_counter_ = 0;

	protected BlobGroup(string n, Blob seedBlob)
	{
		name_ = s_counter_.ToString()+"_"+ n;
		s_counter_++;
	}

	public bool ContainsBlob(Blob b)
	{
		return blobs_.Contains( b );
	}

	public void SetCountdownState( float fraction01 )
	{
		foreach (Blob b in blobs_)
		{
			b.SetCountdownState( fraction01 );
		}
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

	static private readonly bool DEBUG_PATHS = true;

	public List<List<Blob>> GetClosedPaths(  )
	{
		System.Text.StringBuilder sb = null;
		if (DEBUG_PATHS)
		{
			sb = new System.Text.StringBuilder( );
			sb.Append( "GetClosedPaths on group " + name_ +" with "+blobs_.Count+" blobs");
		}
		List<List<Blob>> result = null;
		HashSet<Blob> candidateBlobs = new HashSet<Blob>( );
		foreach (Blob b in blobs_)
		{
			if (b.connectedBlobs.Count > 1)
			{
				candidateBlobs.Add( b );
			}
		}
		if (sb!= null)
		{
			sb.Append( "\n " + candidateBlobs.Count + " with >1 connections" );
		}
		int numCandidateBlobs = 0;
		int pass = 0;
		while (numCandidateBlobs != candidateBlobs.Count)
		{
			numCandidateBlobs = candidateBlobs.Count;
			pass++;
			HashSet<Blob> blobsToRemove = new HashSet<Blob>( );
			foreach (Blob b in candidateBlobs)
			{
				List<Blob> connectedBlobs = b.connectedBlobs;
				int numCandidates = 0;
				foreach( Blob b2 in connectedBlobs)
				{
					if (candidateBlobs.Contains(b2))
					{
						numCandidates++;
					}
				}
				if (numCandidates < 2)
				{
					blobsToRemove.Add( b );
				}
			}
			if (sb != null)
			{
				sb.Append( "\n " + blobsToRemove.Count + " being removed in pass " + pass + " because not connected to >1 candidate" );
			}
			foreach (Blob b in blobsToRemove)
			{
				candidateBlobs.Remove( b );
			}
		}
		if (sb != null)
		{
			if (pass > 0)
			{
				sb.Append( "\n Finished after pass " + pass + " with " + candidateBlobs.Count + " candidates" );
			}
			else
			{
				sb.Append( "\n No more to remove" );
			}
		}
		if (candidateBlobs.Count > 5)
		{

		}
		if (sb!= null)
		{
			Debug.LogError( sb.ToString( ) );
		}
		return result;
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
