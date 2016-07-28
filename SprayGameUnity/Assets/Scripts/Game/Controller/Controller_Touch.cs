using UnityEngine;
using System.Collections;

public class Controller_Touch : Controller_Base
{
	static public Controller_Touch Create(Cannon c)
	{
		GameObject go = new GameObject( "TouchController" );
		Controller_Touch ct = go.AddComponent<Controller_Touch>( );
		ct.cachedTransform.position = Vector3.zero;
		ct.cachedTransform.localRotation = Quaternion.identity;
		ct.Init( c );
		return ct;
	}

	protected override void PostInit(Cannon c)
	{

	}

	public void Update()
	{
		if (Input.touchCount > 0)
		{
			Touch t = Input.touches[0];

			switch (t.phase)
			{
				case TouchPhase.Began:
				{
					doPointerDownAction( t.position );
					break;
				}
				case TouchPhase.Ended:
				{
					doPointerUpAction( t.position );
					break;
				}
				case TouchPhase.Moved:
				{
					doPointerMoveAction( t.position );
					break;
				}
				default:
				{
					if (DEBUG_CONTROLLER)
					{
						Debug.LogWarning( "Unhandled touch phase '" + t.phase.ToString( ) + "' at " + t.position );
					}
					break;
				}
			}
		}
	}
}
