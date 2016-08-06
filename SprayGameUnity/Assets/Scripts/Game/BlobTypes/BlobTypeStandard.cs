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

	static private readonly string s_typeName_ = "STD";
	override public string typeName( )
	{
		return s_typeName_;
	}

	override public void SetConnectorAppearance( BlobConnector_SimpleCylinder connector )
	{
		connector.cachedMaterial.color = colour;
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
