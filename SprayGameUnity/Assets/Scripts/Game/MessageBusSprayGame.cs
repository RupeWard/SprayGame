using UnityEngine;
using System.Collections;

public partial class MessageBus
{
	public System.Action<Vector2> pointerDownAction;
	public void sendPointerDownAction(Vector2 v)
	{
		if (pointerDownAction != null)
		{
			pointerDownAction( v );
		}
		else
		{
			Debug.LogWarning( "No pointerDownAction" );
		}
	}

	public System.Action<Vector2> pointerUpAction;
	public void sendPointerUpAction( Vector2 v )
	{
		if (pointerUpAction != null)
		{
			pointerUpAction( v );
		}
		else
		{
			Debug.LogWarning( "No pointerUpAction" );
		}
	}

	public System.Action<Vector2> pointerMoveAction;
	public void sendPointerMoveAction( Vector2 v )
	{
		if (pointerMoveAction != null)
		{
			pointerMoveAction( v );
		}
		else
		{
			Debug.LogWarning( "No pointerMoveAction" );
		}
	}

	public System.Action<Vector2> pointerAbortAction;
	public void sendPointerAbortAction( Vector2 v )
	{
		if (pointerAbortAction != null)
		{
			pointerAbortAction( v );
		}
		else
		{
			Debug.LogWarning( "No pointerAbortAction" );
		}
	}

	public System.Action< Blob> firedBlobAction;
	public void sendFiredBlobAction( Blob b )
	{
		if (firedBlobAction != null)
		{
			firedBlobAction( b );
		}
		else
		{
			Debug.LogWarning( "No firedBlobAction" );
		}
	}

	public System.Action<Blob> blobFinishedAction;
	public void sendBlobFinishedAction( Blob b)
	{
		if (blobFinishedAction != null)
		{
			blobFinishedAction( b );
		}
		else
		{
			Debug.LogWarning( "No blobFinishedAction" );
		}
	}

	public System.Action<Blob, Blob> blobHitBlobAction;
	public void sendBlobHitBlobAction(Blob b0, Blob b1)
	{
		if (blobHitBlobAction != null)
		{
			blobHitBlobAction( b0, b1 );
		}
		else
		{
			Debug.LogWarning( "No blobhitblob action" );
		}
	}

	public System.Action<Blob, Wall> blobHitWallAction;
	public void sendBlobHitWallAction( Blob b, Wall w)
	{
		if (blobHitWallAction != null)
		{
			blobHitWallAction( b, w );
		}
		else
		{
			Debug.LogWarning( "No blobhitwall action" );
		}
	}

}