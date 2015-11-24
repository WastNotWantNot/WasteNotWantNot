/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"UltimateFPSIntegration.cs"
 * 
 *	This script contains a number of static functions for use
 *	in integrating AC with the Ultimate FPS asset
 *
 *	To allow for UFPS integration, the 'UltimateFPSIsPresent'
 *	preprocessor must be defined.  This can be done from
 *	Edit -> Project Settings -> Player, and entering
 *	'UltimateFPSIsPresent' into the Scripting Define Symbols text box
 *	for your game's build platform.
 *
 *	NOTE: AC is designed for UFPS v1.4.8 or later.
 * 
 */


using UnityEngine;
using System.Collections;


namespace AC
{

	/**
	 * A class the contains a number of static functions to assist with Ultimate FPS integration (v1.4.8 or later).
	 * To use Ultimate FPS with Adventure Creator, the 'UltimateFPSIsPresent' preprocessor must be defined.
	 */
	public class UltimateFPSIntegration : ScriptableObject
	{

		#if UltimateFPSIsPresent
		private static vp_FPCamera fpCameraObject = null;
		private static vp_FPController fpControllerObject = null;
		private static vp_FPInput fpInputObject = null;


		private static vp_FPCamera fpCamera
		{
			get
			{
				if (fpCameraObject != null) return fpCameraObject;
				else
				{
					fpCameraObject = GameObject.FindObjectOfType <vp_FPCamera>();
					return fpCameraObject;
				}
			}
			set
			{
				fpCameraObject = value;
			}
		}


		private static vp_FPController fpController
		{
			get
			{
				if (fpControllerObject != null) return fpControllerObject;
				else
				{
					fpControllerObject = GameObject.FindObjectOfType <vp_FPController>();
					return fpControllerObject;
				}
			}
			set
			{
				fpControllerObject = value;
			}
		}


		private static vp_FPInput fpInput
		{
			get
			{
				if (fpInputObject != null) return fpInputObject;
				else
				{
					fpInputObject = GameObject.FindObjectOfType <vp_FPInput>();
					return fpInputObject;
				}
			}
			set
			{
				fpInputObject = value;
			}
		}

		#endif


		/**
		 * <summary>Checks if the 'UltimateFPSIsPresent' preprocessor has been defined.</summary>
		 * <returns>True if the 'UltimateFPSIsPresent' preprocessor has been defined</returns>
		 */
		public static bool IsDefinePresent ()
		{
			#if UltimateFPSIsPresent
			return true;
			#else
			return false;
			#endif
		}


		/**
		 * <summary>Updates the UFPS Player prefab as needed. This is called every frame by StateHandler.</summary>
		 * <param name = "gameState">The game's current GameState</param>
		 */
		public static void _Update (GameState gameState)
		{
			bool cursorLock = false;
			bool moveLock = false;
			bool cameraIsOn = false;

			if (gameState == GameState.Normal)
			{
				cursorLock = KickStarter.playerInput.cursorIsLocked;
				moveLock = KickStarter.playerInput.CanDirectControlPlayer ();
				cameraIsOn = true;
			}

			if (gameState != GameState.Paused)
			{
				UltimateFPSIntegration.SetCameraEnabled (cameraIsOn);
			}

			UltimateFPSIntegration.SetMovementState (moveLock);
			UltimateFPSIntegration.SetCameraState (cursorLock);
		}


		/**
		 * <summary>Gets the Transform of the vp_FPCamera component.</summary>
		 * <returns>The Transform of the vp_FPCamera component.</returns>
		 */
		public static Transform GetFPCamTransform ()
		{
			#if UltimateFPSIsPresent
			if (fpCamera)
			{
				return fpCamera.transform;
			}
			#endif
			return Camera.main.transform;
		}


		/**
		 * <summary>Checks if the vp_FPInput component is forcing the mouse cursor (MouseCursorForce).</summary>
		 * <returns>Checks if the vp_FPInput component is forcing the mouse cursor</returns>
		 */
		public static bool IsCursorForced ()
		{
			#if UltimateFPSIsPresent
			if (fpInput)
			{
				return fpInput.MouseCursorForced;
			}
			#endif
			return false;
		}


		/**
		 * <summary>Checks if a supplied Camera has the vp_FPCamera component attached.</summary>
		 * <returns>Checks if a supplied Camera has the vp_FPCamera component attached.</returns>
		 */
		public static bool IsUFPSCamera (Camera camera)
		{
			#if UltimateFPSIsPresent
			if (camera != null && camera.GetComponent <vp_FPCamera>())
			{
				return true;
			}
			#endif
			return false;
		}


		/**
		 * <summary>Sets the state of the UFPS camera.</summary>
		 * <param name = "state">If True, the UFPS camera will be enabled, and AC's MainCamera will be disabled. If False, vice versa.</param>
		 * <param name = "force">If False, and state = False, and the MainCamera has no attached camera, then no change will be made</param>
		 */
		public static void SetCameraEnabled (bool state, bool force = false)
		{
			#if UltimateFPSIsPresent
			if (fpCamera)
			{
				if (state)
				{
					KickStarter.mainCamera.attachedCamera = null;
				}

				if (KickStarter.mainCamera.attachedCamera == null && !state && !force)
				{
					// Don't do anything if the MainCamera has nothing else to do
					fpCamera.tag = Tags.mainCamera;
					KickStarter.mainCamera.SetCameraTag (Tags.untagged);
					return;
				}

				// Need to disable camera, not gameobject, otherwise weapon cams will get wrong FOV
				foreach (Camera _camera in fpCamera.GetComponentsInChildren <Camera>())
				{
					_camera.enabled = state;
				}

				fpCamera.GetComponent <AudioListener>().enabled = state;
				KickStarter.mainCamera.SetAudioState (!state);

				if (state)
				{
					fpCamera.tag = Tags.mainCamera;
					KickStarter.mainCamera.SetCameraTag (Tags.untagged);
					KickStarter.mainCamera.Disable ();
				}
				else
				{
					fpCamera.tag = Tags.untagged;
					KickStarter.mainCamera.SetCameraTag (Tags.mainCamera);
					KickStarter.mainCamera.Enable ();
				}
			}
			#else
			ACDebug.Log ("The 'UltimateFPSIsPresent' preprocessor is not defined - check your Player Settings.");
			#endif
		}


		public static void SetMovementState (bool state)
		{
			#if UltimateFPSIsPresent
			if (!KickStarter.playerInput.CanDirectControlPlayer ())
			{
				state = false;
			}

			if (fpInput)
			{
				fpInput.AllowGameplayInput = state;
			}
			else
			{
				ACDebug.LogWarning ("Cannot find 'vp_FPInput' script on Player.");
			}

			if (fpController)
			{
				if (state == false)
				{
					fpController.Stop ();
					if (fpInput != null && fpInput.FPPlayer != null)
					{
						fpInput.FPPlayer.Attack.TryStop ();
					}
				}
			}
			else
			{
				ACDebug.LogWarning ("Cannot find 'vp_FPController' script on Player.");
			}
			#else
			ACDebug.LogWarning ("The 'UltimateFPSIsPresent' preprocessor is not defined - check your Player Settings.");
			#endif
		}


		public static void SetCameraState (bool state)
		{
			#if UltimateFPSIsPresent
			if (KickStarter.playerInput.IsFreeAimingLocked ())
			{
				state = false;
			}

			if (fpInput)
			{
				fpInput.MouseCursorForced = !state;
			}
			else
			{
				ACDebug.LogWarning ("Cannot find 'vp_FPInput' script on Player.");
			}
			#else
			ACDebug.Log ("The 'UltimateFPSIsPresent' preprocessor is not defined - check your Player Settings.");
			#endif
		}


		public static void Teleport (Vector3 position)
		{
			#if UltimateFPSIsPresent
			if (fpCamera)
			{
				fpController.SetPosition (position);
			}
			else
			{
				ACDebug.LogWarning ("Cannot find 'vp_FPController' script.");
			}
			#endif
		}


		public static void SetRotation (Vector3 rotation)
		{
			#if UltimateFPSIsPresent
			if (fpCamera)
			{
				fpCamera.SetRotation (new Vector2 (rotation.x, rotation.y), true, true);
			}
			else
			{
				ACDebug.LogWarning ("Cannot find 'vp_FPCamera' script.");
			}
			#endif
		}


		public static void SetPitch (float pitch)
		{
			#if UltimateFPSIsPresent
			fpCamera.Angle = new Vector2 (fpCamera.transform.eulerAngles.x, pitch);
			#endif
		}


		public static void SetTilt (float tilt)
		{
			#if UltimateFPSIsPresent
			fpCamera.Angle = new Vector2 (tilt, fpCamera.transform.eulerAngles.y);
			#endif
		}

	}
	
}