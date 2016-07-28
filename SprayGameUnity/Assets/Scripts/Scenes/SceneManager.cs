using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SceneManager : RJWard.Core.Singleton.SingletonApplicationLifetimeLazy< SceneManager > 
{
	public static readonly bool DEBUG_SCENES = false;

	private bool isSwitching_ = false;
	public void finishedSwitching()
	{
		isSwitching_ = false;
	}

#region scene types

	public enum EScene
	{
		NONE,
		Bootstrap,
		DevSetup,
		Game
	};

	private Dictionary< EScene, string > sceneNames_ = new Dictionary<EScene, string> ()
	{
		{ EScene.Bootstrap, "BootstrapScene" },
		{ EScene.DevSetup, "DevSetupScene" },
		{ EScene.Game, "GameScene" },
	};

#endregion scene types

#region private data

	private EScene currentScene_ = EScene.NONE;
	private EScene previousScene_ = EScene.NONE;

#endregion private data

#region accessors

	public EScene CurrentScene
	{
		get { return currentScene_; }
	}

	public EScene PreviousScene
	{
		get { return previousScene_; }
	}

#endregion accessors

#region Flow

	public void HandleSceneAwake( SceneController_Base controller )
	{
		if (currentScene_ != controller.Scene())
		{
			previousScene_ = currentScene_;
		}
		currentScene_ = controller.Scene();
#if UNITY_EDITOR
		if (DEBUG_SCENES)
		{
			Debug.Log ("SceneManager: HandleSceneAwake( " + currentScene_ + " ), previously " + previousScene_);
		}
#endif
    }

	public void SwitchScene(EScene newScene)
	{
		SwitchSceneHelper (newScene, false);
	}

	public void ReloadScene(EScene newScene)
	{
		SwitchSceneHelper (newScene, true);
	}

	private void SwitchSceneHelper( EScene newScene, bool allowSameScene)
	{
		if (!allowSameScene && newScene == currentScene_)
		{
			Debug.LogWarning ("Request to switch to same scene " + newScene);
		} 
		else
		{
			if (isSwitching_)
			{
				Debug.LogWarning("SwitchSceneHelper rejecting request to switch from "+currentScene_+" to "+newScene+" because it's already switching");
			}
			else
			{
				if (DEBUG_SCENES)
				{
					Debug.Log ("SwitchSceneHelper: from "+currentScene_+" to "+newScene);
				}
				isSwitching_ = true;

				#if UNITY_IPHONE
				Handheld.SetActivityIndicatorStyle( iOSActivityIndicatorStyle.WhiteLarge );
				#elif UNITY_ANDROID
				Handheld.SetActivityIndicatorStyle( AndroidActivityIndicatorStyle.Large );
				#endif
				
				Handheld.StartActivityIndicator();

				StartCoroutine(SwitchSceneCR(sceneNames_[ newScene ] ));
			}
		}
	}

	private IEnumerator SwitchSceneCR(string s)
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene( s );
		yield return null;
	}

	public void SwitchToPreviousScene()
	{
		if (previousScene_ != EScene.NONE)
		{
			if (DEBUG_SCENES)
			{
				Debug.Log ("Switching to previous scene " + previousScene_);
			}
			SwitchScene (previousScene_);
		} 
		else
		{
			Debug.Log ("Can't switch to previous scene because there is none ");
		}
	}

#endregion Flow



}
