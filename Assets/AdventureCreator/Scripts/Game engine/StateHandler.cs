/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"SceneHandler.cs"
 * 
 *	This script stores the gameState variable, which is used by
 *	other scripts to determine if the game is running normal gameplay,
 *	in a cutscene, paused, or displaying conversation options.
 * 
 */

using UnityEngine;
namespace AC
{

	/**
	 * This script stores the all-important gameState variable, which determines if the game is running normal gameplay, is in a cutscene, or is paused.
	 * It also runs the various "Update", "LateUpdate", "FixedUpdate" and "OnGUI" functions that are within Adventure Creator's main scripts - by running them all from here, performance is drastically improved.
	 */
	public class StateHandler : MonoBehaviour
	{

		/** The current state of the game (Normal, Cutscene, Paused, DialogOptions) */
		public GameState gameState = GameState.Normal;

		private bool inScriptedCutscene;
		private GameState previousUpdateState = GameState.Normal;
		private GameState lastNonPausedState = GameState.Normal;
		private bool isACDisabled = false;

		private bool cursorIsOff = false;
		private bool inputIsOff = false;
		private bool interactionIsOff = false;
		private bool menuIsOff = false;
		private bool movementIsOff = false;
		private bool cameraIsOff = false;
		private bool triggerIsOff = false;
		private bool playerIsOff = false;

		private bool originalMenuState;
		private bool originalCursorState;

		private bool playedGlobalOnStart = false;
		private bool hasGameEngine = false;

		private ArrowPrompt[] arrowPrompts;
		private DragBase[] dragBases;
		private Parallax2D[] parallax2Ds;
		private Hotspot[] hotspots;
		private Highlight[] highlights;
		private GameCamera2D[] gameCamera2Ds;
		private Sound[] sounds;
		private LimitVisibility[] limitVisibilitys;
		private Char[] characters;


		private void Awake ()
		{
			Time.timeScale = 1f;
			DontDestroyOnLoad (this);
			GetReferences ();
		}


		/**
		 * Alerts the StateHandler that a Game Engine prefab is present in the scene.
		 * This is called from KickStarter when the game begins - the StateHandler will not run until this is done.
		 */
		public void RegisterWithGameEngine ()
		{
			hasGameEngine = true;
		}


		/**
		 * Alerts the StateHandler that a Game Engine prefab is no longer present in the scene.
		 * This is called from KickStarter's OnDestroy function.
		 */
		public void UnregisterWithGameEngine ()
		{
			hasGameEngine = false;
		}


		private void OnLevelWasLoaded ()
		{
			hasGameEngine = false;
			GetReferences ();
		}


		/**
		 * <summary>Runs the ActionListAsset defined in SettingsManager's actionListOnStart when the game begins.</summary>
		 * <returns>True if an ActionListAsset was run</returns>
		 */
		public bool PlayGlobalOnStart ()
		{
			if (playedGlobalOnStart)
			{
				return false;
			}

			if (KickStarter.settingsManager.actionListOnStart)
			{
				AdvGame.RunActionListAsset (KickStarter.settingsManager.actionListOnStart);
				playedGlobalOnStart = true;
				return true;
			}

			return false;
		}


		/**
		 * Allows the ActionListAsset defined in SettingsManager's actionListOnStart to be run again.
		 */
		public void CanGlobalOnStart ()
		{
			playedGlobalOnStart = false;
		}


		private void GetReferences ()
		{
			inScriptedCutscene = false;
			if (KickStarter.settingsManager != null && KickStarter.settingsManager.IsInLoadingScene ())
			{
				return;
			}
			GatherObjects ();
		}


		/**
		 * <summary>Re-sets the internal arrays of GameObjects used to mass-update Adventure Creator each frame.
		 * It should be called whenever a GameObject is added to or removed from the scene.</summary>
		 * <param name = "afterDelete">If False, then IgnoreNavMeshCollisions() will also be run</param>
		 */
		public void GatherObjects (bool afterDelete = false)
		{
			dragBases = FindObjectsOfType (typeof (DragBase)) as DragBase[];
			hotspots = FindObjectsOfType (typeof (Hotspot)) as Hotspot[];
			arrowPrompts = FindObjectsOfType (typeof (ArrowPrompt)) as ArrowPrompt[];
			parallax2Ds = FindObjectsOfType (typeof (Parallax2D)) as Parallax2D[];
			highlights = FindObjectsOfType (typeof (Highlight)) as Highlight[];
			gameCamera2Ds = FindObjectsOfType (typeof (GameCamera2D)) as GameCamera2D[];
			sounds = FindObjectsOfType (typeof (Sound)) as Sound[];
			limitVisibilitys = FindObjectsOfType (typeof (LimitVisibility)) as LimitVisibility[];
			characters = FindObjectsOfType (typeof (Char)) as Char[];

			if (!afterDelete)
			{
				IgnoreNavMeshCollisions ();
			}
		}


		/**
		 * Calls Physics.IgnoreCollision on all appropriate Collider combinations (Unity 5 only).
		 */
		public void IgnoreNavMeshCollisions ()
		{
			#if UNITY_5
			Collider[] allColliders = FindObjectsOfType (typeof(Collider)) as Collider[];
			NavMeshBase[] navMeshes = FindObjectsOfType (typeof(NavMeshBase)) as NavMeshBase[];
			foreach (NavMeshBase navMesh in navMeshes)
			{
				if (navMesh.ignoreCollisions)
				{
					Collider _collider = navMesh.GetComponent <Collider>();
					if (_collider != null && _collider.enabled && _collider.gameObject.activeInHierarchy)
					{
						foreach (Collider otherCollider in allColliders)
						{
							if (!_collider.isTrigger && !otherCollider.isTrigger && otherCollider.enabled && otherCollider.gameObject.activeInHierarchy && !(_collider is TerrainCollider) && _collider != otherCollider)
							{
								Physics.IgnoreCollision (_collider, otherCollider);
							}
						}
					}
				}
			}
			#endif
		}


		private void Update ()
		{
			if (isACDisabled || !hasGameEngine)
			{
				return;
			}

			if (KickStarter.settingsManager.IsInLoadingScene () || KickStarter.sceneChanger.IsLoading ())
			{
				if (!menuIsOff)
				{
					KickStarter.playerMenus.UpdateLoadingMenus ();
				}
				return;
			}

			if (gameState != GameState.Paused)
			{
				lastNonPausedState = gameState;
			}

			if (!inputIsOff)
			{
				KickStarter.playerInput.UpdateInput ();

				if (gameState == GameState.Normal)
				{
					KickStarter.playerInput.UpdateDirectInput ();
				}

				if (gameState != GameState.Paused)
				{
					KickStarter.playerQTE.UpdateQTE ();
				}
			}

			KickStarter.dialog._Update ();

			if (!cursorIsOff)
			{
				KickStarter.playerCursor.UpdateCursor ();

				if (gameState == GameState.Normal && KickStarter.settingsManager && KickStarter.settingsManager.hotspotIconDisplay != HotspotIconDisplay.Never)
				{
					foreach (Hotspot hotspot in hotspots)
					{
						hotspot.UpdateIcon ();
						if (KickStarter.settingsManager.hotspotDrawing == ScreenWorld.WorldSpace && KickStarter.settingsManager.cameraPerspective != CameraPerspective.TwoD)
						{
							hotspot.DrawHotspotIcon (true);
						}
					}
				}
			}

			if (!menuIsOff)
			{
				KickStarter.playerMenus.UpdateAllMenus ();
			}

			if (!interactionIsOff)
			{
				KickStarter.playerInteraction.UpdateInteraction ();

				foreach (Highlight highlight in highlights)
				{
					highlight._Update ();
				}

				if (KickStarter.settingsManager.hotspotDetection == HotspotDetection.MouseOver && KickStarter.settingsManager.scaleHighlightWithMouseProximity)
				{
					if (gameState == GameState.Normal)
					{
						foreach (Hotspot hotspot in hotspots)
						{
							hotspot.SetProximity (true);
						}
					}
					else
					{
						foreach (Hotspot hotspot in hotspots)
						{
							hotspot.SetProximity (false);
						}
					}
				}
			}

			KickStarter.actionListManager.UpdateActionListManager ();

			if (!movementIsOff)
			{
				foreach (DragBase dragBase in dragBases)
				{
					dragBase.UpdateMovement ();
				}

				if (gameState == GameState.Normal && KickStarter.settingsManager && KickStarter.settingsManager.movementMethod != MovementMethod.None)
				{
					KickStarter.playerMovement.UpdatePlayerMovement ();
				}

				KickStarter.playerMovement.UpdateFPCamera ();
			}

			if (!interactionIsOff)
			{
				KickStarter.playerInteraction.UpdateInventory ();
			}

			if (!cameraIsOff)
			{
				foreach (GameCamera2D gameCamera2D in gameCamera2Ds)
				{
					gameCamera2D._Update ();
				}
			}

			foreach (LimitVisibility limitVisibility in limitVisibilitys)
			{
				limitVisibility._Update ();
			}

			foreach (Sound sound in sounds)
			{
				sound._Update ();
			}

			foreach (Char character in characters)
			{
				if (!playerIsOff || !(character is Player))
				{
					character._Update ();
				}
			}

			if (HasGameStateChanged ())
			{
				if (KickStarter.settingsManager.movementMethod == MovementMethod.UltimateFPS)
				{
					UltimateFPSIntegration._Update (gameState);
				}
				else if (KickStarter.settingsManager.movementMethod == MovementMethod.FirstPerson)
				{
					if (gameState == GameState.Normal || (gameState == GameState.DialogOptions && KickStarter.settingsManager.useFPCamDuringConversations))
					{
						KickStarter.mainCamera.SetFirstPerson ();
					}
				}

				if (gameState != GameState.Paused)
				{
					AudioListener.pause = false;
				}

				if (gameState == GameState.Cutscene && previousUpdateState != GameState.Cutscene)
				{
					KickStarter.playerMenus.MakeUINonInteractive ();
				}
				else if (gameState != GameState.Cutscene && previousUpdateState == GameState.Cutscene)
				{
					KickStarter.playerMenus.MakeUIInteractive ();
				}

				KickStarter.sceneSettings.OnStateChange ();
			}

			previousUpdateState = gameState;
		}


		private void LateUpdate ()
		{
			if (isACDisabled || !hasGameEngine)
			{
				return;
			}

			if (KickStarter.settingsManager != null && KickStarter.settingsManager.IsInLoadingScene ())
			{
				return;
			}

			foreach (Char character in characters)
			{
				character._LateUpdate ();
			}

			if (!cameraIsOff)
			{
				KickStarter.mainCamera._LateUpdate ();
			}

			foreach (Parallax2D parallax2D in parallax2Ds)
			{
				parallax2D.UpdateOffset ();
			}
		}


		private void FixedUpdate ()
		{
			if (isACDisabled || !hasGameEngine)
			{
				return;
			}

			if (KickStarter.settingsManager != null && KickStarter.settingsManager.IsInLoadingScene ())
			{
				return;
			}

			KickStarter.dialog._FixedUpdate ();

			foreach (Char character in characters)
			{
				if (!playerIsOff || !(character is Player))
				{
					character._FixedUpdate ();
				}
			}
		}


		/**
		 * Sets the maximum volume of all Sound objects in the scene.
		 */
		public void UpdateAllMaxVolumes ()
		{
			foreach (Sound sound in sounds)
			{
				sound.SetMaxVolume ();
			}
		}


		private bool HasGameStateChanged ()
		{
			if (previousUpdateState != gameState)
			{
				return true;
			}
			return false;
		}


		private void OnGUI ()
		{
			if (isACDisabled || !hasGameEngine)
			{
				return;
			}

			if (KickStarter.settingsManager.IsInLoadingScene () || KickStarter.sceneChanger.IsLoading ())
			{
				if (!cameraIsOff)
				{
					KickStarter.mainCamera.DrawCameraFade ();
				}
				if (!menuIsOff)
				{
					KickStarter.playerMenus.DrawLoadingMenus ();
				}
				if (!cameraIsOff)
				{
					KickStarter.mainCamera.DrawBorders ();
				}
				return;
			}

			if (!cursorIsOff && gameState == GameState.Normal && KickStarter.settingsManager)
			{
				if (KickStarter.settingsManager.hotspotIconDisplay != HotspotIconDisplay.Never &&
				   (KickStarter.settingsManager.hotspotDrawing == ScreenWorld.ScreenSpace || KickStarter.settingsManager.cameraPerspective == CameraPerspective.TwoD))
				{
					foreach (Hotspot hotspot in hotspots)
					{
						hotspot.DrawHotspotIcon ();
					}
				}

				foreach (DragBase dragBase in dragBases)
				{
					dragBase.DrawGrabIcon ();
				}
			}

			if (!inputIsOff)
			{
				if (gameState == GameState.DialogOptions)
				{
					KickStarter.playerInput.DetectConversationNumerics ();
				}
				KickStarter.playerInput.DrawDragLine ();
				
				foreach (ArrowPrompt arrowPrompt in arrowPrompts)
				{
					arrowPrompt.DrawArrows ();
				}
			}

			if (!menuIsOff)
			{
				KickStarter.playerMenus.DrawMenus ();
			}

			if (!cursorIsOff)
			{
				KickStarter.playerCursor.DrawCursor ();
			}

			if (!cameraIsOff)
			{
				KickStarter.mainCamera.DrawCameraFade ();
				KickStarter.mainCamera.DrawBorders ();
			}
		}


		/**
		 * <summary>Gets the last value of gameState that wasn't GameState.Paused.</summary>
		 * <returns>The last value of gameState that wasn't GameState.Paused</summary>
		 */
		public GameState GetLastNonPausedState ()
		{
			return lastNonPausedState;
		}
		

		/**
		 * Restores the gameState to it's former state after un-pausing the game.
		 */
		public void RestoreLastNonPausedState ()
		{
			if (Time.timeScale == 0f)
			{
				KickStarter.sceneSettings.UnpauseGame (KickStarter.playerInput.timeScale);
			}

			if (KickStarter.playerInteraction.inPreInteractionCutscene)
			{
				gameState = GameState.Cutscene;
				return;
			}

			KickStarter.playerInteraction.inPreInteractionCutscene = false;
			if (KickStarter.actionListManager.IsGameplayBlocked () || inScriptedCutscene)
			{
				gameState = GameState.Cutscene;
			}
			else if (KickStarter.playerInput.activeConversation != null)
			{
				gameState = GameState.DialogOptions;
			}
			else
			{
				gameState = GameState.Normal;
			}
		}


		/**
		 * <summary>Goes through all Hotspots in the scene, and limits their enabed state based on a specific _Camera, if appropriate.</summary>
		 * <param name = "_camera">The _Camera to attempt to limit all Hotspots to</param>
		 */
		public void LimitHotspotsToCamera (_Camera _camera)
		{
			if (_camera != null)
			{
				foreach (Hotspot hotspot in hotspots)
				{
					hotspot.LimitToCamera (_camera);
				}
			}
		}


		/**
		 * Begins a hard-coded cutscene.
		 * Gameplay will resume once EndCutscene() is called.
		 */
		public void StartCutscene ()
		{
			inScriptedCutscene = true;
			gameState = GameState.Cutscene;
		}


		/**
		 * Ends a hard-coded cutscene, started by calling StartCutscene().
		 */
		public void EndCutscene ()
		{
			inScriptedCutscene = false;
			if (KickStarter.playerMenus.ArePauseMenusOn (null))
			{
				KickStarter.mainCamera.PauseGame ();
			}
			else
			{
				KickStarter.stateHandler.RestoreLastNonPausedState ();
			}
		}


		/**
		 * <summary>Checks if the game is currently in a user-scripted cutscene.</summary>
		 * <returns>True if the game is currently in a user-scripted cutscene</returns>
		 */
		public bool IsInScriptedCutscene ()
		{
			return inScriptedCutscene;
		}


		/**
		 * <summary>Enables or disables Adventure Creator completely.</summary>
		 * <param name = "state">If True, then Adventure Creator will be enabled. If False, then Adventure Creator will be disabled.</param>
		 */
		public void SetACState (bool state)
		{
			isACDisabled = !state;
		}


		/**
		 * Backs up the state of the menu and cursor systems, and disables them, before taking a screenshot.
		 */
		public void PreScreenshotBackup ()
		{
			originalMenuState = menuIsOff;
			originalCursorState = cursorIsOff;
			menuIsOff = true;
			cursorIsOff = true;
		}


		/**
		 * Restores the menu and cursor systems to their former states, after taking a screenshot.
		 */
		public void PostScreenshotBackup ()
		{
			menuIsOff = originalMenuState;
			cursorIsOff = originalCursorState;
		}


		/**
		 * <summary>Sets the enabled state of the PlayerCursor system.</summary>
		 * <param name = "state">If True, the PlayerCursor system will be enabled</param>
		 */
		public void SetCursorSystem (bool state)
		{
			cursorIsOff = !state;
		}


		/**
		 * <summary>Sets the enabled state of the PlayerInput system.</summary>
		 * <param name = "state">If True, the PlayerInput system will be enabled</param>
		 */
		public void SetInputSystem (bool state)
		{
			inputIsOff = !state;
		}


		/**
		 * <summary>Sets the enabled state of the cursor system.</summary>
		 * <param name = "state">If True, the cursor system will be enabled</param>
		 */
		public void SetInteractionSystem (bool state)
		{
			interactionIsOff = !state;

			if (!state)
			{
				KickStarter.playerInteraction.DeselectHotspot (true);
			}
		}


		/**
		 * <summary>Checks if the interaction system is enabled.</summary>
		 * <returns>True if the interaction system is enabled</returns>
		 */
		public bool CanInteract ()
		{
			return !interactionIsOff;
		}


		/**
		 * <summary>Sets the enabled state of the PlayerMenus system.</summary>
		 * <param name = "state">If True, the PlayerMenus system will be enabled</param>
		 */
		public void SetMenuSystem (bool state)
		{
			menuIsOff = !state;
		}


		/**
		 * <summary>Sets the enabled state of the PlayerMovement system.</summary>
		 * <param name = "state">If True, the PlayerMovement system will be enabled</param>
		 */
		public void SetMovementSystem (bool state)
		{
			movementIsOff = !state;
		}


		/**
		 * <summary>Sets the enabled state of the MainCamera system.</summary>
		 * <param name = "state">If True, the MainCamera system will be enabled</param>
		 */
		public void SetCameraSystem (bool state)
		{
			cameraIsOff = !state;
		}


		/**
		 * <summary>Sets the enabled state of the trigger system.</summary>
		 * <param name = "state">If True, the trigger system will be enabled</param>
		 */
		public void SetTriggerSystem (bool state)
		{
			triggerIsOff = !state;
		}


		/**
		 * <summary>Sets the enabled state of the Player system.</summary>
		 * <param name = "state">If True, the Player system will be enabled</param>
		 */
		public void SetPlayerSystem (bool state)
		{
			playerIsOff = !state;
		}


		/**
		 * <summary>Checks if the trigger system is disabled.</summary>
		 * <returns>True if the trigger system is disabled</returns>
		 */
		public bool AreTriggersDisabled ()
		{
			return triggerIsOff;
		}


		/**
		 * <summary>Updates a MainData class with it's own variables that need saving.</summary>
		 * <param name = "mainData">The original MainData class</param>
		 * <returns>The updated MainData class</returns>
		 */
		public MainData SaveMainData (MainData mainData)
		{
			mainData.cursorIsOff = cursorIsOff;
			mainData.inputIsOff = inputIsOff;
			mainData.interactionIsOff = interactionIsOff;
			mainData.menuIsOff = menuIsOff;
			mainData.movementIsOff = movementIsOff;
			mainData.cameraIsOff = cameraIsOff;
			mainData.triggerIsOff = triggerIsOff;
			mainData.playerIsOff = playerIsOff;

			return mainData;
		}


		/**
		 * <summary>Updates it's own variables from a MainData class.</summary>
		 * <param name = "mainData">The MainData class to load from</param>
		 */
		public void LoadMainData (MainData mainData)
		{
			cursorIsOff = mainData.cursorIsOff;
			inputIsOff = mainData.inputIsOff;
			interactionIsOff = mainData.interactionIsOff;
			menuIsOff = mainData.menuIsOff;
			movementIsOff = mainData.movementIsOff;
			cameraIsOff = mainData.cameraIsOff;
			triggerIsOff = mainData.triggerIsOff;
			playerIsOff = mainData.playerIsOff;
		}

	}

}