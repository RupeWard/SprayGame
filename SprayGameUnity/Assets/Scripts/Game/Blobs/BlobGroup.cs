using UnityEngine;
using System.Collections;
using System.Collections.Generic;

abstract public class BlobGroup: RJWard.Core.IDebugDescribable
{
	private static readonly bool DEBUG_GROUPS = false;

	private List<Blob> blobs_ = new List<Blob>( );
	public List<Blob> blobs
	{
		get { return blobs_; }
	}

	protected string name_ = "UNNAMED";
	public string name
	{
		get { return name_;  }
	}
	public bool isConnectedToWall = false;

	static private int s_counter_ = 0;
	private int id_ = 0;

	private string seedName_ = "SEED";

	protected BlobGroup(string n, Blob seedBlob)
	{
		seedName_ = n;
		id_ = s_counter_;
		s_counter_++;
		SetName( );
	}

	protected virtual void SetName()
	{
		name_ = id_.ToString( ) + "_" + seedName_;
	}

	public bool ContainsBlob(Blob b)
	{
		return blobs_.Contains( b );
	}

	public void SetTypeTransitionState( BlobType_Base newType, float fraction )
	{
		for(int i = 0; i< blobs_.Count; i++)
		{
			blobs_[i].SetNewTypeFraction( newType, fraction );
		}
	}
	
	public void SetCountdownState( float fraction01 )
	{
		for (int i = 0; i < blobs_.Count; i++)
		{
			blobs_[i].SetCountdownState( fraction01 );
		}
	}

	public void SeedFrom(Blob seedBlob)
	{
		seedName_ = seedBlob.gameObject.name;
		SetName( );
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

	static private readonly bool DEBUG_PATHS = false;
	static private readonly bool DEBUG_PATHS_VERBOSE = false;

	private bool IsSubPath( List<Blob> bigPath, List<Blob> littlePath)
	{
		return IsSubPath( bigPath, littlePath, false );
	}

    private bool IsSubPath( List<Blob> bigPath, List<Blob> littlePath, bool doWrap)
	{
		bool result = false;
		if (IsSubPathHelper(bigPath, littlePath, doWrap))
		{
			result = true;
		}
		else
		{
			littlePath.Reverse( );
			if (IsSubPathHelper(bigPath, littlePath, doWrap))
			{
				result = true;
			}
			littlePath.Reverse( );
		}
		return result;
	}

	private bool IsSubPathHelper( List<Blob> bigPath, List<Blob> littlePath, bool doWrap )
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

	private bool IsSubSet( List<Blob> big, List<Blob> little)
	{
		bool result = false;
		if (big.Count >= little.Count)
		{
			result = true;
			for(int i=0; i< little.Count;i++)
			{
				if (!big.Contains(little[i]))
				{
					return false;
				}
			}
		}
		return result;
	}

	private bool ListContainsPath(List<List<Blob>> list, List<Blob> path)
	{
		for(int i= 0; i<list.Count;i++)
		{
			if (list[i].Count == path.Count && IsSubPath(list[i],path))
			{
				return true;
			}
		}
		return false;
	}

	List<List<Blob>> toRemoveListListBlob = new List<List<Blob>>();

	private bool AddPathPruningSubpaths(List<List<Blob>> paths, List<Blob> newOne, System.Text.StringBuilder debugsb, string debugstr, int sortdirection )
	{
		toRemoveListListBlob.Clear();
		for (int i = 0; i< paths.Count; i++)
		{
			List<Blob> l = paths[i];
			if (IsSubPath(l, newOne))
			{
				if (debugsb != null)
				{
					debugsb.Append( "\n     List "+debugstr+" already contains superset of path " );
					DebugDescribePath( newOne, debugsb );
					debugsb.Append( " superset = " );
					DebugDescribePath( l, debugsb );

				}
				if (DEBUG_PATHS_VERBOSE)
				{
					Debug.Log( "\n     List " + debugstr + " already contains superpath of path " + DebugDescribePathString( newOne ) );
					Debug.Log( " superset = " + DebugDescribePathString( l ) );
				}
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
					if (DEBUG_PATHS_VERBOSE)
					{
						Debug.Log( "\n     List " + debugstr + " contains subpath of path " + DebugDescribePathString( newOne ) );
						Debug.Log( " subset = " + DebugDescribePathString( l ) );
					}
					toRemoveListListBlob.Add( l );
				}
			}
		}
		for (int i = 0; i< toRemoveListListBlob.Count; i++)
		{
			paths.Remove( toRemoveListListBlob[i] );
		}
		paths.Add( newOne );
		if (sortdirection < 1)
		{
			paths.Sort( delegate ( List<Blob> a, List<Blob> b ) { return (b.Count - a.Count); } );
		}
		else if (sortdirection > 1)
		{
			paths.Sort( delegate ( List<Blob> a, List<Blob> b ) { return (a.Count - b.Count); } );
		}
		if (debugsb != null)
		{
			debugsb.Append( "\nAdded path to " + debugstr + ", list is now...\n" );
			for(int i =0; i< paths.Count; i++)
			{
				DebugDescribePath( paths[i], debugsb );
				debugsb.Append( "\n" );
			}
		}
		
		if (DEBUG_PATHS_VERBOSE)
		{
			Debug.Log( "\nAdded path to " + debugstr + ", list is now...\n" );
			for (int i =0; i< paths.Count; i++)
			{
				Debug.Log( "\n " + DebugDescribePathString( paths[i] ) );
			}
		}
		return true;
	}

	private bool AddPathPruningSubsets( List<List<Blob>> paths, List<Blob> newOne, System.Text.StringBuilder debugsb, string debugstr )
	{
        toRemoveListListBlob.Clear( );
		for(int i=0; i< paths.Count;i++)
		{
			List<Blob> l = paths[i];
            if (IsSubSet( l, newOne ))
			{
				if (debugsb != null)
				{
					debugsb.Append( "\n     List " + debugstr + " already contains superset of path " );
					DebugDescribePath( newOne, debugsb );
					debugsb.Append( " superset = " );
					DebugDescribePath( l, debugsb );

				}
				if (DEBUG_PATHS_VERBOSE)
				{
					Debug.Log( "\n     List " + debugstr + " already contains superset of path " + DebugDescribePathString( newOne ) );
					Debug.Log( " superset = " + DebugDescribePathString( l ) );
				}
				// already have a list which contains all in the new one
				return false;
			}
			else
			{
				if (IsSubSet( newOne, l ))
				{
					if (debugsb != null)
					{
						debugsb.Append( "\n     List " + debugstr + " contains subset of path " );
						DebugDescribePath( newOne, debugsb );
						debugsb.Append( " subset = " );
						DebugDescribePath( l, debugsb );
					}
					if (DEBUG_PATHS_VERBOSE)
					{
						Debug.Log( "\n     List " + debugstr + " contains subset of path " + DebugDescribePathString( newOne ) );
						Debug.Log( " subset = " + DebugDescribePathString( l ) );
					}
					toRemoveListListBlob.Add( l );
				}
			}
		}
		for (int i = 0; i< toRemoveListListBlob.Count; i++)
		{
			paths.Remove( toRemoveListListBlob[i] );
		}
		paths.Add( newOne );
		if (debugsb != null)
		{
			debugsb.Append( "\n     Added path to " + debugstr + ", list is now...\n" );
			for (int i = 0; i<paths.Count;i++)
			{
				debugsb.Append( "\n     " );
				DebugDescribePath( paths[i], debugsb );
			}
		}
		if (DEBUG_PATHS_VERBOSE)
		{
			Debug.Log( "\nAdded path to " + debugstr + ", list is now..." );
			for (int i=0; i<paths.Count; i++)
			{
				Debug.Log( "\n " + DebugDescribePathString( paths[i] ) );
			}
		}
		return true;
	}

	private bool AddPathPruningSupersets( List<List<Blob>> paths, List<Blob> newOne, System.Text.StringBuilder debugsb, string debugstr )
	{
		toRemoveListListBlob.Clear( );
		for (int i = 0; i < paths.Count; i++)
		{
			List<Blob> l = paths[i];
			if (IsSubSet( l, newOne ))
			{
				if (debugsb != null)
				{
					debugsb.Append( "\n     List " + debugstr + " already contains superset of path " );
					DebugDescribePath( newOne, debugsb );
					debugsb.Append( " superset = " );
					DebugDescribePath( l, debugsb );

				}
				if (DEBUG_PATHS_VERBOSE)
				{
					Debug.Log( "\n     List " + debugstr + " already contains superset of path " + DebugDescribePathString( newOne ) );
					Debug.Log( " superset = " + DebugDescribePathString( l ) );
				}
				// already have a list which is a subset of the new one
				toRemoveListListBlob.Add( l );
			}
			else
			{
				if (IsSubSet( newOne, l ))
				{
					if (debugsb != null)
					{
						debugsb.Append( "\n     List " + debugstr + " contains subset of path " );
						DebugDescribePath( newOne, debugsb );
						debugsb.Append( " subset = " );
						DebugDescribePath( l, debugsb );
					}
					if (DEBUG_PATHS_VERBOSE)
					{
						Debug.Log( "\n     List " + debugstr + " contains subset of path " + DebugDescribePathString( newOne ) );
						Debug.Log( " subset = " + DebugDescribePathString( l ) );
					}
					return false;
				}
			}
		}
		for (int i = 0; i < toRemoveListListBlob.Count; i++)
		{
			paths.Remove( toRemoveListListBlob[i] );
		}
		paths.Add( newOne );
		if (debugsb != null)
		{
			debugsb.Append( "\n     Added path to " + debugstr + ", list is now...\n" );
			for (int i = 0; i < paths.Count; i++)
			{
				debugsb.Append( "\n     " );
				DebugDescribePath( paths[i], debugsb );
			}
		}
		if (DEBUG_PATHS_VERBOSE)
		{
			Debug.Log( "\nAdded path to " + debugstr + ", list is now..." );
			for (int i = 0; i < paths.Count; i++)
			{
				Debug.Log( "\n " + DebugDescribePathString( paths[i] ) );
			}
		}
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

	static private readonly int MinToSurround = 6;

	private List<List<Blob>> candidatePaths = new List<List<Blob>>( );

	private List<List<Blob>> closedPaths = new List<List<Blob>>( );
	private List<Blob> candidateBlobs = new List<Blob>( );
	public List<List<Blob>> GetClosedPaths(  )
	{
		if (blobs_.Count < MinToSurround)
		{
			return null;
		}

		System.Text.StringBuilder sb = null;
		if (DEBUG_PATHS)
		{
			sb = new System.Text.StringBuilder( );
			sb.Append( "GetClosedPaths on group " + name_ + " with " + blobs_.Count + " blobs" );
		}
		if (DEBUG_PATHS_VERBOSE)
		{
			Debug.Log( "GetClosedPaths on group " + name_ + " with " + blobs_.Count + " blobs" );
		}
		closedPaths.Clear();
		candidateBlobs.Clear( );
		for (int i = 0; i<blobs_.Count; i++)
		{
			if (blobs_[i].connectedBlobs.Count > 1)
			{
				candidateBlobs.Add( blobs_[i] );
			}
		}
		if (sb!= null)
		{
			sb.Append( "\n " + candidateBlobs.Count + " with >1 connections" );
		}
		if (DEBUG_PATHS_VERBOSE)
		{
			Debug.Log( "\n " + candidateBlobs.Count + " with >1 connections" );
		}
		int numCandidateBlobs = 0;
		int pass = 0;
		while (numCandidateBlobs != candidateBlobs.Count)
		{
			numCandidateBlobs = candidateBlobs.Count;
			pass++;
			List<Blob> blobsToRemove = new List<Blob>( );
			Blob blobWith5 = null;
			for(int i =0; i<candidateBlobs.Count;i++)
			{
				Blob b = candidateBlobs[i];
				List<Blob> connectedBlobs = b.connectedBlobs;
				int numCandidates = 0;
				for (int i2 = 0; i2 < connectedBlobs.Count; i2++)
				{
					if (candidateBlobs.Contains(connectedBlobs[i2]))
					{
						numCandidates++;
					}
				}
				if (numCandidates < 2 || numCandidates >= MinToSurround)
				{
					blobsToRemove.Add( b );
				}
				else
				{
					if (numCandidates == 5)
					{
						blobWith5 = b;
					}
				}
			}
			if (sb != null)
			{
				sb.Append( "\n " + blobsToRemove.Count + " being removed in pass " + pass + " because not connected to >1 candidate" );
			}
			if (DEBUG_PATHS_VERBOSE)
			{
				Debug.Log( "\n " + blobsToRemove.Count + " being removed in pass " + pass + " because not connected to >1 candidate" );
			}
			if (blobsToRemove.Count > 0)
			{
				for (int i3 = 0; i3 < blobsToRemove.Count; i3++)
				{
					candidateBlobs.Remove( blobsToRemove[i3] );
				}
			}
			else
			{
				if (blobWith5 != null)
				{
					candidateBlobs.Remove( blobWith5 );
				}
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
		if (DEBUG_PATHS_VERBOSE)
		{
			if (pass > 0)
			{
				Debug.Log( "\n Finished after pass " + pass + " with " + candidateBlobs.Count + " candidates" );
			}
			else
			{
				Debug.Log( "\n No more to remove" );
			}
		}
		if (candidateBlobs.Count >= MinToSurround)
		{
			candidatePaths.Clear( );
			Blob currentCandidate = candidateBlobs[0];
			for (int i4 =0; i4 <currentCandidate.connectedBlobs.Count; i4++)
			{
				Blob b2 = currentCandidate.connectedBlobs[i4];
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
				sb.Append( "\n Created " + candidatePaths.Count + " initial candidate paths from first blob " + currentCandidate.gameObject.name );
			}
			if (DEBUG_PATHS_VERBOSE)
			{
				Debug.Log( "\n Created " + candidatePaths.Count + " initial candidate paths from first blob " + currentCandidate.gameObject.name );
			}

#if UNITY_EDITOR
			System.DateTime startTime = System.DateTime.Now;
			double maxSecs = 10;
#endif
			bool abort = false;
			while (candidatePaths.Count > 0 && !abort)
			{
				List<Blob> currentCandidatePath = candidatePaths[0];
				candidatePaths.RemoveAt( 0 );
				if (sb != null)
				{
					sb.Append( "\n Examining candidate path: " );
					DebugDescribePath( currentCandidatePath, sb );

					sb.Append( "\nRemaining candidate paths: " );
					for (int i5 =0; i5<candidatePaths.Count;i5++)
					{
						sb.Append( "\n  " );
						DebugDescribePath( candidatePaths[i5], sb );
					}
				}

				Blob lastBlob = currentCandidatePath[currentCandidatePath.Count - 1];
				if (DEBUG_PATHS_VERBOSE)
				{
					Debug.Log( "\n Examining candidate path: " + DebugDescribePathString( currentCandidatePath ) );
					Debug.Log( "\nRemaining candidate paths: " );
					for (int i6=0; i6<candidatePaths.Count; i6++)
					{
						Debug.Log( "\n  " + DebugDescribePathString( candidatePaths[i6] ) );
					}
				}

				for (int i7 = 0; i7 <lastBlob.connectedBlobs.Count; i7++)
				{
					Blob b2 = lastBlob.connectedBlobs[i7];
					if (candidateBlobs.Contains( b2 ))
					{
						int indexInCurrentPath = currentCandidatePath.IndexOf( b2 );
						if (indexInCurrentPath == -1 || indexInCurrentPath < currentCandidatePath.Count - 5)// don't complete too soon
						{
							if (indexInCurrentPath >= 0)
							{
								// found a loop
								List<Blob> newClosedPath = new List<Blob>( );
								for (int i8 = indexInCurrentPath; i8 < currentCandidatePath.Count; i8++)
								{
									newClosedPath.Add( currentCandidatePath[i8] );
								}
								if (newClosedPath.Count > 5)
								{
									if (sb != null)
									{
										sb.Append( "\n  Last blob of " );
										DebugDescribePath( currentCandidatePath, sb );
										sb.Append(" connects to " + b2.gameObject.name + ", making a closed path: " );
										DebugDescribePath( newClosedPath, sb );
									}
									if (DEBUG_PATHS_VERBOSE)
									{
										Debug.Log( "\n  Last blob of "+DebugDescribePathString(currentCandidatePath)
											+" connects to " + b2.gameObject.name + ", making a closed path: " + DebugDescribePathString( newClosedPath ) );
									}
									AddPathPruningSupersets( closedPaths, newClosedPath, sb, "Closed" );
//									AddPathPruningSubsets( closedPaths, newClosedPath, sb, "Closed" );
								}
								else
								{
									if (sb != null)
									{
										sb.Append( "\n  Discarding closed path containing only " +newClosedPath.Count+" blobs: " );
										DebugDescribePath( newClosedPath, sb );
									}
									if (DEBUG_PATHS_VERBOSE)
									{
										Debug.Log( "\n  Discarding closed path containing only " + newClosedPath.Count + " blobs: "+DebugDescribePathString( newClosedPath));
									}
								}
							}
							else
							{
								// found an extension
								List<Blob> newExtensionPath = new List<Blob>( );
								newExtensionPath.AddRange( currentCandidatePath );
								newExtensionPath.Add( b2 );
								AddPathPruningSubpaths( candidatePaths, newExtensionPath, sb, "Candidates", -1 );
								
								if (sb != null)
								{
									sb.Append( "\n Last blob connects to " + b2.gameObject.name + ", making an extended candidate path: " );
									DebugDescribePath( newExtensionPath, sb );
								}
								if (DEBUG_PATHS_VERBOSE)
								{
									Debug.Log( "\n  Last blob connects to " + b2.gameObject.name + ", making an extended candidate path: " + DebugDescribePathString( newExtensionPath ) );
								}
							}
						}
					}
				}
#if UNITY_EDITOR
				if (DEBUG_PATHS || DEBUG_PATHS_VERBOSE)
				{
					double timeTaken = (System.DateTime.Now - startTime).TotalSeconds;
					if (timeTaken > maxSecs)
					{
						abort = true;
						Debug.Log( "Aborting GetClosedPaths because " + timeTaken + " seconds passed" );
					}
				}
#endif
			}
			System.Text.StringBuilder newsb = null;
			if (sb != null || DEBUG_PATHS_VERBOSE)
			{
				newsb = new System.Text.StringBuilder( );
				newsb.Append( "Found " + closedPaths.Count + " closed paths: " );
				foreach (List<Blob> l in closedPaths)
				{
					newsb.Append( "\n  " );
					DebugDescribePath( l, newsb );
				}
			}
			if (sb != null)
			{
				sb.Append( "\n\n" ).Append( newsb.ToString( ) );
			}
			if (DEBUG_PATHS_VERBOSE)
			{
				Debug.Log( newsb.ToString( ) );
			}
		}
		else
		{
			if (sb != null)
			{
				sb.Append( "\n Not enough candidates to look for paths" );
			}
			if (DEBUG_PATHS_VERBOSE)
			{
				Debug.Log( "\n Not enough candidates to look for paths" );
			}
		}
		if (sb != null)
		{
			string output = sb.ToString( );
			do
			{
				int maxLength = 18000; // 15000 safe, 20000 not
				if (output.Length > maxLength)
				{
					Debug.LogError( output.Substring( 0, maxLength) );
					output = output.Substring( maxLength );
				}
				else
				{
					Debug.LogError( output);
					output = string.Empty;
				}
			} while (output.Length > 0);
        }
		return closedPaths;
    }

	public List<List<BlobGroup>> GetEnclosedGroups()
	{
		List<List<BlobGroup>> result = null;

        List<List<Blob>> closedPaths = GetClosedPaths( );
		if (closedPaths != null && closedPaths.Count > 0)
		{
			for (int i = 0; i<closedPaths.Count; i++)
			{
				List<BlobGroup> enclosed = new List<BlobGroup>( );
				List<Blob> l = closedPaths[i];
				List<BlobGroup> groups = GetEnclosedGroups( l );
				if (groups != null && groups.Count >0)
				{
					for (int i2 = 0; i2 < groups.Count; i2++)
					{
						BlobGroup group = groups[i2];
						enclosed.Add( group );
					}
				}
				if (enclosed.Count > 0)
				{
					if (result == null)
					{
						result = new List<List<BlobGroup>>( );
					}
					result.Add( enclosed );
				}

			}
		}
		if (result != null)
		{
			if (DEBUG_ENCLOSURE)
			{
				Debug.LogError( "Found " + result.Count + " enclosed groups" );
			}
		}
		return result;
	}

	private Rect GetBoundingBox(List<Blob> l)
	{
		Rect rect = new Rect( );
		rect.xMin = rect.xMax = l[0].cachedTransform.position.x;
		rect.yMin = rect.yMax = l[0].cachedTransform.position.y;
		for (int i = 1; i < l.Count; i++)
		{
			if (l[i].cachedTransform.position.x > rect.xMax)
			{
				rect.xMax = l[i].cachedTransform.position.x;
			}
			else if (l[i].cachedTransform.position.x < rect.xMin)
			{
				rect.xMin = l[i].cachedTransform.position.x;
			}
			if (l[i].cachedTransform.position.y > rect.yMax)
			{
				rect.yMax = l[i].cachedTransform.position.y;
			}
			else if (l[i].cachedTransform.position.y < rect.yMin)
			{
				rect.yMin = l[i].cachedTransform.position.y;
			}
		}
		return rect;
	}

	static public readonly bool DEBUG_ENCLOSURE = false;
	private List< BlobGroup > GetEnclosedGroups(List<Blob> closedPath)
	{
		List<BlobGroup> result = null;
		Rect boundingRect = GetBoundingBox( closedPath );
		List<Blob> blobsInBox = GameManager.Instance.GetBlobsInBox( boundingRect, closedPath[0].typeGroup.blobs );
		if (blobsInBox.Count > 0)
		{
			if (DEBUG_ENCLOSURE)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder( );
				sb.Append( "Found " + blobsInBox.Count + " blobs in box: " + DebugDescribePathString( blobsInBox ) );
				Debug.LogError( sb.ToString( ) );
			}

			List<Blob> enclosedBlobs = new List<Blob>( );

			Vector2 e = 0.01f * new Vector2(boundingRect.width, boundingRect.height); 
			for( int i = 0; i < blobsInBox.Count; i++)
			{
				Blob blobToCheck = blobsInBox[i]; //v1_1
				Vector2 rayStart = new Vector2(boundingRect.xMin, boundingRect.yMin) - e; //v1_2

				int intersections = 0;
				for (int i2 = 0; i2 < closedPath.Count; i2++)
				{
					Blob firstBlob = closedPath[i2];
					Blob secondBlob = (i2 == closedPath.Count - 1) ? (closedPath[0]) : (closedPath[i2 + 1]);

					if (areIntersecting( 
						blobToCheck.cachedTransform.position,
						rayStart,
						firstBlob.cachedTransform.position,
						secondBlob.cachedTransform.position))
					{
						intersections++;
					}
				}
				if ((intersections & 1 )==1)
				{
					if (DEBUG_ENCLOSURE)
					{
						Debug.Log( "Blob " + blobToCheck + " has " + intersections + " intersections so is inside" );
					}
					enclosedBlobs.Add( blobToCheck );
				}
				else
				{
					if (DEBUG_ENCLOSURE)
					{
						Debug.Log( "Blob " + blobToCheck + " has " + intersections + " intersections so is outside" );
					}
				}
			}
			if (enclosedBlobs.Count > 0)
			{
				result = new List<BlobGroup>( );
				for (int i = 0; i < enclosedBlobs.Count; i++)
				{
					if (!result.Contains( enclosedBlobs[i].typeGroup ))
					{
						result.Add( enclosedBlobs[i].typeGroup );
					}
				}
			}
		}

		
		return result;
	}

	public bool IsDIrectlyConnectedTo(BlobGroup other)
	{
		foreach (Blob b in blobs_)
		{
			List<Blob> connectedBlobs = b.connectedBlobs;
			for (int i = 0; i<connectedBlobs.Count; i++)
			{
				if (other.blobs_.Contains(connectedBlobs[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool areIntersecting(
		Vector2 v1_1,
		Vector2 v1_2,
		Vector2 v2_1,
		Vector2 v2_2)

	{
		float v1x1 = v1_1.x;
		float v1y1 = v1_1.y;
		float v1x2 = v1_2.x;
		float v1y2 = v1_2.y;
		float v2x1 = v2_1.x;
		float v2y1 = v2_1.y;
		float v2x2 = v2_2.x;
        float v2y2 = v2_2.y;
	
		float d1, d2;
		float a1, a2, b1, b2, c1, c2;

		// Convert vector 1 to a line (line 1) of infinite length.
		// We want the line in linear equation standard form: A*x + B*y + C = 0
		// See: http://en.wikipedia.org/wiki/Linear_equation
		a1 = v1y2 - v1y1;
		b1 = v1x1 - v1x2;
		c1 = (v1x2 * v1y1) - (v1x1 * v1y2);

		// Every point (x,y), that solves the equation above, is on the line,
		// every point that does not solve it, is not. The equation will have a
		// positive result if it is on one side of the line and a negative one 
		// if is on the other side of it. We insert (x1,y1) and (x2,y2) of vector
		// 2 into the equation above.
		d1 = (a1 * v2x1) + (b1 * v2y1) + c1;
		d2 = (a1 * v2x2) + (b1 * v2y2) + c1;

		// If d1 and d2 both have the same sign, they are both on the same side
		// of our line 1 and in that case no intersection is possible. Careful, 
		// 0 is a special case, that's why we don't test ">=" and "<=", 
		// but "<" and ">".
		if (d1 > 0 && d2 > 0) return false;
		if (d1 < 0 && d2 < 0) return false;

		// The fact that vector 2 intersected the infinite line 1 above doesn't 
		// mean it also intersects the vector 1. Vector 1 is only a subset of that
		// infinite line 1, so it may have intersected that line before the vector
		// started or after it ended. To know for sure, we have to repeat the
		// the same test the other way round. We start by calculating the 
		// infinite line 2 in linear equation standard form.
		a2 = v2y2 - v2y1;
		b2 = v2x1 - v2x2;
		c2 = (v2x2 * v2y1) - (v2x1 * v2y2);

		// Calculate d1 and d2 again, this time using points of vector 1.
		d1 = (a2 * v1x1) + (b2 * v1y1) + c2;
		d2 = (a2 * v1x2) + (b2 * v1y2) + c2;

		// Again, if both have the same sign (and neither one is 0),
		// no intersection is possible.
		if (d1 > 0 && d2 > 0) return false;
		if (d1 < 0 && d2 < 0) return false;

		// If we get here, only two possibilities are left. Either the two
		// vectors intersect in exactly one point or they are collinear, which
		// means they intersect in any number of points from zero to infinite.
		if ((a1 * b2) - (a2 * b1) == 0.0f) return false; // FIXME (shoudl be colinear)

		// If they are not collinear, they must intersect in exactly one point.
		return true;
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
