using UnityEngine;
using System;
using System.Collections;

public partial class MessageBus : MonoBehaviour
{
	private static MessageBus _instance;

	//	public Action<BOotsaData> bootsChanged;

	public static MessageBus instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new GameObject("MessageBus").AddComponent<MessageBus>();
			}			
			return _instance;
		}
	}

	/*
	public void dispatchBootsChanged(BootsData bd)
	{
		if (bootsChanged != null)
		{
//			Debug.Log ("Sending boots changed to " + bd.id);
			bootsChanged (bd);
		} 
		else
		{
			Debug.LogWarning ("No boots changed action ");
		}
	}
	*/

	public void clear()
	{
//		bootsChanged = null;
	}
}