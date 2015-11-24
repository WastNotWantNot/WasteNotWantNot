/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"NavigationManager.cs"
 * 
 *	This script instantiates the chosen
 *	NavigationEngine subclass at runtime.
 * 
 */

using UnityEngine;

namespace AC
{

	/**
	 * This component instantiates the scene's chosen NavigationEngine ScriptableObject when the game begins.
	 * It should be placed on the GameEngine prefab.
	 */
	public class NavigationManager : MonoBehaviour
	{

		/** The NavigationEngine ScriptableObject that performs the scene's pathfinding algorithms. */
		[HideInInspector] public NavigationEngine navigationEngine = null;
		
		
		private void Awake ()
		{
			navigationEngine = null;
			ResetEngine ();
		}


		/**
		 * Sets up the scene's chosen NavigationEngine ScriptableObject if it is not already present.
		 */
		public void ResetEngine ()
		{
			if (GetComponent <SceneSettings>())
			{
				string className = "NavigationEngine_" + GetComponent <SceneSettings>().navigationMethod.ToString ();

				if (navigationEngine == null || !navigationEngine.ToString ().Contains (className))
				{
					navigationEngine = (NavigationEngine) ScriptableObject.CreateInstance (className);
					navigationEngine.Awake ();
				}
			}
		}

	}

}