using UnityEngine;
using System.Collections;

public class Blob_SimpleCylinder : Blob
{
	#region mesh

	static private Mesh sharedMesh_ = null;

	#endregion mesh

	#region private hooks

	public MeshRenderer cylinderRenderer;
	public MeshFilter cylinderMeshFilter;

	public Transform top;

	#endregion private hooks

	protected override void PostAwake( )
	{
		radius_ = cachedTransform.localScale.x;
		 
		cachedMaterial_ = new Material( cylinderRenderer.sharedMaterial );
		cylinderRenderer.sharedMaterial = cachedMaterial_;
		cachedTransform.localScale = new Vector3 ( radius, 0.5f, radius);

		if (sharedMesh_ == null)
		{
			sharedMesh_ = cylinderMeshFilter.sharedMesh;
			int nV = sharedMesh_.vertices.Length;
			int nUV = sharedMesh_.uv.Length;
			System.Text.StringBuilder sb = new System.Text.StringBuilder( );
			sb.Append( "Found mesh: nV=" + nV + ", nUV=" + nUV );
			if (nV == nUV)
			{
				for (int i = 0; i < nV; i++)
				{
					sb.Append( "\n " + sharedMesh_.vertices[i] + " " + sharedMesh_.uv[i] );
				}
				Vector2[] newUV = new Vector2[nV];
				for (int i = 0; i< nV; i++)
				{
					float u = 0f;
					if (Vector2.Distance(new Vector2( sharedMesh_.vertices[i].z, sharedMesh_.vertices[i].x ), Vector2.zero) < Mathf.Epsilon)
					{
						u = 1f;
					}
					float angle = Mathf.Atan2( sharedMesh_.vertices[i].z, sharedMesh_.vertices[i].x );
					while (angle < 0f) angle += 2f * Mathf.PI;
					float angleFraction = angle / (2f * Mathf.PI);
					newUV[i] = new Vector2( u, angleFraction );
				}
				sharedMesh_.uv = newUV;

			}
			Debug.LogError( sb.ToString( ) );

		}
		else
		{
			cylinderMeshFilter.sharedMesh = sharedMesh_;
		}
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
		cachedMaterial_.SetFloat( "_UPhase", 1f - fraction01 );
//		top.gameObject.SetActive( false );
		/*
		if (fraction01 == 0f)
		{
			top.gameObject.SetActive( false );
		}
		else
		{
			top.gameObject.SetActive( true );
			top.localScale = new Vector3( fraction01, fraction01, 1f );
			top.rotation = Quaternion.identity;
		}*/
	}

}
