/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"_Camera.cs"
 * 
 *	This is the base class for all other cameras.
 * 
 */


using UnityEngine;
using System.Collections;

namespace AC
{

	/**
	 * The base class for all Adventure Creator cameras.
	 * To integrate a custom camera script to AC, just add this component to the same object as the Camera component, and it will be visible to AC's fields, functions and Actions.
	 */
	public class _Camera : MonoBehaviour
	{

		/** The Unity Camera component */
		public Camera _camera;
		/** If True, the camera will move according to the Player prefab */
		public bool targetIsPlayer = true;
		/** The Transform that affects the camera's movement */
		public Transform target;
		/** If True, then the camera can be drag-controlled (used for GameCameraThirdPerson only) */
		public bool isDragControlled = false;
		/** The camera's focal distance */
		public float focalDistance = 10f;

		protected Vector2 inputMovement;


		protected virtual void Awake ()
		{
			if (GetComponent <Camera>())
			{
				_camera = GetComponent <Camera>();

				if (KickStarter.mainCamera)
				{
					_camera.enabled = false;
				}
			}
			else
			{
				ACDebug.LogWarning (this.name + " has no Camera component!");
			}
		}


		/**
		 * Auto-assigns "_camera" as the Unity Camera component on the same object as this script
		 */
		public void SetCameraComponent ()
		{
			if (_camera == null && GetComponent <Camera>())
			{
				_camera = GetComponent <Camera>();
			}
		}


		/**
		 * Auto-assigns "target" as the Player prefab Transform if targetIsPlayer = True.
		 */
		public void ResetTarget ()
		{
			if (targetIsPlayer && KickStarter.player)
			{
				target = KickStarter.player.transform;
			}
		}


		protected Vector3 PositionRelativeToCamera (Vector3 _position)
		{
			return (_position.x * ForwardVector ()) + (_position.z * RightVector ());
		}
		
		
		protected Vector3 RightVector ()
		{
			return (transform.right);
		}
		
		
		protected Vector3 ForwardVector ()
		{
			Vector3 camForward;
			
			camForward = transform.forward;
			camForward.y = 0;
			
			return (camForward);
		}
		

		/**
		 * Moves the camera instantly to it's destination.
		 */
		public virtual void MoveCameraInstant ()
		{ }


		protected float ConstrainAxis (float desired, Vector2 range)
		{
			if (range.x < range.y)
			{
				desired = Mathf.Clamp (desired, range.x, range.y);
			}
			
			else if (range.x > range.y)
			{
				desired = Mathf.Clamp (desired, range.y, range.x);
			}
			
			else
			{
				desired = range.x;
			}
				
			return desired;
		}


		/**
		 * Enables the camera for split-screen, using the MainCamera as the "main" part of the split, with all the data.
		 */
		public void SetSplitScreen ()
		{
			_camera.enabled = true;
			_camera.rect = KickStarter.mainCamera.GetSplitScreenRect (false);
		}


		/**
		 * Removes the split-screen effect on this camera.
		 */
		public void RemoveSplitScreen ()
		{
			if (_camera.enabled)
			{
				_camera.rect = new Rect (0f, 0f, 1f, 1f);
				_camera.enabled = false;
			}
		}

	}

}