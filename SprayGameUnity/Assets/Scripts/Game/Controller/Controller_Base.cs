﻿using UnityEngine;
using System.Collections;

public class Controller_Base : MonoBehaviour
{
	static protected readonly bool DEBUG_CONTROLLER = false;

	#region private data

	private float movementThreshold_ = 0.01f;
	private Vector2 previousPosition_ = Vector2.zero;

	#endregion private data

	#region private hooks

	private Cannon cannon_ = null;

	private Transform cachedTransform_ = null;
	public Transform cachedTransform
	{
		get { return cachedTransform_;  }
	}

	#endregion private hooks

	private void Awake()
	{
		cachedTransform_ = transform;
	}

	protected virtual void PostAwake()
	{

	}

	protected void Init(Cannon c)
	{
		cannon_ = c;
		
		if (DEBUG_CONTROLLER)
		{
			Debug.Log( "Cannon at " + Camera.main.WorldToScreenPoint( cannon_.cachedTransform.position ) );
		}
		PostInit( c );
	}

	protected virtual void PostInit(Cannon c)
	{

	}

	protected void doPointerDownAction(Vector2 v)
	{
		v = GetPositionRelativeToCannon( v );
		previousPosition_ = v;
		if (v.y > 0f)
		{
			MessageBus.instance.sendPointerDownAction( v );
		}
	}

	protected void doPointerUpAction( Vector2 v )
	{
		v = GetPositionRelativeToCannon( v );
		previousPosition_ = v;
		if (v.y > 0f)
		{
			MessageBus.instance.sendPointerUpAction( v );
		}
		else
		{
			MessageBus.instance.sendPointerAbortAction( v );
		}
	}

	protected void doPointerMoveAction( Vector2 v )
	{
		v = GetPositionRelativeToCannon( v );
		if (Vector2.Distance(v, previousPosition_) >= movementThreshold_)
		{
			previousPosition_ = v;

			if (v.y > 0f)
			{
				MessageBus.instance.sendPointerMoveAction( v );
			}
			/*
			else
			{
				float angle = Mathf.Rad2Deg * Mathf.Atan2( v.y, v.x );
				Debug.Log( "Aborting at " + angle );
				MessageBus.instance.sendPointerAbortAction( v );
			}*/
		}
	}

	protected void doPointerAbortAction( Vector2 v )
	{
		v = GetPositionRelativeToCannon( v );
		MessageBus.instance.sendPointerAbortAction( v );
	}

	private Vector2 GetPositionRelativeToCannon(Vector2 v)
	{
		Vector2 screenPos = Camera.main.WorldToScreenPoint( cannon_.cachedTransform.position );
        return (v - screenPos);
    }
}
