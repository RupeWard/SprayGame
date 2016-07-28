/**
 * Below are several singleton implementations that all have different purposes. 
 * Each singleton will stop fetching of the singleton instance if it has been detected that we're shutting down.
 * This will stop singletons from being recreated when it isn't not required.
 * There are scene singletons which have the lifetime of a scene, when you change scene they will be destroyed
 * Application singletons are singletons that will remain for the entire application life. They will be destroyed on application quit.
 * 
 * Any second instance of the singleton game object will be destroyed immediately (with a warning message).
 * 
 * Instead of using Awake, OnDestroy and OnApplicationQuit in derived classes use PostAwake, PostOnDestroy and PostOnApplicationQuit. 
 * These are NOT called if the game object being instantiated is a second instance of the singleton
 * If you override Awake, OnDestroy or OnApplicationQuit in derived classes then you will get a compiler warning in Unity
 * 
 * SingletonSceneLifetime: A singleton that lasts for an entire scene, the GameObject NEEDS TO BE PRESENT in the scene for the singleton instance to be cached
 *                          We block the instance being fetched until one game object in the scene of the required type has been awaken, after which we cache the instance.

 * SingletonApplicationLifetime:  A singleton that lasts for an entire application, the GameObject NEEDS TO BE PRESENT in the FIRST scene for the singleton instance to be cached
 *                          We block the instance from being fetched until one game object in the scene of the required type has been awaken, after which we cache the instance.
 *                          
 * SingletonApplicationLifetimeLazy: A singleton that lasts for an entire application, the GameObject will be created and added to the scene (and never destroyed until application quit)
 *                          We create the game object and add it to the scene
 * 
 * Singleton: Regular old singleton, not associated with any Unity object
 * 
 */

//#define DEBUG_SINGLETONS

using System;
using UnityEngine;

namespace RJWard.Core.Singleton
{

#region Scene singletons

	static class SingletonCore
	{
#if DEBUG_SINGLETON
	static public bool DebugSingleton = true; 
#else
		static public bool DebugSingleton = false;
#endif

	};

	/// <summary>
	/// Expects a game object to be present in the scene, fetches it, caches it and disposes of it correctly
	/// </summary>
	/// <typeparam name="GameObjectType"></typeparam>
	public class SingletonSceneLifetime<GameObjectType> : MonoBehaviour
		where GameObjectType : MonoBehaviour
	{
		public static bool IsInitialised( )
		{
			return instance != null;
		}

		// Don't override in derived classes, override PostAwake instead
		void Awake( )
		{
			if (instance == null)
			{
				blockInstanceFetch = false;
			}

			// The call to Instance will cache the instance static var
			if (!Instance.Equals( this ))
			{
				Debug.LogWarning( "Attempting to instantiate a second instance of scene singleton of type " + typeof( GameObjectType ).ToString( ) + "! Destroying it" );
				UnityHelpers.Destroy( gameObject );
			}
			else
			{
#if DEBUG_SINGLETONS
				Debug.Log( "Awaking scene singleton of type " + typeof( GameObjectType ).ToString( ) );
#endif //DEBUG_SINGLETONS
				PostAwake( );
			}
		}

		// Don't override in derived classes, override PostOnDestroy instead
		void OnDestroy( )
		{
			// Only clear the instance if this object is the instance
			if (instance != null)
			{
				if (instance.Equals( this ))
				{
#if DEBUG_SINGLETONS
					Debug.Log( "Destroying scene singleton of type " + typeof( GameObjectType ).ToString( ) );
#endif //DEBUG_SINGLETONS
					SingletonHelper<GameObjectType>.HandleOnDestroy( ref instance, ref blockInstanceFetch );
					PostOnDestroy( );
				}
			}
			else
			{
				Debug.LogWarning( "Destroying an object when the instance of the singleton is null, this should not happen (unless you change scripts at runtime in the editor)!\n Type is " + typeof( GameObjectType ).ToString( ) );
			}
		}

		// Don't override in derived classes, override PostOnApplicationQuit instead
		void OnApplicationQuit( )
		{
			SingletonHelper<GameObjectType>.HandleOnApplicationQuit( ref instance, ref blockInstanceFetch );
			PostOnApplicationQuit( );
		}

		// Overriden in derived classes
		protected virtual void PostAwake( )
		{

		}

		// Overriden in derived classes
		protected virtual void PostOnDestroy( )
		{

		}

		// Overriden in derived classes
		protected virtual void PostOnApplicationQuit( )
		{

		}

		/// <summary>
		/// Careful with this, the instance will be recreated on next .Instance call. Is this required behaviour???
		/// </summary>
		protected static void ClearSingletonInstance( )
		{
			instance = null;
		}

		/// <summary>
		/// Singleton
		/// </summary>
		private static GameObjectType instance = null;
		// If an instance of the game object has been created then we don't want to let the game object to be recreated again in this scene
		// If changing scene it is possible that the instance could be set to null and something else could call .Instance. This could potentially recreate the game object even 
		// though it is supposed to be destroyed. This variable stops the instance from being recreated when shutting down/destroying
		// Default value here is VERY important, block the fetching until we have a game object awaken in the scene, then this boolean is set to false
		public static bool blockInstanceFetch = true;

		public static GameObjectType Instance
		{
			get
			{

				if (SingletonHelper<GameObjectType>.ShouldAssignInstance( ref instance, ref blockInstanceFetch ))
				{
					string instanceName = "The" + typeof( GameObjectType ).ToString( );
					GameObject gameObj = GameObject.Find( instanceName ) as GameObject;

					if (gameObj != null)
					{
						instance = gameObj.GetComponent<GameObjectType>( );
#if DEBUG_SINGLETONS
						Debug.Log( "Found and assigned scene singleton instance of type " + typeof( GameObjectType ).ToString( ) );
#endif //DEBUG_SINGLETONS
					}
					else
					{
#if DEBUG_SINGLETONS
						Debug.Log( "Unable to find game object named " + instanceName + " of type " + typeof( GameObjectType ).ToString( ) + " in current scene" );
#endif
						GameObjectType got = FindObjectOfType( typeof( GameObjectType ) ) as GameObjectType;
						if (got != null)
						{
							instance = got;
							got.gameObject.name = instanceName;
#if DEBUG_SINGLETONS
							Debug.Log( "Found, assigned, and renamed lifetime instance of type " + typeof( GameObjectType ).ToString( ) );
#endif //DEBUG_SINGLETONS
						}
						else
						{
							Debug.LogError( "Unable to find game object of type " + typeof( GameObjectType ).ToString( ) + " in current scene" );
						}
					}

				}

				return instance;
			}
		}
	}

#endregion Scene singletons

#region Application Singletons

	/// <summary>
	/// Expects a game object to present in the scene and deletes any further instances of the game object created
	/// Major difference with this singleton is that it has a DontDestroyOnLoad in the Instance property
	/// </summary>
	/// <typeparam name="GameObjectType"></typeparam>
	public class SingletonApplicationLifetime<GameObjectType> : MonoBehaviour
		where GameObjectType : MonoBehaviour
	{
		public static bool IsInitialised( )
		{
			return instance != null;
		}

		// Don't override in derived classes, override PostAwake instead
		void Awake( )
		{
			if (instance == null)
			{
				blockInstanceFetch = false;
			}

			// The call to Instance will cache the instance static var
			if (!Instance.Equals( this ))
			{
				Debug.LogWarning( "Attempting to instantiate a second instance of application singleton of type " + typeof( GameObjectType ).ToString( ) + "! Destroying it" );
				UnityHelpers.Destroy( gameObject );
			}
			else
			{
#if DEBUG_SINGLETONS
				Debug.Log( "Awaking an application singleton of type " + typeof( GameObjectType ).ToString( ) );
#endif //DEBUG_SINGLETONS
				PostAwake( );
			}
		}

		// Don't override in derived classes, override PostOnDestroy instead
		void OnDestroy( )
		{
			// Only clear the instance if this object is the instance
			if (instance != null)
			{
				if (instance.Equals( this ))
				{
					SingletonHelper<GameObjectType>.HandleOnDestroy( ref instance, ref blockInstanceFetch );
					PostOnDestroy( );
				}
			}
			else
			{
				Debug.LogWarning( "Destroying an object when the instance of the singleton is null, this should not happen (unless you change scripts at runtime in the editor)!\n Type is " + typeof( GameObjectType ).ToString( ) );
			}
		}

		// Don't override in derived classes, override PostOnApplicationQuit instead
		void OnApplicationQuit( )
		{
			SingletonHelper<GameObjectType>.HandleOnApplicationQuit( ref instance, ref blockInstanceFetch );
			PostOnApplicationQuit( );
		}

		// Overriden in derived classes
		protected virtual void PostAwake( )
		{

		}

		// Overriden in derived classes
		protected virtual void PostOnDestroy( )
		{

		}

		// Overriden in derived classes
		protected virtual void PostOnApplicationQuit( )
		{

		}

		/// <summary>
		/// Careful with this, the instance will be recreated on next .Instance call. Is this required behaviour???
		/// </summary>
		protected static void ClearSingletonInstance( )
		{
			instance = null;
		}

		/// <summary>
		/// Singleton
		/// </summary>
		private static GameObjectType instance = null;
		// If an instance of the game object has been created then we don't want to let the game object to be recreated again in this scene
		// If changing scene it is possible that the instance could be set to null and something else could call .Instance. This could potentially recreate the game object even 
		// though it is supposed to be destroyed. This variable stops the instance from being recreated when shutting down/destroying
		public static bool blockInstanceFetch = true;

		public static GameObjectType Instance
		{
			get
			{
				if (SingletonHelper<GameObjectType>.ShouldAssignInstance( ref instance, ref blockInstanceFetch ))
				{
					string instanceName = "The" + typeof( GameObjectType ).ToString( );

					GameObjectType got = FindObjectOfType( typeof( GameObjectType ) ) as GameObjectType;
					if (got != null)
					{
						instance = got;
						got.gameObject.name = instanceName;
						DontDestroyOnLoad( got.gameObject );
#if DEBUG_SINGLETONS
						Debug.Log( "Found and assigned lifetime instance of type " + typeof( GameObjectType ).ToString( ) );
#endif //DEBUG_SINGLETONS
					}
					else
					{
						Debug.Log( "Couldn't find instance of type " + typeof( GameObjectType ).ToString( ) + " so attempting to create" );
						GameObject go = new GameObject( );
						instance = go.AddComponent<GameObjectType>( );
						go.name = instanceName;
						DontDestroyOnLoad( go );
#if DEBUG_SINGLETONS
						Debug.Log( "Created lifetime instance of type " + typeof( GameObjectType ).ToString( ) );
#endif //DEBUG_SINGLETONS
					}

					/*
					GameObject gameObj = GameObject.Find(instanceName) as GameObject;

					if (gameObj != null)
					{
						instance = gameObj.GetComponent<GameObjectType>();
	#if DEBUG_SINGLETONS
						Debug.LogError("Found and assigned lifetime instance of type " + typeof(GameObjectType).ToString());
	#endif //DEBUG_SINGLETONS
						DontDestroyOnLoad(gameObj);
					}
					else
					{
						Debug.LogError("Unable to find game object called '"+instanceName+"' of type " + typeof(GameObjectType).ToString() + " in current scene");
					}*/
				}

				return instance;
			}
		}
	}

	/// <summary>
	/// Major difference with this singleton between SingletonApplicationLifetime is that the former expects the game object to be present in the scene
	/// This singleton creates and adds the game object to the scene/// </summary>
	/// <typeparam name="GameObjectType"></typeparam>
	public class SingletonApplicationLifetimeLazy<GameObjectType> : MonoBehaviour
		where GameObjectType : MonoBehaviour
	{
		public static void CreateInstance( )
		{
			if (!IsInitialised( ))
			{
				GameObjectType theOne = Instance;
				if (SingletonCore.DebugSingleton)
				{
					Debug.Log( "Created instance of " + typeof( GameObjectType ) + "=" + theOne.gameObject.name );
				}
			}
		}

		public static bool IsInitialised( )
		{
			return instance != null;
		}

		// Don't override in derived classes, override PostAwake instead
		void Awake( )
		{
			if (instance != null)
			{
				// The call to Instance will cache the instance static var
				if (!instance.Equals( this ))
				{
					Debug.LogWarning( "Attempting to instantiate a second instance of application singleton of type " + typeof( GameObjectType ).ToString( ) + "! Destroying it" );
					UnityHelpers.Destroy( gameObject );
				}
			}
			else
			{
#if DEBUG_SINGLETONS
				Debug.Log( "Awaking application lazy singleton of type " + typeof( GameObjectType ).ToString( ) );
#endif //DEBUG_SINGLETONS
				PostAwake( );
			}
		}

		// Don't override in derived classes, override PostOnDestroy instead
		void OnDestroy( )
		{
			// Only clear the instance if this object is the instance
			if (instance != null)
			{
				if (instance.Equals( this ))
				{
#if DEBUG_SINGLETONS
					Debug.Log( "Destroying a SingletonApplicationLifetimeLazy. Type is " + typeof( GameObjectType ).ToString( ) );
#endif //DEBUG_SINGLETONS
					SingletonHelper<GameObjectType>.HandleOnDestroy( ref instance, ref blockInstanceFetch );
					PostOnDestroy( );
				}
			}
			else
			{
				Debug.LogWarning( "Destroying a SingletonApplicationLifetimeLazy when the instance of the singleton is null, this should not happen (unless you change scripts at runtime in the editor)!\n Type is " + typeof( GameObjectType ).ToString( ) );
				PostOnDestroy( );
			}
		}

		// Don't override in derived classes, override PostOnApplicationQuit instead
		void OnApplicationQuit( )
		{
			SingletonHelper<GameObjectType>.HandleOnApplicationQuit( ref instance, ref blockInstanceFetch );
			PostOnApplicationQuit( );
		}

		// Overriden in derived classes
		protected virtual void PostAwake( )
		{

		}

		// Overriden in derived classes
		protected virtual void PostOnDestroy( )
		{

		}

		// Overriden in derived classes
		protected virtual void PostOnApplicationQuit( )
		{

		}

		/// <summary>
		/// Singleton
		/// </summary>
		private static GameObjectType instance = null;
		// If an instance of the game object has been created then we don't want to let the game object to be recreated again in this scene
		// If changing scene it is possible that the instance could be set to null and something else could call .Instance. This could potentially recreate the game object even 
		// though it is supposed to be destroyed. This variable stops the instance from being recreated when shutting down/destroying
		// Default value here is VERY important, set to false so that the first fetch will allow the creation of the instance
		public static bool blockInstanceFetch = false;

		public static GameObjectType Instance
		{
			get
			{
				if (SingletonHelper<GameObjectType>.ShouldAssignInstance( ref instance, ref blockInstanceFetch ))
				{
					GameObject foundObj = GameObject.Find( typeof( GameObjectType ).ToString( ) );
					if (foundObj != null)
					{
#if DEBUG_SINGLETONS
						Debug.Log( "Found an existing game object of type " + typeof( GameObjectType ).ToString( ) + " in the scene, this singleton autocreates the game object and adds to the scene. There will now be 2 of the same game object in the scene!" );
#endif
					}

					GameObject gameObj = new GameObject( typeof( GameObjectType ).ToString( ) );

					if (gameObj != null)
					{
						instance = gameObj.AddComponent<GameObjectType>( );
#if DEBUG_SINGLETONS
						Debug.Log( "Created a singleton with an application lifetime of type " + typeof( GameObjectType ).ToString( ) );
#endif //DEBUG_SINGLETONS
						DontDestroyOnLoad( gameObj );
					}
					else
					{
						Debug.LogError( "Unable to create application singleton of type " + typeof( GameObjectType ).ToString( ) );
					}
				}

				return blockInstanceFetch ? null : instance;
			}
		}
	}

#endregion Application Singletons

#region Regular singleton

	public class Singleton<ObjectType> where ObjectType : class, new()
	{

		private static bool blockInstantiation = false;
		protected static void ClearSingletonInstance( )
		{
			instance = null;
			blockInstantiation = true;
		}

		public static bool IsInitialised( )
		{
			return instance != null;
		}

		// Used in derived classes instead of a private constructor
		protected virtual void Init( )
		{

		}

		public static void AllowInstantiation( )
		{
			blockInstantiation = false;
		}

		protected Singleton( )
		{
#if DEBUG_SINGLETONS
			Debug.Log( "Created singleton of type " + GetType( ).ToString( ) );
#endif //DEBUG_SINGLETONS
			Init( );
		}

		~Singleton( )
		{
#if DEBUG_SINGLETONS
			Debug.Log( "Destroyed singleton of type " + GetType( ).ToString( ) );
#endif //DEBUG_SINGLETONS
		}

		/// <summary>
		/// Singleton
		/// </summary>
		private static ObjectType instance = null;

		public static ObjectType Instance
		{
			get
			{
				if (blockInstantiation)
				{
					instance = null;
					return null;
				}
				else if (instance == null)
				{
					instance = new ObjectType( );
				}

				return instance;
			}
		}
	}

#endregion Regular singleton

#region Helpers

	/// <summary>
	/// Shared methods across all singletons (ie. invalidating instances, verifying there is only 1, deleting duplicates)
	/// </summary>
	/// <typeparam name="GameObjectType"></typeparam>
	internal class SingletonHelper<GameObjectType>
		where GameObjectType : MonoBehaviour
	{
		public static void HandleOnDestroy( ref GameObjectType instance, ref bool shouldBlockFetch )
		{
			instance = null;
			shouldBlockFetch = true;
		}

		public static void HandleOnApplicationQuit( ref GameObjectType instance, ref bool shouldBlockFetch )
		{
			//instance = null;
			shouldBlockFetch = true;
		}

		public static bool ShouldAssignInstance( ref GameObjectType instance, ref bool shouldBlockFetch )
		{
			bool shouldReassign = false;
			if (shouldBlockFetch)
			{
				shouldBlockFetch = true;
				shouldReassign = false;
			}
			else if (instance == null && !shouldBlockFetch)
			{
				shouldReassign = true;
			}

			return shouldReassign;
		}
	}

#endregion Helpers

}

