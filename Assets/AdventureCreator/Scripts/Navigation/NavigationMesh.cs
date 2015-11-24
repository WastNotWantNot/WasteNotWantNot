/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"NavigationMesh.cs"
 * 
 *	This script is used by the MeshCollider and PolygonCollider
 *  navigation methods to define the pathfinding area.
 * 
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	/**
	 * Defines a walkable area of AC's built-in pathfinding algorithms.
	 */
	public class NavigationMesh : NavMeshBase
	{

		/** A List of holes within the base PolygonCollider2D */
		public List<PolygonCollider2D> polygonColliderHoles = new List<PolygonCollider2D>();
		/** If True, the boundary will be drawn in the Scene window (Polygon Collider-based navigation only) */
		public bool showInEditor = true;
		/** If True, then holes will be generated around stationary 2D characters so that moving characters cannot walk through them (Polygon Collider-based navigation only) */
		public bool moveAroundChars = true;

		private Vector2[] vertexData;


		private void Awake ()
		{
			BaseAwake ();
			ResetHoles ();
		}


		/**
		 * <summary>Integrates a PolygonCollider2D into the shape of the base PolygonCollider2D.
		 * If the shape of the new PolygonCollider2D is within the boundary of the base PolygonCollider2D, then the shape will effectively be subtracted.
		 * If the shape is instead outside the boundary of the base and overlaps, then the two shapes will effectively be combined.</summary>
		 * <param name = "newHole">The new PolygonCollider2D to integrate</param>
		 */
		public void AddHole (PolygonCollider2D newHole)
		{
			if (polygonColliderHoles.Contains (newHole))
			{
				return;
			}

			polygonColliderHoles.Add (newHole);
			ResetHoles ();

			if (GetComponent <RememberNavMesh2D>() == null)
			{
				ACDebug.LogWarning ("Changes to " + this.gameObject.name + "'s holes will not be saved because it has no RememberNavMesh2D script");
			}
		}


		/**
		 * <summary>Removes the effects of a PolygonCollider2D on the shape of the base PolygonCollider2D.
		 * This function will only have an effect if the PolygonCollider2D was previously added, using AddHole().</sumary>
		 * <param name = "oldHole">The new PolygonCollider2D to remove</param>
		 */
		public void RemoveHole (PolygonCollider2D oldHole)
		{
			if (polygonColliderHoles.Contains (oldHole))
			{
				polygonColliderHoles.Remove (oldHole);
				ResetHoles ();
			}
		}


		/**
		 * <summary>Adds holes in the base PolygonCollider2D to account for stationary characters, so they can be evaded during pathfinding calculations (Polygon Collider-based navigation only).
		 * This function will only have an effect if moveAroundChars is True.</summary>
		 * <param name = "charToExclude">The character to ignore when creating holes. Typically this is the Player character, or any character already moving.</param>
		 * <returns>True if changes were made to the base PolygonCollider2D.</returns>
		 */
		public bool AddCharHoles (Char charToExclude)
		{
			if (!moveAroundChars)
			{
				return false;
			}

			if (GetComponent <PolygonCollider2D>())
			{
				PolygonCollider2D poly = GetComponent <PolygonCollider2D>();
				if (KickStarter.navigationManager.navigationEngine.AddCharHoles (poly, charToExclude))
				{
					RebuildVertexArray (poly);
					return true;
				}
			}

			return false;
		}


		/**
		 * Integrates all PolygonCollider2D objects in the polygonColliderHoles List into the base PolygonCollider2D shape.
		 * This is called automatically by AddHole() and RemoveHole() once the List has been amended
		 */
		public void ResetHoles ()
		{
			if (GetComponent <PolygonCollider2D>())
			{
				PolygonCollider2D poly = GetComponent <PolygonCollider2D>();
				poly.pathCount = 1;
			
				if (polygonColliderHoles.Count == 0)
				{
					RebuildVertexArray (poly);
					return;
				}

				foreach (PolygonCollider2D hole in polygonColliderHoles)
				{
					if (hole != null)
					{
						poly.pathCount ++;
						
						List<Vector2> newPoints = new List<Vector2>();
						foreach (Vector2 holePoint in hole.points)
						{
							newPoints.Add (hole.transform.TransformPoint (holePoint) - transform.position);
						}
						
						poly.SetPath (poly.pathCount-1, newPoints.ToArray ());
						hole.gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.deactivatedLayer);
						hole.isTrigger = true;
					}
				}

				RebuildVertexArray (poly);
			}
			else if (GetComponent <MeshCollider>())
			{
				if (GetComponent <MeshCollider>().sharedMesh == null)
				{
					if (GetComponent <MeshFilter>() && GetComponent <MeshFilter>().sharedMesh)
					{
						GetComponent <MeshCollider>().sharedMesh = GetComponent <MeshFilter>().sharedMesh;
						ACDebug.LogWarning (this.gameObject.name + " has no MeshCollider mesh - temporarily using MeshFilter mesh instead.");
					}
					else
					{
						ACDebug.LogWarning (this.gameObject.name + " has no MeshCollider mesh.");
					}
				}
			}
		}
		

		/**
		 * Enables the GameObject so that it can be used in pathfinding.
		 */
		public void TurnOn ()
		{
			if (KickStarter.sceneSettings.navigationMethod == AC_NavigationMethod.meshCollider || KickStarter.sceneSettings.navigationMethod == AC_NavigationMethod.PolygonCollider)
			{
				if (LayerMask.NameToLayer (KickStarter.settingsManager.navMeshLayer) == -1)
				{
					ACDebug.LogError ("Can't find layer " + KickStarter.settingsManager.navMeshLayer + " - please define it in Unity's Tags Manager (Edit -> Project settings -> Tags and Layers).");
				}
				else if (KickStarter.settingsManager.navMeshLayer != "")
				{
					gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.navMeshLayer);
				}
				
				if (KickStarter.sceneSettings.navigationMethod == AC_NavigationMethod.meshCollider && GetComponent <Collider>() == null)
				{
					ACDebug.LogWarning ("A Collider component must be attached to " + this.name + " for pathfinding to work - please attach one.");
				}
				else if (KickStarter.sceneSettings.navigationMethod == AC_NavigationMethod.PolygonCollider && GetComponent <Collider2D>() == null)
				{
					ACDebug.LogWarning ("A 2D Collider component must be attached to " + this.name + " for pathfinding to work - please attach one.");
				}
			}
			else
			{
				ACDebug.LogWarning ("Cannot enable NavMesh " + this.name + " as this scene's Navigation Method is Unity Navigation.");
			}
		}
		

		/**
		 * Disables the GameObject from being used in pathfinding.
		 */
		public void TurnOff ()
		{
			gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.deactivatedLayer);
		}


		protected void OnDrawGizmos ()
		{
			if (showInEditor)
			{
				DrawGizmos ();
			}
		}
		
		
		protected void OnDrawGizmosSelected ()
		{
			DrawGizmos ();
		}


		/**
		 * <summary>Gets the an array of the positions of vertices, in all holes, in the attached PolygonCollider2D.</summary>
		 * <returns>Gets the an array of the positions of vertices, in all holes, in the attached PolygonCollider2D.</returns>
		 */
		public Vector2[] GetVertexArray ()
		{
			return vertexData;
		}


		private void RebuildVertexArray (PolygonCollider2D poly)
		{
			List<Vector2> _vertexData = new List<Vector2>();

			for (int i=0; i<poly.pathCount; i++)
			{
				Vector2[] _vertices = poly.GetPath (i);
				foreach (Vector2 _vertex in _vertices)
				{
					Vector3 vertex3D = transform.TransformPoint (new Vector3 (_vertex.x, _vertex.y, transform.position.z));
					_vertexData.Add (new Vector2 (vertex3D.x, vertex3D.y));
				}
			}
			vertexData = _vertexData.ToArray ();
		}


		/**
		 * Draws the outline of the GameObject's PolygonCollider2D shape in the Scene window
		 */
		public virtual void DrawGizmos ()
		{
			if (GetComponent <PolygonCollider2D>())
			{
				AdvGame.DrawPolygonCollider (transform, GetComponent <PolygonCollider2D>(), Color.white);
			}
		}

	}

}