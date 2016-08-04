using UnityEngine;
using System.Collections;

public class Blob_SimpleSphere : Blob
{
	#region private hooks

	private Material cachedMaterial_ = null;
	private MeshRenderer cachedRenderer_ = null;

	#endregion private hooks

	protected override void PostAwake( )
	{
		radius_ = cachedTransform.localScale.x;

		cachedRenderer_ = GetComponent<MeshRenderer>( );
		cachedMaterial_ = new Material( cachedRenderer_.sharedMaterial );
		cachedRenderer_.material = cachedMaterial_;
		cachedTransform.localScale = radius * Vector3.one;
	}

	protected override void SetAppearanceByType(BlobType t)
	{
		cachedMaterial_.color = t.colour;
	}

	override public void Init( Cannon cannon )
	{
		cachedTransform.position = cannon.cachedTransform.position;
		cachedTransform.rotation = cannon.cachedTransform.rotation;
    }

	override public void SetFlashState( float f )
	{
		Debug.LogWarning( "Not implemeneted" );
	}

	override public void SetCountdownState( float fraction01 )
	{
		Debug.LogWarning( "Not implemeneted" );
	}

}
