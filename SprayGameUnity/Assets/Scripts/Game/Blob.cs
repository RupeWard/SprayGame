using UnityEngine;
using System.Collections;

public class Blob : MonoBehaviour
{
	static private readonly bool DEBUG_BLOB = true;

	#region private hooks

	private Transform cachedTransform_ = null;
	public Transform cachedTransform
	{
		get { return cachedTransform_;  }
	}

	private Rigidbody cachedRB_ = null;
	public Rigidbody cachedRB
	{
		get { return cachedRB_; }
	}


	#endregion private hooks

	private void Awake()
	{
		cachedTransform_ = transform;
		cachedRB_ = GetComponent<Rigidbody>( );
		PostAwake( );
	}

	protected virtual void PostAwake()
	{

	}

	public void Init(Cannon cannon)
	{
		cachedTransform.position = cannon.cachedTransform.position;
		cachedTransform.rotation = cannon.cachedTransform.rotation;

	}

	private void OnCollisionEnter( Collision c)
	{
		if (DEBUG_BLOB)
		{
			Debug.Log( "Blob Collision with " + c.gameObject.name );
		}
	}
}
