/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"NavigationEngine.cs"
 * 
 *	This script is a base class for the Navigation method scripts.
 *  Create a subclass of name "NavigationEngine_NewMethodName" and
 * 	add "NewMethodName" to the AC_NavigationMethod enum to integrate
 * 	a new method into the engine.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	/**
	 * A base class for all navigation methods.  Subclasses of this script are used to return a navigation path, as an array of Vector3s, based on two positions.
	 * A number of functions to allow easier integration within SceneManager are also included.
	 * To create a new navigation method, create a new subclass of this script with the name syntax "NavigationEngine_NewMethodName", and add "NewMethodName" to the AC_NavigationMethod enum in Enums.cs.
	 * The method will then be an option in the "Navigation engine" popup in the Scene Manager.
	 */
	public class NavigationEngine : ScriptableObject
	{

		/**
		 * Called when the scene begins or is reset.
		 */
		public virtual void Awake ()
		{ }


		/**
		 * <summary>Calculates a path between two points.</summary>
		 * <param name = "startPosition">The start position</param>
		 * <param name = "targetPosition">The indended end position</param>
		 * <param name = "_char">The character (see Char) who this path is for (only used in PolygonCollider pathfinding)</param>
		 * <returns>The path to take, as an array of Vector3s.</returns>
		 */
		public virtual Vector3[] GetPointsArray (Vector3 startPosition, Vector3 targetPosition, AC.Char _char = null)
		{
			List <Vector3> pointsList = new List<Vector3>();
			pointsList.Add (targetPosition);
			return pointsList.ToArray ();
		}


		/**
		 * <summary>Gets the name of a "helper" prefab to list in the Scene Manager.</summary>
		 * <returns>The name of the prefab to list in SceneManager. The prefab must be placed in the Assets/AdventureCreator/Prefabs/Navigation folder. If nothing is returned, no prefab will be listed.</returns>
		 */
		public virtual string GetPrefabName ()
		{
			return "";
		}


		/**
		 * <summary>Sets the visibility state of any relevant prefabs.
		 * This is called when the "NavMesh" visibility buttons in SceneManager are clicked on.</summary>
		 * <param name = "visibility">True if the prefabs should be made visible. Otherwise, they should be made invisible.</param>
		 */
		public virtual void SetVisibility (bool visibility)
		{ }


		/**
		 * <summary>Adds holes in a PolygonCollider2D to account for stationary characters, so they can be evaded during pathfinding calculations (Polygon Collider-based navigation only).
		 * <param name = "navPoly">The original PolygonCollider2D to modify</param>
		 * <param name = "charToExclude">The character to ignore when creating holes. Typically this is the Player character, or any character already moving.</param>
		 * <returns>True if changes were made to the base PolygonCollider2D.</returns>
		 */
		public virtual bool AddCharHoles (PolygonCollider2D navPoly, Char charToExclude)
		{
			return false;
		}


		/**
		 * Provides a space for any custom Editor GUI code that should be displayed in SceneManager.
		 */
		public virtual void SceneSettingsGUI ()
		{ 
			#if UNITY_EDITOR
			#endif
		}

	}

}