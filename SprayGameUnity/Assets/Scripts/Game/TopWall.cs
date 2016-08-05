using UnityEngine;
using System.Collections;

public class TopWall : MonoBehaviour
{
	private Transform cachedTransform_ = null;

	private void Awake()
	{
		cachedTransform_ = transform;
	}

}
