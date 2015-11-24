/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"LevelStorage.cs"
 * 
 *	This script handles the loading and unloading of per-scene data.
 *	Below the main class is a series of data classes for the different object types.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

namespace AC
{

	/**
	 * Manages the loading and storage of per-scene data (the various Remember scripts).
	 * This needs to be attached to the PersistentEngine prefab
	 */
	public class LevelStorage : MonoBehaviour
	{

		/** A collection of level data for each visited scene */
		[HideInInspector] public List<SingleLevelData> allLevelData = new List<SingleLevelData>();
		
		
		private void Awake ()
		{
			allLevelData = new List<SingleLevelData>();
		}


		/**
		 * Wipes all stored scene save data from memory.
		 */
		public void ClearAllLevelData ()
		{
			allLevelData.Clear ();
			allLevelData = new List<SingleLevelData>();
		}



		/**
		 * Wipes the currently-loaded scene's save data from memory
		 */
		public void ClearCurrentLevelData ()
		{
			foreach (SingleLevelData levelData in allLevelData)
			{
				if (levelData.sceneNumber == Application.loadedLevel)
				{
					allLevelData.Remove (levelData);
					return;
				}
			}
		}
		

		/**
		 * <summary>Returns the currently-loaded scene's save data to the appriopriate Remember components.</summary>
		 * <param name = "restoringSaveFile">True if the game is currently loading a saved game file, as opposed to just switching scene</param>
		 */
		public void ReturnCurrentLevelData (bool restoringSaveFile)
		{
			foreach (SingleLevelData levelData in allLevelData)
			{
				if (levelData.sceneNumber == Application.loadedLevel)
				{
					UnloadCutsceneOnLoad (levelData.onLoadCutscene);
					UnloadCutsceneOnStart (levelData.onStartCutscene);
					UnloadNavMesh (levelData.navMesh);
					UnloadPlayerStart (levelData.playerStart);
					UnloadSortingMap (levelData.sortingMap);
					UnloadTintMap (levelData.tintMap);

					UnloadTransformData (levelData.allTransformData);

					foreach (ScriptData _scriptData in levelData.allScriptData)
					{
						Remember saveObject = Serializer.returnComponent <Remember> (_scriptData.objectID);				
						if (saveObject != null && _scriptData.data != null && _scriptData.data.Length > 0)
						{
							// May have more than one Remember script on the same object, so check all
							Remember[] saveScripts = saveObject.gameObject.GetComponents <Remember>();
							foreach (Remember saveScript in saveScripts)
							{
								saveScript.LoadData (_scriptData.data, restoringSaveFile);
							}
						}
					}

					UnloadVariablesData (levelData.localVariablesData);

					break;
				}
			}

			AssetLoader.UnloadAssets ();
		}
		

		/**
		 * Combs the scene for data to store, combines it into a SingleLevelData variable, and adds it to the SingleLevelData List, allLevelData.
		 */
		public void StoreCurrentLevelData ()
		{
			List<TransformData> thisLevelTransforms = PopulateTransformData ();
			List<ScriptData> thisLevelScripts = PopulateScriptData ();

			SingleLevelData thisLevelData = new SingleLevelData ();
			thisLevelData.sceneNumber = Application.loadedLevel;
			
			if (KickStarter.sceneSettings)
			{
				if (KickStarter.sceneSettings.navMesh && KickStarter.sceneSettings.navMesh.GetComponent <ConstantID>())
				{
					thisLevelData.navMesh = Serializer.GetConstantID (KickStarter.sceneSettings.navMesh.gameObject);
				}
				if (KickStarter.sceneSettings.defaultPlayerStart && KickStarter.sceneSettings.defaultPlayerStart.GetComponent <ConstantID>())
				{
					thisLevelData.playerStart = Serializer.GetConstantID (KickStarter.sceneSettings.defaultPlayerStart.gameObject);
				}
				if (KickStarter.sceneSettings.sortingMap && KickStarter.sceneSettings.sortingMap.GetComponent <ConstantID>())
				{
					thisLevelData.sortingMap = Serializer.GetConstantID (KickStarter.sceneSettings.sortingMap.gameObject);
				}
				if (KickStarter.sceneSettings.cutsceneOnLoad && KickStarter.sceneSettings.cutsceneOnLoad.GetComponent <ConstantID>())
				{
					thisLevelData.onLoadCutscene = Serializer.GetConstantID (KickStarter.sceneSettings.cutsceneOnLoad.gameObject);
				}
				if (KickStarter.sceneSettings.cutsceneOnStart && KickStarter.sceneSettings.cutsceneOnStart.GetComponent <ConstantID>())
				{
					thisLevelData.onStartCutscene = Serializer.GetConstantID (KickStarter.sceneSettings.cutsceneOnStart.gameObject);
				}
				if (KickStarter.sceneSettings.tintMap && KickStarter.sceneSettings.tintMap.GetComponent <ConstantID>())
				{
					thisLevelData.tintMap = Serializer.GetConstantID (KickStarter.sceneSettings.tintMap.gameObject);
				}
			}

			thisLevelData.localVariablesData = SaveSystem.CreateVariablesData (KickStarter.localVariables.localVars, false, VariableLocation.Local);
			thisLevelData.allTransformData = thisLevelTransforms;
			thisLevelData.allScriptData = thisLevelScripts;

			bool found = false;
			for (int i=0; i<allLevelData.Count; i++)
			{
				if (allLevelData[i].sceneNumber == Application.loadedLevel)
				{
					allLevelData[i] = thisLevelData;
					found = true;
					break;
				}
			}
			
			if (!found)
			{
				allLevelData.Add (thisLevelData);
			}
		}

		
		private void UnloadNavMesh (int navMeshInt)
		{
			NavigationMesh navMesh = Serializer.returnComponent <NavigationMesh> (navMeshInt);

			if (navMesh && KickStarter.sceneSettings && KickStarter.sceneSettings.navigationMethod != AC_NavigationMethod.UnityNavigation)
			{
				if (KickStarter.sceneSettings.navMesh)
				{
					NavigationMesh oldNavMesh = KickStarter.sceneSettings.navMesh;
					oldNavMesh.TurnOff ();
				}

				//navMesh.collider.GetComponent <NavigationMesh>().TurnOn ();
				navMesh.TurnOn ();
				KickStarter.sceneSettings.navMesh = navMesh;
			}
		}


		private void UnloadPlayerStart (int playerStartInt)
		{
			PlayerStart playerStart = Serializer.returnComponent <PlayerStart> (playerStartInt);

			if (playerStart && KickStarter.sceneSettings)
			{
				KickStarter.sceneSettings.defaultPlayerStart = playerStart;
			}
		}


		private void UnloadSortingMap (int sortingMapInt)
		{
			SortingMap sortingMap = Serializer.returnComponent <SortingMap> (sortingMapInt);

			if (sortingMap && KickStarter.sceneSettings)
			{
				KickStarter.sceneSettings.sortingMap = sortingMap;

				// Reset all FollowSortingMap components
				FollowSortingMap[] followSortingMaps = FindObjectsOfType (typeof (FollowSortingMap)) as FollowSortingMap[];
				foreach (FollowSortingMap followSortingMap in followSortingMaps)
				{
					followSortingMap.UpdateSortingMap ();
				}
			}
		}


		private void UnloadTintMap (int tintMapInt)
		{
			TintMap tintMap = Serializer.returnComponent <TintMap> (tintMapInt);
			
			if (tintMap && KickStarter.sceneSettings)
			{
				KickStarter.sceneSettings.tintMap = tintMap;
				
				// Reset all FollowTintMap components
				FollowTintMap[] followTintMaps = FindObjectsOfType (typeof (FollowTintMap)) as FollowTintMap[];
				foreach (FollowTintMap followTintMap in followTintMaps)
				{
					followTintMap.ResetTintMap ();
				}
			}
		}


		private void UnloadCutsceneOnLoad (int cutsceneInt)
		{
			Cutscene cutscene = Serializer.returnComponent <Cutscene> (cutsceneInt);

			if (cutscene && KickStarter.sceneSettings)
			{
				KickStarter.sceneSettings.cutsceneOnLoad = cutscene;
			}
		}


		private void UnloadCutsceneOnStart (int cutsceneInt)
		{
			Cutscene cutscene = Serializer.returnComponent <Cutscene> (cutsceneInt);

			if (cutscene && KickStarter.sceneSettings)
			{
				KickStarter.sceneSettings.cutsceneOnStart = cutscene;
			}
		}


		private List<TransformData> PopulateTransformData ()
		{
			List<TransformData> allTransformData = new List<TransformData>();
			RememberTransform[] transforms = FindObjectsOfType (typeof (RememberTransform)) as RememberTransform[];
			
			foreach (RememberTransform _transform in transforms)
			{
				if (_transform.constantID != 0)
				{
					allTransformData.Add (_transform.SaveTransformData ());
				}
				else
				{
					ACDebug.LogWarning ("GameObject " + _transform.name + " was not saved because it's ConstantID has not been set!");
				}
			}
			
			return allTransformData;
		}


		private void UnloadTransformData (List<TransformData> _transforms)
		{
			// Delete any objects (if told to)
			RememberTransform[] currentTransforms = FindObjectsOfType (typeof (RememberTransform)) as RememberTransform[];
			foreach (RememberTransform transformOb in currentTransforms)
			{
				if (transformOb.saveScenePresence)
				{
					// Was object not saved?
					bool found = false;
					foreach (TransformData _transform in _transforms)
					{
						if (_transform.objectID == transformOb.constantID)
						{
							found = true;
						}
					}

					if (!found)
					{
						// Can't find: delete
						Destroy (transformOb.gameObject);
					}
				}
			}

			foreach (TransformData _transform in _transforms)
			{
				RememberTransform saveObject = Serializer.returnComponent <RememberTransform> (_transform.objectID);

				// Restore any deleted objects (if told to)
				if (saveObject == null && _transform.bringBack)
				{
					Object[] assets = Resources.LoadAll ("", typeof (GameObject));
					foreach (Object asset in assets)
					{
						if (asset is GameObject)
						{
							GameObject assetObject = (GameObject) asset;
							if (assetObject.GetComponent <RememberTransform>() && assetObject.GetComponent <RememberTransform>().constantID == _transform.objectID)
							{
								GameObject newObject = (GameObject) Instantiate (assetObject.gameObject);
								newObject.name = assetObject.name;
								saveObject = newObject.GetComponent <RememberTransform>();
							}
						}
					}
					Resources.UnloadUnusedAssets ();
				}

				if (saveObject != null)
				{
					saveObject.LoadTransformData (_transform);
				}
			}
		}


		private List<ScriptData> PopulateScriptData ()
		{
			List<ScriptData> allScriptData = new List<ScriptData>();
			Remember[] scripts = FindObjectsOfType (typeof (Remember)) as Remember[];
			
			foreach (Remember _script in scripts)
			{
				if (_script.constantID != 0)
				{
					allScriptData.Add (new ScriptData (_script.constantID, _script.SaveData ()));
				}
				else
				{
					ACDebug.LogWarning ("GameObject " + _script.name + " was not saved because it's ConstantID has not been set!");
				}
			}
			
			return allScriptData;
		}


		private void AssignMenuLocks (List<Menu> menus, string menuLockData)
		{
			if (menuLockData.Length == 0)
			{
				return;
			}

			string[] lockArray = menuLockData.Split ("|"[0]);
			
			foreach (string chunk in lockArray)
			{
				string[] chunkData = chunk.Split (":"[0]);
				
				int _id = 0;
				int.TryParse (chunkData[0], out _id);
				
				bool _lock = false;
				bool.TryParse (chunkData[1], out _lock);
				
				foreach (AC.Menu _menu in menus)
				{
					if (_menu.id == _id)
					{
						_menu.isLocked = _lock;
						break;
					}
				}
			}
		}


		private void UnloadVariablesData (string data)
		{
			if (data == null)
			{
				return;
			}
			
			if (data.Length > 0)
			{
				string[] varsArray = data.Split ("|"[0]);
				
				foreach (string chunk in varsArray)
				{
					string[] chunkData = chunk.Split (":"[0]);
					
					int _id = 0;
					int.TryParse (chunkData[0], out _id);

					GVar var = LocalVariables.GetVariable (_id);
					if (var.type == VariableType.String)
					{
						string _text = chunkData[1];
						var.SetStringValue (_text);
					}
					else if (var.type == VariableType.Float)
					{
						float _value = 0f;
						float.TryParse (chunkData[1], out _value);
						var.SetFloatValue (_value, SetVarMethod.SetValue);
					}
					else
					{
						int _value = 0;
						int.TryParse (chunkData[1], out _value);
						var.SetValue (_value, SetVarMethod.SetValue);
					}
				}
			}
		}

	}
		

	/**
	 * A data container for a single scene's save data. Used by the LevelStorage component.
	 */
	[System.Serializable]
	public class SingleLevelData
	{

		/** A List of all data recorded by the scene's Remember scripts */
		public List<ScriptData> allScriptData;
		/** A List of all data recorded by the scene's RememberTransform scripts */
		public List<TransformData> allTransformData;
		/** The scene number this data is for */
		public int sceneNumber;

		/** The ConstantID number of the default NavMesh */
		public int navMesh;
		/** The ConstantID number of the default PlayerStart */
		public int playerStart;
		/** The ConstantID number of the scene's SortingMap */
		public int sortingMap;
		/** The ConstantID number of the scene's TintMap */
		public int tintMap;
		/** The ConstantID number of the "On load" Cutscene */
		public int onLoadCutscene;
		/** The ConstantID number of the "On start" Cutscene */
		public int onStartCutscene;

		/** The values of the scene's local Variables, combined into a single string */
		public string localVariablesData;


		/**
		 * The default Constructor.
		 */
		public SingleLevelData ()
		{
			allScriptData = new List<ScriptData>();
			allTransformData = new List<TransformData>();
		}

	}


	/**
	 * A data container for save data returned by each Remember script.  Used by the SingleLevelData class.
	 */
	[System.Serializable]
	public struct ScriptData
	{

		/** The Constant ID number of the Remember script component */
		public int objectID;
		/** The data returned by the Remember script, serialised into a string */
		public string data;


		/**
		 * <summary>The default Constructor.</summary>
		 * <param name = "_objectID">The Remember script's Constant ID number</param>
		 * <param name = "_data">The serialised data</param>
		 */
		public ScriptData (int _objectID, string _data)
		{
			objectID = _objectID;
			data = _data;
		}
	}

}