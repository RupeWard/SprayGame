using UnityEngine;
using System.Collections;

public class Blob_SimpleCylinder : Blob
{
	#region private hooks

	private Material cachedMaterial_ = null;
	public MeshRenderer myRenderer = null;

	#endregion private hooks

	protected override void PostAwake( )
	{
		radius_ = cachedTransform.localScale.x;
		 
		cachedMaterial_ = new Material( myRenderer.sharedMaterial );
		myRenderer.sharedMaterial = cachedMaterial_;
		cachedTransform.localScale = new Vector3 ( radius, 0.5f, radius);
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
        //cachedMaterial_.SetColor( "_EmissionColor", Color.Lerp( Color.black, blobType.colour, f ) );
		cachedMaterial_.color = Color.Lerp( blobType.colour, Color.black, f ) ;
	}

}
