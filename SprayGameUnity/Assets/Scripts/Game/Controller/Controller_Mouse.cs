using UnityEngine;
using System.Collections;

public class Controller_Mouse : Controller_Base
{
	static public Controller_Mouse Create(Cannon c)
	{
		GameObject go = new GameObject( "MouseController" );
		Controller_Mouse cm = go.AddComponent<Controller_Mouse>( );
		cm.cachedTransform.position = Vector3.zero;
		cm.cachedTransform.localRotation = Quaternion.identity;
		cm.Init( c );
		return cm;
	}

	protected override void PostInit(Cannon c)
	{

	}

	public void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			doPointerDownAction( Input.mousePosition );
		}
		else if (Input.GetMouseButtonUp(0))
		{
			doPointerUpAction( Input.mousePosition );
		}
		else if (Input.GetMouseButton(0))
		{
			doPointerMoveAction( Input.mousePosition );
		}
		Event e = Event.current;
		if (e != null)
		{
			if (e.isMouse)
			{
				Vector2 pos = e.mousePosition;
				switch (e.type)
				{
					case EventType.MouseDown:
						{
							doPointerDownAction( pos );
							break;
						}
					case EventType.MouseUp:
						{
							doPointerUpAction( pos );
							break;
						}
					case EventType.MouseDrag:
						{
							doPointerMoveAction( pos );
							break;
						}
					default:
						{
							if (DEBUG_CONTROLLER)
							{
								Debug.Log( "Mouse event of type " + e.type.ToString( ) + " at " + pos );
							}
							break;
						}
				}
			}
			else
			{
				if (DEBUG_CONTROLLER)
				{
					Debug.Log( "Event of type " + e.type.ToString( ) );
				}
			}
		}
	}
}
