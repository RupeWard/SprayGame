﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlobManager : MonoBehaviour, RJWard.Core.IDebugDescribable
{
	private static readonly bool DEBUG_BLOBMANAGER = false;

	private List<BlobGroupConnected> connectedGroups_ = new List<BlobGroupConnected>( );
	private List<BlobGroupSameType> typeGroups_ = new List<BlobGroupSameType>( );
	private Dictionary< KeyValuePair<Blob, Blob>, BlobConnector_Base> blobConnectors_ = new Dictionary<KeyValuePair<Blob, Blob>, BlobConnector_Base>( );

    private Transform cachedTransform_ = null;

	private void Awake()
	{
		cachedTransform_ = transform;
		cachedTransform_.parent = GameManager.Instance.transform;
		cachedTransform_.localPosition = Vector3.zero;
		cachedTransform_.localRotation = Quaternion.identity;
		cachedTransform_.localScale = Vector3.one;
	}

	private void OnEnable()
	{
		registerHandlers( );
	}

	private void OnDisable()
	{
		deregisterHandlers( );
	}

	private void registerHandlers()
	{
		MessageBus.instance.blobHitBlobAction += HandleBlobHitBlob;
		MessageBus.instance.blobHitWallAction += HandleBlobHitWall;
		MessageBus.instance.firedBlobAction += HandleBlobFired;
		MessageBus.instance.gameOverAction += HandleGameOver;
	}

	private void deregisterHandlers( )
	{
		if (MessageBus.exists)
		{
			MessageBus.instance.blobHitBlobAction -= HandleBlobHitBlob;
			MessageBus.instance.blobHitWallAction -= HandleBlobHitWall;
			MessageBus.instance.firedBlobAction -= HandleBlobFired;
			MessageBus.instance.gameOverAction -= HandleGameOver;
		}
	}

	private bool gameOver_ = false;

	public void HandleGameOver()
	{
		foreach (GroupCountdownInfo gci in groupCountdownToDeletes_)
		{
			gci.Restart( );
		}
		groupCountdownToDeletes_.Clear( );
		groupCountdownToChangeTypes_.Clear( );
		typeGroupsToCheck_.Clear( );
		gameOver_ = true;
	}

	public void HandleBlobHitBlob(Blob b0, Blob b1)
	{
		if (DEBUG_BLOBMANAGER)
		{
			Debug.Log( "Blob " + b0.gameObject.name + " Collision with blob " + b1.gameObject.name );
		}
		//cachedRB_.velocity = Vector3.zero;

		bool b01 = b0.connectedBlobs.Contains( b1 );
		bool b10 = b1.connectedBlobs.Contains( b0 );

		if (b01 != b10)
		{
			Debug.LogError( "Containment mismatch, b01/b10 = " + b01 + "/" + b10 + " for " + b0.gameObject.name + " and " + b1.gameObject.name );
			return;
		}

		bool b0AlreadyHitAndInKillZone = (b0.IsInKillZone && b0.state == Blob.EState.Hit);
		bool b1AlreadyHitAndInKillZone = (b1.IsInKillZone && b1.state == Blob.EState.Hit);
		bool bEitherAlreadyHitAndInKillZone = (b0AlreadyHitAndInKillZone || b1AlreadyHitAndInKillZone);
		b0.SetHitState();
		b1.SetHitState( );

		if (!b01 )
		{
			SpringJoint joint = b0.gameObject.AddComponent<SpringJoint>( );
			joint.anchor = Vector3.zero;
			joint.connectedAnchor = Vector3.zero;

			float distance = 0.5f * (b0.radius + b1.radius);
			joint.minDistance = distance;
			joint.maxDistance = distance;
			joint.tolerance = GameManager.Instance.levelSettings.jointTolerance;
			joint.spring = 40000f;// FIXME magic
			joint.damper = 10000f;// FIXME magic
			joint.autoConfigureConnectedAnchor = false;
			joint.connectedBody = b1.cachedRB;
			joint.anchor = Vector3.zero;
			joint.connectedAnchor = Vector3.zero;

			b0.AddConnection( b1);
			b1.AddConnection( b0);

			BlobConnector_Base connection = BlobConnector_Base.CreateConnection( cachedTransform_, b0, b1);
			connection.joint = joint;
			blobConnectors_[new KeyValuePair<Blob, Blob>( b0, b1 )] = connection;

			if (b0.connectedGroup == b1.connectedGroup)
			{
//				Debug.LogWarning( "Blobs " + b0.gameObject.name + " and " + b1.gameObject.name + " are both already in connected group " + b0.connectedGroup.name );
			}
			else
			{
				MergeConnectedGroups( b0.connectedGroup, b1.connectedGroup );
			}

			if (GameManager.Instance.ShouldMergeTypeGroups( b0.typeGroup, b1.typeGroup))
			{
				BlobGroupSameType bgt = MergeTypeGroups( b0.typeGroup, b1.typeGroup );
				if (typeGroupsToCheck_.Contains( bgt ))
				{
					Debug.LogWarning( "Already checking " + bgt.DebugDescribe( ) );
				}
				else
				{
					if (!b1AlreadyHitAndInKillZone)
//						if (!bEitherAlreadyHitAndInKillZone)
					{
						typeGroupsToCheck_.Add( bgt );
					}
				}
			}
			else if (b0.typeGroup == b1.typeGroup)
			{
				CheckForEnclosedGroups(b0.typeGroup);
			}

			if (DEBUG_BLOBMANAGER)
			{
				Debug.Log( this.DebugDescribe( ) );
			}
		}
		if (b1AlreadyHitAndInKillZone)
//			if (bEitherAlreadyHitAndInKillZone)
		{
//			if (b0.IsInKillZone)
			{
				b0.HandleDeath( );
			}
//			if (b1.IsInKillZone)
			{
	//			b1.HandleDeath( );
			}
			MessageBus.instance.sendBlobHitInKillZoneAction( b0, b1 );
		}
	}

	private List<BlobGroupSameType> typeGroupsToCheck_ = new List<BlobGroupSameType>( );

	private void LateUpdate()
	{
		if (gameOver_)
		{
			typeGroupsToCheck_.Clear( );
			groupCountdownToDeletes_.Clear( );
			return;
		}
		int num = 0;
		for (int i = 0; i < typeGroupsToCheck_.Count; i++)
		{
			BlobGroupSameType bg = typeGroupsToCheck_[i];
			if (bg.blobType.ShouldDeleteGroupOfNum( bg.blobs.Count))
			{
				AddGroupCountdownToDelete( bg );
				num++;
			}
		}
		typeGroupsToCheck_.Clear( );

		List<GroupCountdownInfo> toRemove = new List<GroupCountdownInfo>( );
		for (int i = 0; i < groupCountdownToChangeTypes_.Count; i++)
		{
			GroupCountdownToChangeTypeInfo gci = groupCountdownToChangeTypes_[i];
            gci.Update( );
			if (gci.finished)
			{
				toRemove.Add( gci );
			}
		}
		for (int i = 0; i < toRemove.Count; i++)
		{
			groupCountdownToChangeTypes_.Remove( toRemove[i] as GroupCountdownToChangeTypeInfo );
		}

		toRemove.Clear( );
		for(int i = 0; i < groupCountdownToDeletes_.Count; i++)
		{
			GroupCountdownInfo gci = groupCountdownToDeletes_[i];
            gci.Update( );
			if (gci.finished)
			{
				toRemove.Add( gci );
			}
		}
		for (int i = 0; i < toRemove.Count; i++)
		{
			groupCountdownToDeletes_.Remove( toRemove[i] );
		}

		for (int i = 0; i <groupsToDelete_.Count; i++)
		{
			BlobGroupSameType g = groupsToDelete_[i];
			if (g.blobs.Count > 0)
			{
				GameManager.Instance.PlayDeleteClip( );
			}
			DeleteBlobTypeGroup( g );
		}
		groupsToDelete_.Clear( );
	}

	private void AddGroupCountdownToChangeType(BlobGroupSameType bg, BlobType_Base newType)
	{
		GroupCountdownToChangeTypeInfo gci = null;
		for (int i = 0; i< groupCountdownToChangeTypes_.Count; i++)
		{
			GroupCountdownToChangeTypeInfo info = groupCountdownToChangeTypes_[i];
            if (info.group == bg)
			{
				gci = info;
				gci.newType = newType;
				gci.Restart( );
//				GameManager.Instance.PlayRestartCountdownClip( );
				if (DEBUG_BLOBMANAGER)
				{
					Debug.Log( "Restarting type change countdown for group " + bg.name );
				}
				break;
			}
		}
		if (gci == null)
		{
			if (DEBUG_BLOBMANAGER)
			{
				Debug.Log( "Adding countdown to change type for group " + bg.name );
			}
			groupCountdownToChangeTypes_.Add( new GroupCountdownToChangeTypeInfo( bg, newType, GameManager.Instance.levelSettings.groupTypeChangeCountdown, HandleCountdownToTypeChangeFinished ) );
		}

	}

	private void AddGroupCountdownToDelete( BlobGroupSameType bg)
	{
		GroupCountdownInfo gci = null;
		for (int i=0; i<groupCountdownToDeletes_.Count; i++)
		{
			GroupCountdownInfo info = groupCountdownToDeletes_[i];
            if (info.group == bg)
			{
				gci = info;
				gci.Restart( );
				GameManager.Instance.PlayRestartCountdownClip( );
				if (DEBUG_BLOBMANAGER)
				{
					Debug.Log( "Restarting countdown for group " + bg.name );
				}
				break;
			}
		}
		if (gci == null)
		{
			if (DEBUG_BLOBMANAGER)
			{
				Debug.Log( "Adding countdown for group " + bg.name );
			}
			groupCountdownToDeletes_.Add( new GroupCountdownInfo( bg, GameManager.Instance.levelSettings.groupDeleteCountdown, HandleCountdownToDeleteFinished ) );
		}
	}

	private void HandleCountdownToDeleteFinished(GroupCountdownInfo info)
	{
		if (DEBUG_BLOBMANAGER)
		{
			Debug.Log( "Countdown to delete finished for group " + info.group.name );
		}
		if (!groupsToDelete_.Contains( info.group ))
		{
			groupsToDelete_.Add( info.group );
		}
	}

	private void HandleCountdownToTypeChangeFinished(GroupCountdownInfo info)
	{
		GroupCountdownToChangeTypeInfo info2 = info as GroupCountdownToChangeTypeInfo;
		if (info2 == null)
		{
			Debug.LogError( "Wrong type!" );
		}
		else
		{
			// change group type, looks for mergers
			info2.group.ChangeType( info2.newType);
			CheckForConnectionsToSameTypeGroups( info2.group );
		}
	}

	private List<BlobGroupSameType> groupsToDelete_ = new List<BlobGroupSameType>( );

	private class GroupCountdownInfo
	{
		public BlobGroupSameType group;
		public float startTime;
		public float endTime;
		public float duration;
		public float elapsed;

		public System.Action<GroupCountdownInfo> endAction;

		public GroupCountdownInfo( BlobGroupSameType g, float durn, System.Action<GroupCountdownInfo> ea)
		{
			elapsed = 0f;
			duration = durn;
			group = g;
			startTime = Time.time;
			endTime = startTime + durn;
			endAction = ea;
		}

		public virtual void Restart()
		{
			elapsed = 0f;
			startTime = Time.time;
			endTime = startTime + duration;
			group.SetCountdownState( 0f );
		}

		public bool finished = false;

		public virtual void Update()
		{
			elapsed += Time.deltaTime * GameManager.Instance.blobGroupDeleteSpeed;
			float fraction = elapsed/ duration;
			group.SetCountdownState( fraction );
			if (fraction >= 1f)
			{
				finished = true;
				if (endAction != null)
				{
					endAction( this );
				}
				else
				{
					Debug.LogWarning( "No endAction" );
				}
			}
		}
	}

	private class GroupCountdownToChangeTypeInfo : GroupCountdownInfo
	{
		public BlobType_Base newType;
		public GroupCountdownToChangeTypeInfo( BlobGroupSameType g, BlobType_Base nt, float durn, System.Action<GroupCountdownInfo> ea ) 
			: base( g, durn, ea)
		{
			newType = nt;
		}

		public override void Update( )
		{
			elapsed += Time.deltaTime * GameManager.Instance.blobGroupDeleteSpeed;
			float fraction = elapsed / duration;
			group.SetTypeTransitionState( newType, fraction );
			if (fraction >= 1f)
			{
				finished = true;
				if (endAction != null)
				{
					endAction( this );
				}
				else
				{
					Debug.LogWarning( "No endAction" );
				}
			}
		}
		public override void Restart( )
		{
			elapsed = 0f;
			startTime = Time.time;
			endTime = startTime + duration;
			group.SetTypeTransitionState( newType, 0f );
		}

	}

	private List<GroupCountdownInfo> groupCountdownToDeletes_ = new List<GroupCountdownInfo>( );
	private List<GroupCountdownToChangeTypeInfo> groupCountdownToChangeTypes_ = new List<GroupCountdownToChangeTypeInfo>( );

	private BlobGroupConnected MergeConnectedGroups( BlobGroupConnected retainGroup, BlobGroupConnected loseGroup )
	{
		if (retainGroup == loseGroup)
		{
			Debug.LogError( "Identical params" );
			return retainGroup;
		}
		if (loseGroup != null)
		{
			if (loseGroup.isConnectedToWall)
			{
				retainGroup.isConnectedToWall = true;
			}
			if (loseGroup.blobs != null)
			{
				foreach (Blob b in loseGroup.blobs)
				{
					b.connectedGroup = retainGroup;
					retainGroup.blobs.Add( b );
				}
				loseGroup.blobs.Clear( );
			}
			else
			{
				Debug.LogError( "null loseGroup.blobs" );
			}
		}
		else 
		{
			Debug.LogError( "null loseGroup" );
		}
		connectedGroups_.Remove( loseGroup );
		return retainGroup;
	}
	
	private BlobGroupSameType MergeTypeGroupsWithoutCheckingForEnclosures( BlobGroupSameType retainGroup, BlobGroupSameType loseGroup)
	{
		return MergeTypeGroups( retainGroup, loseGroup, false );
    }

	private BlobGroupSameType MergeTypeGroups( BlobGroupSameType retainGroup, BlobGroupSameType loseGroup )
	{
		return MergeTypeGroups( retainGroup, loseGroup, true );
	}

	private BlobGroupSameType MergeTypeGroups( BlobGroupSameType retainGroup, BlobGroupSameType loseGroup, bool checkForEnclosures )
	{
		if (retainGroup == loseGroup)
		{
			Debug.LogError( "Identical params" );
			return retainGroup;
		}
		for (int i = 0; i <loseGroup.blobs.Count; i++)
		{
			Blob b = loseGroup.blobs[i];
			b.typeGroup = retainGroup;
			retainGroup.blobs.Add( b );
		}
		loseGroup.blobs.Clear( );
		typeGroups_.Remove( loseGroup );
		if (loseGroup.isConnectedToWall)
		{
			retainGroup.isConnectedToWall = true;
		}
		if (typeGroupsToCheck_.Contains( loseGroup ))
		{
			typeGroupsToCheck_.Remove( loseGroup );
		}
		if (checkForEnclosures)
		{
			CheckForEnclosedGroups( retainGroup );
		}
		return retainGroup;
	}

	public void CheckForEnclosedGroups( BlobGroup group )
	{
		List<BlobGroup> enclosedGroups = group.GetEnclosedGroups( );
		if (enclosedGroups != null && enclosedGroups.Count > 0)
		{
			BlobGroupSameType bgst = group as BlobGroupSameType;
			if (bgst == null)
			{
				Debug.LogError( "enclosing group isn't a type group!" );
			}
			else
			{
				if (bgst.blobType.name == "FIXED")
				{
					// if only one enclosed type, change to that type, then merge with it, and add to groups to check for number
					if (enclosedGroups.Count == 1)
					{
						BlobGroupSameType bgst_e = enclosedGroups[0] as BlobGroupSameType;
						if (bgst == null)
						{
							Debug.LogError( "Enclosed group isn't a type group!" );
						}
						else
						{
							if (BlobGroup.DEBUG_ENCLOSURE)
							{
								Debug.Log( "Changing type of enclosing group " + bgst.name + " to " + bgst_e.blobType.name );
							}
							AddGroupCountdownToChangeType( bgst, bgst_e.blobType );
//							bgst.ChangeType( bgst_e.blobType );
//							BlobGroupSameType newGroup = MergeIntoIfConnected( bgst_e, bgst );
//							CheckForConnectionsToSameTypeGroups( newGroup );
						}
					}
				}
				else
				{
					// change all enclosed groups and merge into group, then add to groups to check for number
					for (int i=0; i<enclosedGroups.Count; i++)
					{
						BlobGroup bg = enclosedGroups[i];
						BlobGroupSameType bgst_e = bg as BlobGroupSameType;
						if (bgst_e == null)
						{
							Debug.LogError( "Enclosed group isn't a type group!" );
						}
						else
						{
							Debug.Log( "Changing type of enclosed group " + bgst_e.name + " to " + bgst.blobType.name );
							AddGroupCountdownToChangeType( bgst_e, bgst.blobType );
//							bgst_e.ChangeType( bgst.blobType );
//							MergeIntoIfConnected( bgst, bgst_e );
						}
					}
				}
			}
		}
	}

	private static bool PathsHasMoreBlobs( List<Blob> a, List<Blob> b)
	{
		return a.Count > b.Count;
	}

	private void CheckForConnectionsToSameTypeGroups(BlobGroupSameType seedGroup)
	{
		List<BlobGroupSameType> groupsFound = new List<BlobGroupSameType>( );
		for (int i=0; i<seedGroup.blobs.Count;i++)
		{
			for (int j=0; j < seedGroup.blobs[i].connectedBlobs.Count; j++)
			{
				Blob cb = seedGroup.blobs[i].connectedBlobs[j];
                if (cb.typeGroup != seedGroup && cb.blobType == seedGroup.blobType && !groupsFound.Contains(cb.typeGroup))
				{
					groupsFound.Add( cb.typeGroup );
				}
			}
		}
		if (groupsFound.Count > 0)
		{
			if (BlobGroup.DEBUG_ENCLOSURE)
			{
				Debug.Log( "Found " + groupsFound.Count + " connected groups of same type, merging" );
			}
			for (int i=0; i < groupsFound.Count; i++)
			{
				BlobGroupSameType group = groupsFound[i];
				BlobGroupSameType bgt = MergeTypeGroupsWithoutCheckingForEnclosures( seedGroup, group );
				if (typeGroupsToCheck_.Contains( bgt ))
				{
					if (BlobGroup.DEBUG_ENCLOSURE)
					{
						Debug.LogWarning( "Already checking " + bgt.DebugDescribe( ) );
					}
				}
				else
				{
					typeGroupsToCheck_.Add( bgt );
				}
			}

		}
	}

	private BlobGroupSameType MergeIntoIfConnected( BlobGroupSameType retain, BlobGroupSameType lose)
	{
		BlobGroupSameType bgt = null;
        if (retain.IsDIrectlyConnectedTo( lose ))
		{
			Debug.Log( "Merging groups " + retain.DebugDescribe( ) + " and " + lose.DebugDescribe( ));
			bgt = MergeTypeGroupsWithoutCheckingForEnclosures( retain, lose );
			if (typeGroupsToCheck_.Contains( bgt ))
			{
				Debug.LogWarning( "Already checking " + bgt.DebugDescribe( ) );
			}
			else
			{
				typeGroupsToCheck_.Add( bgt );
			}
			return bgt;
		}
		else
		{
			Debug.Log( "Not merging groups "+retain.DebugDescribe()+" and "+lose.DebugDescribe()+" because not directly connected" );
		}
		return bgt;
	}

	private void DeleteBlobTypeGroup(BlobGroupSameType bg)
	{
		System.Text.StringBuilder sb = null;
		if (DEBUG_BLOBMANAGER)
		{
			sb = new System.Text.StringBuilder( );
			sb.Append( "Deleting BG " + bg.name );
		}
		List<KeyValuePair<Blob, Blob>> connectionsToRemove = new List<KeyValuePair<Blob, Blob>>( );
		 
		for (int i = 0; i<bg.blobs.Count; i++)
		{
			Blob b = bg.blobs[i];
			for (int j =0; j<b.connectedBlobs.Count; j++)
			{
				Blob otherB = b.connectedBlobs[j];
//				if (! bg.blobs.Contains( otherB ))
				{
					if (!connectionsToRemove.Contains( new KeyValuePair<Blob, Blob>( b, otherB ) ) && !connectionsToRemove.Contains( new KeyValuePair<Blob, Blob>( otherB, b ) ))
					{
						connectionsToRemove.Add( new KeyValuePair<Blob, Blob>( b, otherB ) );
					}
				}
			}
		}
		if (sb !=null)
		{
			sb.Append( "\n Found " + connectionsToRemove.Count + " connections to remove" );
		}
		for (int i =0; i < connectionsToRemove.Count; i++)
		{
			KeyValuePair<Blob, Blob> kvp = connectionsToRemove[i];
            if (sb!= null)
			{
				sb.Append( "\n Removing connection " + kvp.Key.name + " to " + kvp.Value.name );
			}
			RemoveConnection( kvp.Key, kvp.Value );
		}
		if (sb!= null)
		{
			sb.Append( "\n " + bg.blobs.Count + " blobs to delete" );
		}
		for (int i = 0; i < bg.blobs.Count; i++)
		{
			Blob b = bg.blobs[i];
			if (sb != null)
			{
				sb.Append( "\n  Deleting" + b.name );
			}
			b.connectedGroup.blobs.Remove( b );
			if (b.connectedGroup.blobs.Count == 0)
			{
				if (sb != null)
				{
					sb.Append( "\n  Deleting empty connected group " + b.connectedGroup.name );
				}
				connectedGroups_.Remove( b.connectedGroup );
			}
			GameObject.Destroy( b.gameObject );			
		}
		if (sb != null)
		{
			sb.Append( "\nRemoving group from list" );
		}
		typeGroups_.Remove( bg );

		List<GroupCountdownInfo> toRemove = new List<GroupCountdownInfo>( );
		for (int i = 0; i < groupCountdownToDeletes_.Count; i++)
		{
			GroupCountdownInfo info = groupCountdownToDeletes_[i];
            if (info.group == bg)
			{
				toRemove.Add( info );
			}
		}
		for (int i = 0; i< toRemove.Count; i++)
		{
			groupCountdownToDeletes_.Remove( toRemove[i] );
		}

		if (sb!= null)
		{
			Debug.Log( sb.ToString( ) );
		}
	}

	private BlobConnector_Base FindConnection(Blob b0, Blob b1, ref KeyValuePair<Blob, Blob> kvp)
	{
		BlobConnector_Base bcb;
		kvp = new KeyValuePair<Blob, Blob>( b0, b1 );
		if (!blobConnectors_.TryGetValue( kvp, out bcb ))
		{
			kvp = new KeyValuePair<Blob, Blob>( b1, b0 );
			blobConnectors_.TryGetValue( kvp, out bcb );
		}
		return bcb;
	}

	private void RemoveConnection(Blob b0, Blob b1)
	{
		KeyValuePair<Blob, Blob> kvp = new KeyValuePair<Blob, Blob>( null, null );
		BlobConnector_Base bcb = FindConnection(b0, b1, ref kvp );

		if (bcb != null)
		{
			blobConnectors_.Remove( kvp );
			GameObject.Destroy( bcb.gameObject );
		}
		else
		{
			Debug.LogError( "On Delete, failed to find connection between " + b0.name + " and " + b1.name );
		}
	}

	public void HandleBlobHitWall( Blob b, Wall w )
	{
		b.connectedGroup.isConnectedToWall = true;
		b.typeGroup.isConnectedToWall = true;
	}

	private List<Blob> currentBlobs = new List<Blob>( );

	public void HandleBlobDestroyed(Blob b)
	{
		if (currentBlobs.Contains(b))
		{
			currentBlobs.Remove( b );
		}
		else
		{
			Debug.LogError( "Blob " + b.gameObject.name + " not in list on destroy" );
		}
	}

	public List<Blob> GetBlobsInBox( Rect rect, ICollection<Blob> excluding )
	{
		return GetBlobsInBox( rect.xMin, rect.yMin, rect.xMax, rect.yMax, excluding );
	}

	public List<Blob> GetBlobsInBox( Vector2 min, Vector2 max, ICollection<Blob> excluding )
	{
		return GetBlobsInBox( min.x, min.y, max.x, max.y, excluding );
	}

	public List<Blob> GetBlobsInBox(float minx, float miny, float maxx, float maxy, ICollection<Blob> excluding )
	{
		List<Blob> result = new List<Blob>( );
		for (int i = 0;  i < currentBlobs.Count; i++)
		{
			if (!excluding.Contains(currentBlobs[i]))
			{
				Vector3 pos = currentBlobs[i].cachedTransform.position;
				if (pos.x >= minx && pos.y >= miny && pos.x <= maxx && pos.y <= maxy)
				{
					result.Add( currentBlobs[i] );
				}
			}
		}
		return result;
	}

	public void HandleBlobFired( Blob b )
	{
		currentBlobs.Add( b );

		BlobGroupConnected cGroup = new BlobGroupConnected( b );
		BlobGroupSameType tGroup = new BlobGroupSameType( b );

		connectedGroups_.Add( cGroup );
		typeGroups_.Add( tGroup );

		b.connectedGroup = cGroup;
		b.typeGroup = tGroup;
	}

	public void DebugDescribe(System.Text.StringBuilder sb)
	{
		sb.Append( "BlobManager: " );
		sb.Append( " \n" + connectedGroups_.Count + " connected Groups" );
		foreach (BlobGroup bg in connectedGroups_)
		{
			sb.Append( "\n" );
			bg.DebugDescribe( sb );
		}
		sb.Append( " \n" + typeGroups_.Count + " type Groups" );
		foreach (BlobGroup bg in typeGroups_)
		{
			sb.Append( "\n" );
			bg.DebugDescribe( sb );
		}
		sb.Append( " \n" + blobConnectors_.Count + " connectors" );
	}
}
