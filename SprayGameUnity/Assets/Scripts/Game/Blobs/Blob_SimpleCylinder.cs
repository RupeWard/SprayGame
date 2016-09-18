using UnityEngine;
using System.Collections;

public class Blob_SimpleCylinder : Blob
{
	#region mesh

	static private Mesh sharedMesh_ = null;

	#endregion mesh

	#region private hooks

	private MeshRenderer bottomDiscRenderer_;
	private MeshFilter bottomDiscMeshFilter_;
	public Transform bottomDiscTransform;

	private MeshRenderer topDiscRenderer_;
	private MeshFilter topDiscMeshFilter_;
	public Transform topDiscTransform;

	public Transform top;

	#endregion private hooks

	public static readonly float s_bottomDiscThickness = 0.1f;
	public static readonly float s_globalScale = 0.5f;

	private static readonly bool DEBUG_MESH = false;

	private Material cachedBottomDiscMaterial_ = null;
	public Material cachedBottomDiscMaterial
	{
		get { return cachedBottomDiscMaterial_; }
	}

    private Material cachedTopDiscMaterial_ = null;
	public Material cachedTopDiscMaterial
	{
		get { return cachedTopDiscMaterial_; }
	}

	public override void SetNewTypeFraction( BlobType_Base newType, float fraction )
	{
		BlobTypeStandard blobTypeS = blobType as BlobTypeStandard;
		BlobTypeStandard newTypeS = newType as BlobTypeStandard;
		if (blobTypeS == null || newType == null)
		{
			Debug.LogError( "What to do?" );
		}
		else
		{

			if (fraction <= 0f)
			{
				cachedBottomDiscMaterial.SetColor( "_Color", blobTypeS.colour );
				cachedTopDiscMaterial.SetColor( "_Color", blobTypeS.colour );
				cachedTopDiscMaterial.SetColor( "_Color2", Color.black );
				cachedTopDiscMaterial.SetFloat( "_UPhase", 0f );
			}
			else if (fraction >= 1f)
			{
				cachedBottomDiscMaterial.SetColor( "_Color", newTypeS.colour );
				cachedTopDiscMaterial.SetColor( "_Color", newTypeS.colour );
				cachedTopDiscMaterial.SetColor( "_Color2", Color.black );
				cachedTopDiscMaterial.SetFloat( "_UPhase", 0f );
			}
			else
			{
				cachedBottomDiscMaterial.SetColor( "_Color", blobTypeS.colour );
				cachedTopDiscMaterial.SetColor( "_Color", blobTypeS.colour );
				cachedTopDiscMaterial.SetColor( "_Color2", newTypeS.colour );
				cachedTopDiscMaterial.SetFloat( "_UPhase", Mathf.Clamp01( fraction ) );
			}


		}
	}


	protected override void PostAwake( )
	{
		radius_ = cachedTransform.localScale.x;

		bottomDiscMeshFilter_ = bottomDiscTransform.gameObject.GetComponent<MeshFilter>( );
		bottomDiscRenderer_ = bottomDiscTransform.gameObject.GetComponent<MeshRenderer>( );

		topDiscMeshFilter_ = topDiscTransform.gameObject.GetComponent<MeshFilter>( );
		topDiscRenderer_ = topDiscTransform.gameObject.GetComponent<MeshRenderer>( );

		cachedBottomDiscMaterial_ = new Material( bottomDiscRenderer_.sharedMaterial );
		bottomDiscRenderer_.sharedMaterial = cachedBottomDiscMaterial_;

		cachedTopDiscMaterial_ = new Material( topDiscRenderer_.sharedMaterial );
		topDiscRenderer_.sharedMaterial = cachedTopDiscMaterial_;

		cachedTopDiscMaterial_.SetFloat( "_UPhase", -1f );
		cachedBottomDiscMaterial_.SetFloat( "_UPhase", 0f );

		cachedTransform.localScale = new Vector3 ( radius, s_globalScale, radius);

		if (sharedMesh_ == null)
		{
			sharedMesh_ = bottomDiscMeshFilter_.sharedMesh;
			int nV = sharedMesh_.vertices.Length;
			int nUV = sharedMesh_.uv.Length;
			System.Text.StringBuilder sb = null;
			if (DEBUG_MESH)
			{
				sb = new System.Text.StringBuilder( );
				sb.Append( "Found mesh: nV=" + nV + ", nUV=" + nUV );
			}

			if (nV == nUV)
			{
				if (sb != null)
				{
					for (int i = 0; i < nV; i++)
					{
						sb.Append( "\n " + sharedMesh_.vertices[i] + " " + sharedMesh_.uv[i] );
					}
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
			if (sb!= null)
			{
				Debug.LogError( sb.ToString( ) );
			}

		}
		else
		{
			bottomDiscMeshFilter_.sharedMesh = sharedMesh_;
		}
		topDiscMeshFilter_.sharedMesh = sharedMesh_;
		top.gameObject.SetActive( false );
	}

	public override void SetWarningState( bool warning )
	{
		Color c = (warning) ? (Color.white) : (Color.black);
		cachedBottomDiscMaterial_.SetColor( "_Color2", c );
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

	public override void HandleDeath( )
	{
		base.HandleDeath( );
		cachedBottomDiscMaterial_.SetColor( "_Color2", Color.red);
	}

	override public void SetCountdownState( float fraction01 )
	{
		fraction01 = Mathf.Clamp01( fraction01 );
		cachedTopDiscMaterial_.SetFloat( "_UPhase", fraction01 );
		cachedBottomDiscMaterial_.SetColor( "_Color", Color.black);
	}

}
