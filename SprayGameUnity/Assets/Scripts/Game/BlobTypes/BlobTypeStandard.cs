using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class BlobTypeStandard: BlobType_Base
{
	public Color colour;
	public string prefabName;

	private static readonly string defaultPrefabName_ = "Blob_Standard";

	override public GameObject Instantiate()
	{
		GameObject prefab = GetPrefab( prefabName );
		GameObject result = GameObject.Instantiate( prefab );
		result.GetComponent<Blob>( ).SetType( this );
		return result; 
	}

	private static Dictionary<string, GameObject> s_prefabDB_ = new Dictionary<string, GameObject>( );
	private static GameObject GetPrefab( string n)
	{
		GameObject result = null;
		if (n.Length == 0)
		{
			n = defaultPrefabName_;
		}
		if (false == s_prefabDB_.TryGetValue(n, out result))
		{
			result = Resources.Load<GameObject>( "Prefabs/Blobs/" + n );
			s_prefabDB_.Add( n, result );
		}
		return result;
	}


	static public readonly string s_typeName_ = "STD";
	override public string typeName( )
	{
		return s_typeName_;
	}

	override public bool ShouldDeleteGroupOfNum( int n )
	{
		if (name == "FIXED")
			return false; // TODO more sphistication!

		return (n >= GameManager.Instance.levelSettings.numBlobs);
    }

	override public void SetConnectorAppearance( BlobConnector_SimpleCylinder connector )
	{
		connector.cachedMaterial.SetColor("_Color1",colour);
		connector.cachedMaterial.SetColor( "_Color2", colour );
		connector.cachedMaterial.SetFloat( "_Alpha", 1f );
	}

	override public void SetBlobAppearance( Blob_SimpleCylinder blob )
	{
		blob.cachedMaterial.color = colour;
	}

	override public void SetCannonAppearance( Cannon c)
	{
		c.SetColour( colour );
    }

	override protected void DebugDescribeSub( System.Text.StringBuilder sb )
	{
		sb.Append( colour);
		sb.Append( ", prefab=" );
		if (prefabName.Length > 0)
		{
			sb.Append( prefabName );
		}
		else
		{
			sb.Append( "default" );
		}
	}

	override protected string GetSubDefnString( )
	{
		return prefabName + ":" + colour.ToString( );
	} 

}
