using UnityEngine;
using System.Collections;

[System.Serializable]
public class BlobTypeStandard: BlobType_Base
{
	public Color colour;

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

	override public void DebugDescribe( System.Text.StringBuilder sb )
	{
		sb.Append( "[BT_STD: " + colour +"]");
	}

}
