/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionListManager.cs"
 * 
 *	This script keeps track of which ActionLists
 *	are running in a scene.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	/**
	 * This component keeps track of which ActionLists are running.
	 * When an ActionList runs or ends, it is passed to this script, which sets up the correct GameState in StateHandler.
	 * It should be placed on the GameEngine prefab.
	 */
	public class ActionListManager : MonoBehaviour
	{

		/** If True, then the next time ActionConversation's Skip() function is called, it will be ignored */
		[HideInInspector] public bool ignoreNextConversationSkip = false;

		private bool playCutsceneOnVarChange = false;
		private bool saveAfterCutscene = false;

		private int playerIDOnStartQueue;
		private Conversation conversationOnEnd;
		private List<ActionList> activeLists = new List<ActionList>();
		private List<SkipList> skipQueue = new List<SkipList>();
		private SkipList activeConversationPoint = new SkipList ();

		
		private void Awake ()
		{
			activeLists.Clear ();
		}
		

		/**
		 * Checks for autosaving and changed variables.
		 * This is called every frame by StateHandler.
		 */
		public void UpdateActionListManager ()
		{
			if (saveAfterCutscene && !IsGameplayBlocked ())
			{
				saveAfterCutscene = false;
				SaveSystem.SaveAutoSave ();
			}
			
			if (playCutsceneOnVarChange && KickStarter.stateHandler && (KickStarter.stateHandler.gameState == GameState.Normal || KickStarter.stateHandler.gameState == GameState.DialogOptions))
			{
				playCutsceneOnVarChange = false;
				
				if (KickStarter.sceneSettings.cutsceneOnVarChange != null)
				{
					KickStarter.sceneSettings.cutsceneOnVarChange.Interact ();
				}
			}
		}
		

		/**
		 * Ends all skippable ActionLists.
		 * This is triggered when the user presses the "EndCutscene" Input button.
		 */
		public void EndCutscene ()
		{
			if (!IsGameplayBlocked ())
			{
				return;
			}
			
			if (AdvGame.GetReferences ().settingsManager.blackOutWhenSkipping)
			{
				KickStarter.mainCamera.HideScene ();
			}
			
			// Stop all non-looping sound
			Sound[] sounds = FindObjectsOfType (typeof (Sound)) as Sound[];
			foreach (Sound sound in sounds)
			{
				if (sound.GetComponent <AudioSource>())
				{
					if (sound.soundType != SoundType.Music && !sound.GetComponent <AudioSource>().loop)
					{
						sound.Stop ();
					}
				}
			}

			for (int i=0; i<activeLists.Count; i++)
			{
				if (!ListIsInSkipQueue (activeLists[i]) && activeLists[i].IsSkippable ())
				{
					// Kill, but do isolated, to bypass setting GameState etc
					ActionList listToRemove = activeLists[i];
					listToRemove.ResetList ();
					activeLists.RemoveAt (i);
					i-=1;
					
					if (listToRemove is RuntimeActionList)
					{
						Destroy (listToRemove.gameObject);
					}
				}
			}

			// Set correct Player prefab
			if (KickStarter.player != null && playerIDOnStartQueue != KickStarter.player.ID && playerIDOnStartQueue >= 0)
			{
				Player playerToRevertTo = KickStarter.settingsManager.GetPlayer (playerIDOnStartQueue);
				KickStarter.ResetPlayer (playerToRevertTo, playerIDOnStartQueue, true, Quaternion.identity);
			}

			for (int i=0; i<skipQueue.Count; i++)
			{
				skipQueue[i].Skip ();
			}
		}
			

		/**
		 * <summary>Checks if any ActionLists are currently running.</summary>
		 * <returns>True if any ActionLists are currently running.</returns>
		 */
		public bool AreActionListsRunning ()
		{
			if (activeLists.Count > 0)
			{
				return true;
			}
			return false;
		}


		/**
		 * <summary>Checks if a particular ActionListAsset file is running.</summary>
		 * <param name = "_assetSource">The ActionListAsset to search for</param>
		 * <returns>True if the ActionListAsset file is currently running</returns>
		 */
		public bool IsListRunning (ActionListAsset _assetSource)
		{
			RuntimeActionList[] runtimeActionLists = FindObjectsOfType (typeof (RuntimeActionList)) as RuntimeActionList[];
			foreach (RuntimeActionList runtimeActionList in runtimeActionLists)
			{
				if (runtimeActionList.assetSource == _assetSource)
				{
					return true;
				}
			}
			return false;
		}
		

		/**
		 * <summary>Checks if a particular ActionList is running.</summary>
		 * <param name = "_list">The ActionList to search for</param>
		 * <returns>True if the ActionList is currently running</returns>
		 */
		public bool IsListRunning (ActionList _list)
		{
			foreach (ActionList list in activeLists)
			{
				if (list == _list)
				{
					return true;
				}
			}
			
			return false;
		}


		/**
		 * <summary>Checks if any currently-running ActionLists pause gameplay.</summary>
		 * <param title = "_actionToIgnore">Any ActionList that contains this Action will be excluded from the check</param>
		 * <returns>True if any currently-running ActionLists pause gameplay</returns>
		 */
		public bool IsGameplayBlocked (Action _actionToIgnore = null)
		{
			if (KickStarter.stateHandler.IsInScriptedCutscene ())
			{
				return true;
			}
			
			foreach (ActionList list in activeLists)
			{
				if (list.actionListType == ActionListType.PauseGameplay)
				{
					if (_actionToIgnore != null)
					{
						if (list.actions.Contains (_actionToIgnore))
						{
							continue;
						}
					}
					return true;
				}
			}
			return false;
		}
		
		
		/**
		 * <summary>Checks if any currently-running ActionListAssets pause gameplay and unfreeze 'Pause' Menus.</summary>
		 * <returns>True if any currently-running ActionListAssets pause gameplay and unfreeze 'Pause' Menus.</returns>
		 */
		public bool IsGameplayBlockedAndUnfrozen ()
		{
			foreach (ActionList list in activeLists)
			{
				if (list.actionListType == ActionListType.PauseGameplay && list.source == ActionListSource.AssetFile && list.unfreezePauseMenus)
				{
					ACDebug.Log (list.source);
					return true;
				}
			}
			return false;
		}
		
		
		/**
		 * <summary>Checks if any skippable ActionLists are currently running.</summary>
		 * <returns>True if any skippable ActionLists are currently running.</returns>
		 */
		public bool IsInSkippableCutscene ()
		{
			foreach (ActionList list in activeLists)
			{
				if (list.IsSkippable ())
				{
					return true;
				}
			}
			
			return false;
		}


		#if UNITY_EDITOR

		private Rect debugWindowRect = new Rect (0, 0, 220, 500);

		private void OnGUI ()
		{
			if (KickStarter.settingsManager.showActiveActionLists)
			{
				debugWindowRect.height = 21f;
				debugWindowRect = GUILayout.Window (0, debugWindowRect, StatusWindow, "AC status", GUILayout.Width (220));
			}
		}


		private void StatusWindow (int windowID)
		{
			GUISkin testSkin = (GUISkin) Resources.Load ("SceneManagerSkin");
			GUI.skin = testSkin;
			
			GUILayout.Label ("Current game state: " + KickStarter.stateHandler.gameState.ToString ());
			
			if (KickStarter.stateHandler.gameState == GameState.DialogOptions && KickStarter.playerInput.activeConversation != null)
			{
				if (GUILayout.Button ("Conversation: " + KickStarter.playerInput.activeConversation.gameObject.name))
				{
					UnityEditor.EditorGUIUtility.PingObject (KickStarter.playerInput.activeConversation.gameObject);
				}
			}
			
			GUILayout.Space (4f);
			
			if (activeLists.Count > 0)
			{
				GUILayout.Label ("Current ActionLists running:");
				
				foreach (ActionList list in activeLists)
				{
					if (GUILayout.Button (list.gameObject.name))
					{
						UnityEditor.EditorGUIUtility.PingObject (list.gameObject);
					}
				}
				
				if (IsGameplayBlocked ())
				{
					GUILayout.Space (4f);
					GUILayout.Label ("Gameplay is blocked");
				}
			}
		}

		#endif


		private bool ListIsInSkipQueue (ActionList _list)
		{
			foreach (SkipList skipList in skipQueue)
			{
				if (skipList.actionList == _list)
				{
					return true;
				}
			}
			return false;
		}
		

		/**
		 * <summary>Adds a new ActionList, assumed to already be running, to the internal record of currently-running ActionLists, and sets the correct GameState in StateHandler.</summary>
		 * <param name = "_list">The ActionList to run</param>
		 * <param name = "addToSkipQueue">If True, then the ActionList will be added to the list of ActionLists to skip</param>
		 * <param name = "_startIndex">The index number of the Action to start skipping from, if addToSkipQueue = True</param>
		 */
		public void AddToList (ActionList _list, bool addToSkipQueue, int _startIndex)
		{
			if (skipQueue.Count == 0 && _list.IsSkippable ())
			{
				if (KickStarter.player)
				{
					playerIDOnStartQueue = KickStarter.player.ID;
				}
				else
				{
					playerIDOnStartQueue = -1;
				}
				addToSkipQueue = true;
			}

			if (!IsListRunning (_list))
			{
				activeLists.Add (_list);

				if (addToSkipQueue && !ListIsInSkipQueue (_list) && _list.IsSkippable ())
				{
					skipQueue.Add (new SkipList (_list, _startIndex));
				}
			}
			
			if (_list.conversation)
			{
				conversationOnEnd = _list.conversation;
			}
			
			if (_list is RuntimeActionList && _list.actionListType == ActionListType.PauseGameplay && !_list.unfreezePauseMenus && KickStarter.playerMenus.ArePauseMenusOn (null))
			{
				// Don't affect the gamestate if we want to remain frozen
				return;
			}
			
			SetCorrectGameState ();
		}
		

		/**
		 * <summary>Resets and removes a ActionList from the internal record of currently-running ActionLists, and sets the correct GameState in StateHandler.</summary>
		 * <param name = "_list">The ActionList to end</param>
		 */
		public void EndList (ActionList _list)
		{
			if (_list == null)
			{
				return;
			}

			if (IsListRunning (_list))
			{
				activeLists.Remove (_list);
			}

			_list.ResetList ();
			
			if (_list.conversation == conversationOnEnd && _list.conversation != null)
			{
				if (KickStarter.stateHandler)
				{
					KickStarter.stateHandler.gameState = GameState.Cutscene;
				}
				else
				{
					ACDebug.LogWarning ("Could not set correct GameState!");
				}

				ResetSkipVars ();
				conversationOnEnd.Interact ();
				conversationOnEnd = null;
			}
			else
			{
				if (_list is RuntimeActionList && _list.actionListType == ActionListType.PauseGameplay && !_list.unfreezePauseMenus && KickStarter.playerMenus.ArePauseMenusOn (null))
				{
					// Don't affect the gamestate if we want to remain frozen
					if (KickStarter.stateHandler.gameState != GameState.Cutscene)
					{
						ResetSkipVars ();
					}
				}
				else
				{
					SetCorrectGameStateEnd ();
				}
			}
			
			if (_list.autosaveAfter)
			{
				if (!IsGameplayBlocked ())
				{
					SaveSystem.SaveAutoSave ();
				}
				else
				{
					saveAfterCutscene = true;
				}
			}
			
			if (_list is RuntimeActionList)
			{
				RuntimeActionList runtimeActionList = (RuntimeActionList) _list;
				runtimeActionList.DestroySelf ();
			}
		}


		/**
		 * <summary>Destroys the RuntimeActionList scene object that is running Actions from an ActionListAsset.</summary>
		 * <param name = "asset">The asset file that the RuntimeActionList has sourced it's Actions from</param>
		 */
		public void DestroyAssetList (ActionListAsset asset)
		{
			RuntimeActionList[] runtimeActionLists = FindObjectsOfType (typeof (RuntimeActionList)) as RuntimeActionList[];
			foreach (RuntimeActionList runtimeActionList in runtimeActionLists)
			{
				if (runtimeActionList.assetSource == asset)
				{
					if (activeLists.Contains (runtimeActionList))
					{
						activeLists.Remove (runtimeActionList);
					}
					Destroy (runtimeActionList.gameObject);
				}
			}
		}


		/**
		 * <summary>Stops an ActionListAsset from running.</summary>
		 * <param name = "The ActionListAsset file to stop"></param>
		 * <param name = "_action">An Action that, if present within 'asset', will prevent the ActionListAsset from ending prematurely</param>
		 */
		public void EndAssetList (ActionListAsset asset, Action _action = null)
		{
			RuntimeActionList[] runtimeActionLists = FindObjectsOfType (typeof (RuntimeActionList)) as RuntimeActionList[];
			foreach (RuntimeActionList runtimeActionList in runtimeActionLists)
			{
				if (runtimeActionList.assetSource == asset)
				{
					if (_action == null || !runtimeActionList.actions.Contains (_action))
					{
						EndList (runtimeActionList);
					}
					else if (_action != null) ACDebug.Log ("Left " + runtimeActionList.gameObject.name + " alone.");
				}
			}
		}
		

		/**
		 * Inform ActionListManager that a Variable's value has changed.
		 */
		public void VariableChanged ()
		{
			playCutsceneOnVarChange = true;
		}


		/**
		 * Ends all currently-running ActionLists and ActionListAssets.
		 */
		public void KillAllLists ()
		{
			foreach (ActionList _list in activeLists)
			{
				_list.ResetList ();
				
				if (_list is RuntimeActionList)
				{
					RuntimeActionList runtimeActionList = (RuntimeActionList) _list;
					runtimeActionList.DestroySelf ();
				}
			}
			
			activeLists.Clear ();
		}
		

		/**
		 * Ends all currently-running ActionLists and ActionListAssets.
		 */
		public static void KillAll ()
		{
			KickStarter.actionListManager.KillAllLists ();
		}
		
		
		private void SetCorrectGameStateEnd ()
		{
			if (KickStarter.stateHandler != null)
			{
				if (KickStarter.playerMenus.ArePauseMenusOn (null))
				{
					KickStarter.mainCamera.PauseGame ();
				}
				else
				{
					KickStarter.stateHandler.RestoreLastNonPausedState ();
				}

				if (KickStarter.stateHandler.gameState != GameState.Cutscene)
				{
					ResetSkipVars ();
				}
			}
			else
			{
				ACDebug.LogWarning ("Could not set correct GameState!");
			}
		}


		private void ResetSkipVars ()
		{
			if (!IsGameplayBlocked ())
			{
				ignoreNextConversationSkip = false;
				skipQueue.Clear ();
				GlobalVariables.BackupAll ();
				KickStarter.localVariables.BackupAllValues ();
			}
		}

		
		private void SetCorrectGameState ()
		{
			if (KickStarter.stateHandler != null)
			{
				if (IsGameplayBlocked ())
				{
					if (KickStarter.stateHandler.gameState != GameState.Cutscene)
					{
						ResetSkipVars ();
					}
					KickStarter.stateHandler.gameState = GameState.Cutscene;
				}
				else if (KickStarter.playerMenus.ArePauseMenusOn (null))
				{
					KickStarter.stateHandler.gameState = GameState.Paused;
					KickStarter.sceneSettings.PauseGame ();
				}
				else
				{
					if (KickStarter.playerInput.activeConversation != null)
					{
						KickStarter.stateHandler.gameState = GameState.DialogOptions;
					}
					else
					{
						KickStarter.stateHandler.gameState = GameState.Normal;
					}
				}
			}
			else
			{
				ACDebug.LogWarning ("Could not set correct GameState!");
			}
		}
		

		private void OnDestroy ()
		{
			activeLists.Clear ();
		}


		/**
		 * <summary>Sets the point to continue from, when a Conversation's options are overridden by an ActionConversation.</summary>
		 * <param title = "actionConversation">The "Dialogue: Start conversation" Action that is overriding the Conversation's options</param>
		 */
		public void SetConversationPoint (ActionConversation actionConversation)
		{
			if (actionConversation == null)
			{
				activeConversationPoint = new SkipList();
			}

			foreach (ActionList actionList in activeLists)
			{
	           foreach (Action action in actionList.actions)
				{
					if (action == actionConversation)
					{
						activeConversationPoint = new SkipList (actionList, actionList.actions.IndexOf (action));
						if (!(actionList is RuntimeActionList))
						{
							actionList.Kill ();
						}
						return;
					}
				}
			}
		}


		/**
		 * <summary>Attempts to override a Conversation object's default options by resuming an ActionList from the last ActionConversation.</summary>
		 * <param title = "optionIndex">The index number of the chosen dialogue option.</param>
		 * <returns>True if the override was succesful.</returns>
		 */
		public bool OverrideConversation (int optionIndex)
		{
			KickStarter.playerInput.lastConversationOption = optionIndex;

			SkipList tempPoint = new SkipList (activeConversationPoint);
			activeConversationPoint = new SkipList ();

			if (tempPoint != null && (tempPoint.actionList != null || tempPoint.actionListAsset != null))
			{
				tempPoint.Resume ();
				return true;
			}

			KickStarter.playerInput.lastConversationOption = -1; //
			return false;
		}
		
	}
	
}