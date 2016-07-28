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
		cachedRenderer_ = GetComponent<MeshRenderer>( );
		cachedMaterial_ = new Material( cachedRenderer_.sharedMaterial );
		cachedRenderer_.material = cachedMaterial_;
	}

	protected override void SetAppearanceByType(BlobType t)
	{
		cachedMaterial_.color = t.colour;
	}
}
