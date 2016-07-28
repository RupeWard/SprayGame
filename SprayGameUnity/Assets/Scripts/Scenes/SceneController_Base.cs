using UnityEngine;
using System.Collections;

abstract public class SceneController_Base : MonoBehaviour 
{
	abstract public SceneManager.EScene Scene ();

	static private SceneController_Base current_;
	static public SceneController_Base Current
	{
		get { return current_; }
	}

	void Awake()
	{
		current_ = this;
		PostAwake( );

		if (!SqliteUtils.IsInitialised( ))
		{
			if (SqliteUtils.DEBUG_SQL)
			{
				Debug.Log( "No SqliteUtils in " + this.GetType( ).ToString( ) );
			}
			SqliteUtils.Instance.databaseLoadComplete += OnDatabasesLoaded;
			SqliteUtils.Instance.initialiseDatabases( "English" );
		}
		else
		{
			OnDatabasesLoaded( );
		}
	}

	void Start () 
	{
		if (SceneManager.DEBUG_SCENES)
		{
			Debug.Log ("Scene " + Scene() + " Start");
		}
		SceneManager.Instance.HandleSceneAwake (this);

		PostStart ();
		SceneManager.Instance.finishedSwitching ();
		Handheld.StopActivityIndicator ();
	}

	// Override in subclasses for set-up
	protected virtual void PostStart()
	{
	}

	// Override in subclasses for set-up
	protected virtual void PostAwake()
	{
	}

	// Override in subclasses for set-up
	protected virtual void OnDatabasesLoaded( )
	{
	}


}
