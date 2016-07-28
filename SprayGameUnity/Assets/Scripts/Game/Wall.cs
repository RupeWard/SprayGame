using UnityEngine;
using System.Collections;

public class Wall : MonoBehaviour
{
	public enum ETriBehaviour
	{
		Always,
		Never,
		Sometimes
	}

	#region inspector data

	public ETriBehaviour stickiness = ETriBehaviour.Sometimes;

	#endregion inspector data
}
