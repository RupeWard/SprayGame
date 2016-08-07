using UnityEngine;
using System.Collections;

public class Blob_SimpleCylinder : Blob
{
	#region private hooks

	public MeshRenderer myRenderer = null;

	public Transform top;

	#endregion private hooks

	protected override void PostAwake( )
	{
		radius_ = cachedTransform.localScale.x;
		 
		cachedMaterial_ = new Material( myRenderer.sharedMaterial );
		myRenderer.sharedMaterial = cachedMaterial_;
		cachedTransform.localScale = new Vector3 ( radius, 0.5f, radius);

		top.gameObject.SetActive( false );
	}

	protected override void SetAppearanceByType(BlobType_Base t)
	{
		t.SetBlobAppearance( this );
	}

	override public void Init( Cannon cannon )
	{
		cachedTransform.position = cannon.cachedTransform.position;
		cachedTransform.rotation = cannon.cachedTransform.rotation;

		top.gameObject.SetActive( false );
	}

	override public void SetCountdownState( float fraction01 )
	{
		fraction01 = Mathf.Clamp01( fraction01 );
		if (fraction01 == 0f)
		{
			top.gameObject.SetActive( false );
		}
		else
		{
			top.gameObject.SetActive( true );
			top.localScale = new Vector3( fraction01, fraction01, 1f );
			top.rotation = Quaternion.identity;
		}
	}

}
