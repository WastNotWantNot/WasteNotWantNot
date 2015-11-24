/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ManagerPackage.cs"
 * 
 *	This script is used to store references to Manager assets,
 *	so that they can be quickly loaded into the game engine in bulk.
 * 
 */

using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/**
	 * An asset file that stores references to Manager assets, so that they can be quickly assigned in bulk.
	 */
	[System.Serializable]
	public class ManagerPackage : ScriptableObject
	{

		public ActionsManager actionsManager;
		public SceneManager sceneManager;
		public SettingsManager settingsManager;
		public InventoryManager inventoryManager;
		public VariablesManager variablesManager;
		public SpeechManager speechManager;
		public CursorManager cursorManager;
		public MenuManager menuManager;


		/**
		 * Assigns it's various Manager asset files.
		 */
		public void AssignManagers ()
		{
			if (AdvGame.GetReferences () != null)
			{
				if (sceneManager)
				{
					AdvGame.GetReferences ().sceneManager = sceneManager;
				}
				
				if (settingsManager)
				{
					AdvGame.GetReferences ().settingsManager = settingsManager;
				}
				
				if (actionsManager)
				{
					AdvGame.GetReferences ().actionsManager = actionsManager;
				}
				
				if (variablesManager)
				{
					AdvGame.GetReferences ().variablesManager = variablesManager;
				}
				
				if (inventoryManager)
				{
					AdvGame.GetReferences ().inventoryManager = inventoryManager;
				}
				
				if (speechManager)
				{
					AdvGame.GetReferences ().speechManager = speechManager;
				}
				
				if (cursorManager)
				{
					AdvGame.GetReferences ().cursorManager = cursorManager;
				}
				
				if (menuManager)
				{
					AdvGame.GetReferences ().menuManager = menuManager;
				}

				#if UNITY_EDITOR
				if (KickStarter.sceneManager)
				{
					KickStarter.sceneManager.GetPrefabsInScene ();
				}

				AssetDatabase.SaveAssets ();
				#endif

				if (this)
				{
					ACDebug.Log (this.name + " - Managers assigned.");
				}
			}
			else
			{
				ACDebug.LogError ("Can't assign managers - no References file found in Resources folder.");
			}
		}

	}

}