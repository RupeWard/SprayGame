using UnityEngine;
using System.Collections;

[System.Serializable]
abstract public class BlobType_Base :RJWard.Core.IDebugDescribable
{
	abstract public void SetConnectorAppearance( BlobConnector_SimpleCylinder connector );
	abstract public void SetBlobAppearance( Blob_SimpleCylinder blob);
	abstract public void SetCannonAppearance( Cannon c );

	abstract public void DebugDescribe( System.Text.StringBuilder sb);
}
