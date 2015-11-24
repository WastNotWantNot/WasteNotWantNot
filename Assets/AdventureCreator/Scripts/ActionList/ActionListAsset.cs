/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionListAsset.cs"
 * 
 *	This script stores a list of Actions in an asset file.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/**
	 * An ActionListAsset is a ScriptableObject that allows a List of Action objects to be stored within an asset file.
	 * When the file is run, the Actions are transferred to a local instance of RuntimeActionList and run from there.
	 */
	[System.Serializable]
	public class ActionListAsset : ScriptableObject
	{

		/** The Actions within this asset file */
		public List<AC.Action> actions = new List<AC.Action>();
		/** If True, the Actions will be skipped when the user presses the 'EndCutscene' Input button */
		public bool isSkippable = true;
		/** The effect that running the Actions has on the rest of the game (PauseGameplay, RunInBackground) */
		public ActionListType actionListType = ActionListType.PauseGameplay;
		/** If True, the game will un-freeze itself while the Actions run if the game was previously paused due to an enabled Menu */
		public bool unfreezePauseMenus = true;
		/** If True, ActionParameters can be used to override values within the Action objects */
		public bool useParameters = false;
		/** A List of ActionParameter objects that can be used to override values within the Actions, if useParameters = True */
		public List<ActionParameter> parameters = new List<ActionParameter>();


		/**
		 * <summary>Checks if the ActionListAsset is skippable. This is safer than just reading 'isSkippable', because it also accounts for actionListType - since ActionLists that run in the background cannot be skipped</summary>
		 * <returns>True if the ActionListAsset is skippable</returns>
		 */
		public bool IsSkippable ()
		{
			if (isSkippable && actionListType == ActionListType.PauseGameplay)
			{
				return true;
			}
			return false;
		}


		/**
		 * <summary>Runs the ActionList asset file</summary>
		 */
		public void Interact ()
		{
			AdvGame.RunActionListAsset (this);
		}


		/**
		 * <summary>Runs the ActionList asset file, after setting the value of an integer parameter if it has one.</summary>
		 * <param name = "parameterID">The ID of the Integer parameter to set</param>
		 * <param name = "parameterValue">The value to set the Integer parameter to</param>
		 */
		public RuntimeActionList Interact (int parameterID, int parameterValue)
		{
			return AdvGame.RunActionListAsset (this, parameterID, parameterValue);
		}


	}


	public class ActionListAssetMenu
	{

		#if UNITY_EDITOR
	
		[MenuItem ("Assets/Create/Adventure Creator/ActionList")]
		public static ActionListAsset CreateAsset ()
		{
			ScriptableObject t = CustomAssetUtility.CreateAsset <ActionListAsset> ("New ActionList");
			return (ActionListAsset) t;
		}


		public static ActionListAsset AssetGUI (string label, ActionListAsset actionListAsset)
		{
			EditorGUILayout.BeginHorizontal ();
			actionListAsset = (ActionListAsset) EditorGUILayout.ObjectField (label, actionListAsset, typeof (ActionListAsset), false);

			if (actionListAsset == null)
			{
				if (GUILayout.Button ("Create", GUILayout.MaxWidth (60f)))
				{
					actionListAsset = ActionListAssetMenu.CreateAsset ();
				}
			}

			EditorGUILayout.EndHorizontal ();
			return actionListAsset;
		}


		public static Cutscene CutsceneGUI (string label, Cutscene cutscene)
		{
			EditorGUILayout.BeginHorizontal ();
			cutscene = (Cutscene) EditorGUILayout.ObjectField (label, cutscene, typeof (Cutscene), true);

			if (cutscene == null)
			{
				if (GUILayout.Button ("Create", GUILayout.MaxWidth (60f)))
				{
					cutscene = SceneManager.AddPrefab ("Logic", "Cutscene", true, false, true).GetComponent <Cutscene>();
					cutscene.Initialise ();
				}
			}

			EditorGUILayout.EndHorizontal ();
			return cutscene;
		}

		#endif


	}

}