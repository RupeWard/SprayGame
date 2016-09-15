﻿using UnityEngine;
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
	
	private bool IsSubPath( List<Blob> bigPath, List<Blob> littlePath)
	{
		bool result = false;
		if (IsSubPathHelper(bigPath, littlePath))
		{
			result = true;
		}
		else
		{
			littlePath.Reverse( );
			if (IsSubPathHelper(bigPath, littlePath))
			{
				result = true;
			}
			littlePath.Reverse( );
		}
		return result;
	}

	private bool IsSubPathHelper( List<Blob> bigPath, List<Blob> littlePath )
	{
		bool result = false;
		if (littlePath.Count == 0)
		{
			Debug.LogError( "Empty little path" );
		}
		else
		{
			if (littlePath.Count < bigPath.Count)
			{
				int index = bigPath.IndexOf( littlePath[0] );
				if (index != -1)
				{
					for (int i = 1; i < littlePath.Count; i++)
					{
						int indexInBigPath = index + i;
						while (indexInBigPath >= bigPath.Count)
						{
							indexInBigPath -= bigPath.Count;
						}
						if (bigPath[indexInBigPath] == littlePath[i])
						{
							result = true;
							// match
						}
						else
						{
							result = false;
							break;
						}
					}
				}
			}
		}
		return result;
	}

	private bool AddPathPruningSubsets(List<List<Blob>> paths, List<Blob> newOne, System.Text.StringBuilder debugsb, string debugstr )
	{
		List<List<Blob>> toRemove = new List<List<Blob>>( );
		foreach (List<Blob> l in paths)
		{
			if (IsSubPath(l, newOne))
			{
				if (debugsb != null)
				{
					debugsb.Append( "\n     List "+debugstr+" already contains superset of path " );
					DebugDescribePath( newOne, debugsb );
					debugsb.Append( " superset = " );
					DebugDescribePath( l, debugsb );

				}
				/*
				Debug.Log( "\n     List " + debugstr + " already contains superset of path "+DebugDescribePathString( newOne));
				Debug.Log( " superset = " + DebugDescribePathString( l));
				*/
				// already have a list which contains all in the new one
				return false;
			}
			else
			{
				if (IsSubPath(newOne, l))
				{
					if (debugsb != null)
					{
						debugsb.Append( "\n     List " + debugstr + " contains subset of path " );
						DebugDescribePath( newOne, debugsb );
						debugsb.Append( " subset = " );
						DebugDescribePath( l, debugsb );
					}
					/*
					Debug.Log( "\n     List " + debugstr + " contains subset of path " +DebugDescribePathString( newOne));
					Debug.Log( " subset = "+DebugDescribePathString( l));
					*/
					toRemove.Add( l );
				}
			}
		}
		foreach (List<Blob> l in toRemove)
		{
			paths.Remove( l );
		}
		paths.Add( newOne );
		if (debugsb != null)
		{
			debugsb.Append( "\nAdded path to " + debugstr + ", list is now...\n" );
			foreach( List<Blob> l in paths)
			{
				DebugDescribePath( l, debugsb );
				debugsb.Append( "\n" );
			}
		} 
		/*
		Debug.Log( "\nAdded path to " + debugstr + ", list is now...\n" );
		foreach (List<Blob> l in paths)
		{
			Debug.Log( "\n "+DebugDescribePathString( l));
		}*/
		return true;
	}

	private void DebugDescribePath(List<Blob> l, System.Text.StringBuilder sb)
	{
		sb.Append( "(" );
		for (int i = 0; i < l.Count; i++)
		{
			if (i > 0) sb.Append( " " );
			sb.Append( l[i].gameObject.name );
		}
		sb.Append( ")" );
	}

	private string DebugDescribePathString( List<Blob> l )
	{
		System.Text.StringBuilder sb = new System.Text.StringBuilder( );
		DebugDescribePath( l, sb );
		return sb.ToString( );
    }

	public List<List<Blob>> GetClosedPaths(  )
	{
		System.Text.StringBuilder sb = null;
		if (DEBUG_PATHS)
		{
			sb = new System.Text.StringBuilder( );
			sb.Append( "GetClosedPaths on group " + name_ +" with "+blobs_.Count+" blobs");

//			Debug.Log( "GetClosedPaths on group " + name_ + " with " + blobs_.Count + " blobs" );
		}
		List<List<Blob>> closedPaths = new List<List<Blob>>();
		List<Blob> candidateBlobs = new List<Blob>( );
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
//			Debug.Log( "\n " + candidateBlobs.Count + " with >1 connections" );
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
//				Debug.Log( "\n " + blobsToRemove.Count + " being removed in pass " + pass + " because not connected to >1 candidate" );
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
//				Debug.Log( "\n Finished after pass " + pass + " with " + candidateBlobs.Count + " candidates" );
			}
			else
			{
				sb.Append( "\n No more to remove" );
//				Debug.Log( "\n No more to remove" );
			}
		}
		if (candidateBlobs.Count > 2)
		{
			List<List<Blob>> candidatePaths = new List<List<Blob>>( );
			Blob currentCandidate = candidateBlobs[0];
			foreach (Blob b2 in currentCandidate.connectedBlobs)
			{
				if (candidateBlobs.Contains( b2 ))
				{
					List<Blob> newCandidatePath = new List<Blob>( );
					newCandidatePath.Add( currentCandidate );
					newCandidatePath.Add( b2 );
					candidatePaths.Add( newCandidatePath );
				}
			}
			if (sb != null)
			{
				sb.Append( "\n Created "+candidatePaths.Count+" initial candidate paths from first blob "+currentCandidate.gameObject.name );
//				Debug.Log( "\n Created " + candidatePaths.Count + " initial candidate paths from first blob " + currentCandidate.gameObject.name );
			}
			while (candidatePaths.Count > 0)
			{
				List<Blob> currentCandidatePath = candidatePaths[0];
				candidatePaths.RemoveAt( 0 );
				if (sb != null)
				{
					sb.Append( "\n Examining candidate path: " );
					DebugDescribePath( currentCandidatePath, sb );

					sb.Append( "\nRemaining candidate paths: " );
					foreach (List<Blob> l in candidatePaths)
					{
						sb.Append( "\n  " );
						DebugDescribePath( l, sb );
					}
				}

				Blob lastBlob = currentCandidatePath[currentCandidatePath.Count - 1];
				/*
				Debug.Log( "\n Examining candidate path: " + DebugDescribePathString( currentCandidatePath ) );
				Debug.Log( "\nRemaining candidate paths: " );
				foreach (List<Blob> l in candidatePaths)
				{
					Debug.Log( "\n  "+DebugDescribePathString( l));
				}
				*/

				foreach (Blob b2 in lastBlob.connectedBlobs)
				{
					if (candidateBlobs.Contains( b2 ) )
					{
						int indexInCurrentPath = currentCandidatePath.IndexOf( b2 );
						if (indexInCurrentPath < currentCandidatePath.Count-2)// don't go straight back
						{
							if (indexInCurrentPath >= 0)
							{
								// found a loop
								List<Blob> newClosedPath = new List<Blob>( );
								for (int i = indexInCurrentPath; i < currentCandidatePath.Count; i++)
								{
									newClosedPath.Add( currentCandidatePath[i] );
								}
								if (sb != null)
								{
									sb.Append( "\n  Last blob connects to " + b2.gameObject.name +", making a closed path: " );
									DebugDescribePath( newClosedPath, sb );
								}
//								Debug.Log( "\n  Last blob connects to "+b2.gameObject.name+", making a closed path: " + DebugDescribePathString( newClosedPath ) );
								AddPathPruningSubsets( closedPaths, newClosedPath, sb, "Closed" );
							}
							else
							{
								// found an extension
								List<Blob> newExtensionPath = new List<Blob>( );
								newExtensionPath.AddRange( currentCandidatePath );
								newExtensionPath.Add( b2 );
								AddPathPruningSubsets( candidatePaths, newExtensionPath, sb, "Candidates" );
								if (sb != null)
								{
									sb.Append( "\n Last blob connects to "+b2.gameObject.name+", making an extended candidate path: " );
									DebugDescribePath( newExtensionPath, sb );
								}
//								Debug.Log( "\n  Last blob connects to " + b2.gameObject.name + ", making an extended candidate path: " + DebugDescribePathString( newExtensionPath ) );
							}

						}
					}
				}
			}
			if (sb != null)
			{
				System.Text.StringBuilder newsb = new System.Text.StringBuilder( );
				newsb.Append( "Found " + closedPaths.Count + " closed paths: " );
				foreach (List<Blob> l in closedPaths)
				{
					newsb.Append( "\n  " );
					DebugDescribePath( l, newsb );
				}
				sb.Append( "\n\n" ).Append( newsb.ToString( ) );
				Debug.Log( newsb.ToString( ) );
			}
		}
		else
		{
			if (sb != null)
			{
				sb.Append( "\n Not enough candidates to look for paths" );
			}
		}
		if (sb!= null)
		{
			Debug.LogError( sb.ToString( ) );
		}
		return closedPaths;
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
