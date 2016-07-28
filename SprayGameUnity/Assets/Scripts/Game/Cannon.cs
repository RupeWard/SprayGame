using UnityEngine;
using System.Collections;

public class Cannon : MonoBehaviour
{
	static private readonly bool DEBUG_CANNON = true;

	private Transform cachedTransform_ = null;
	public Transform cachedTransform
	{
		get { return cachedTransform_; }
	}

	private void Awake( )
	{
		cachedTransform_ = transform;
	}

	private void Start()
	{
		addListeners( );
	}

	private void addListeners()
	{
		MessageBus.instance.pointerDownAction += HandlePointerDown;
		MessageBus.instance.pointerUpAction += HandlePointerUp;
		MessageBus.instance.pointerMoveAction += HandlePointerMove;
	}

	private void removeListeners( )
	{
		MessageBus.instance.pointerDownAction -= HandlePointerDown;
		MessageBus.instance.pointerUpAction -= HandlePointerUp;
		MessageBus.instance.pointerMoveAction -= HandlePointerMove;
	}

	private void PointAt(Vector2 v)
	{
		float angle = Mathf.Rad2Deg * Mathf.Atan2( v.y, v.x );
		if (DEBUG_CANNON)
		{
			Debug.Log( "Cannon Angle is " + angle );
		}

		cachedTransform_.rotation = Quaternion.Euler( new Vector3( 0f,0f,angle - 90f ) );
	}

	public void HandlePointerDown(Vector2 v)
	{
		if (DEBUG_CANNON)
		{
			Debug.Log( "Cannon: Ptr DOWN at " + v );
		}
		PointAt( v );
	}

	public void HandlePointerUp( Vector2 v )
	{
		PointAt( v );
		if (DEBUG_CANNON)
		{
			Debug.Log( "Cannon: Ptr UP at " + v );
		}
	}

	public void HandlePointerMove( Vector2 v )
	{
		PointAt( v );
		if (DEBUG_CANNON)
		{
			Debug.Log( "Cannon: Ptr MOVE at " + v );
		}
	}
}
