/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionScene.cs"
 * 
 *	This action loads a new scene.
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

	[System.Serializable]
	public class ActionScene : Action
	{
		
		public ChooseSceneBy chooseSceneBy = ChooseSceneBy.Number;
		public int sceneNumber;
		public int sceneNumberParameterID = -1;
		public string sceneName;
		public int sceneNameParameterID = -1;
		public bool assignScreenOverlay;
		public bool onlyPreload = false;

		public bool relativePosition = false;
		public Marker relativeMarker;
		public int relativeMarkerID;
		public int relativeMarkerParameterID = -1;


		public ActionScene ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Engine;
			title = "Change scene";
			description = "Moves the Player to a new scene. The scene must be listed in Unity's Build Settings. By default, the screen will cut to black during the transition, but the last frame of the current scene can instead be overlayed. This allows for cinematic effects: if the next scene fades in, it will cause a crossfade effect; if the next scene doesn't fade, it will cause a straight cut.";
			numSockets = 0;
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			sceneNumber = AssignInteger (parameters, sceneNumberParameterID, sceneNumber);
			sceneName = AssignString (parameters, sceneNameParameterID, sceneName);
			relativeMarker = AssignFile <Marker> (parameters, relativeMarkerParameterID, relativeMarkerID, relativeMarker);
		}
		
		
		override public float Run ()
		{
			if (!assignScreenOverlay || onlyPreload)
			{
				ChangeScene ();
				return 0f;
			}

			if (!isRunning)
			{
				isRunning = true;
				KickStarter.mainCamera._ExitSceneWithOverlay ();
				return defaultPauseTime;
			}
			else
			{
				ChangeScene ();
				isRunning = false;
				return 0f;
			}
		}


		override public void Skip ()
		{
			ChangeScene ();
		}


		private void ChangeScene ()
		{
			if (sceneNumber > -1 || chooseSceneBy == ChooseSceneBy.Name)
			{
				SceneInfo sceneInfo = new SceneInfo (chooseSceneBy, sceneName, sceneNumber);

				if (!onlyPreload && relativePosition && relativeMarker != null)
				{
					KickStarter.sceneChanger.SetRelativePosition (relativeMarker.transform);
				}

				if (onlyPreload)
				{
					if (AdvGame.GetReferences ().settingsManager.useAsyncLoading)
					{
						KickStarter.sceneChanger.PreloadScene (sceneInfo);
					}
					else
					{
						ACDebug.LogWarning ("To pre-load scenes, 'Load scenes asynchronously?' must be enabled in the Settings Manager.");
					}
				}
				else
				{
					KickStarter.sceneChanger.ChangeScene (sceneInfo, true);
				}
			}
		}


		override public ActionEnd End (List<Action> actions)
		{
			return GenerateStopActionEnd ();
		}
		

		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			chooseSceneBy = (ChooseSceneBy) EditorGUILayout.EnumPopup ("Choose scene by:", chooseSceneBy);
			if (chooseSceneBy == ChooseSceneBy.Name)
			{
				sceneNameParameterID = Action.ChooseParameterGUI ("Scene name:", parameters, sceneNameParameterID, ParameterType.String);
				if (sceneNameParameterID < 0)
				{
					sceneName = EditorGUILayout.TextField ("Scene name:", sceneName);
				}
			}
			else
			{
				sceneNumberParameterID = Action.ChooseParameterGUI ("Scene number:", parameters, sceneNumberParameterID, ParameterType.Integer);
				if (sceneNumberParameterID < 0)
				{
					sceneNumber = EditorGUILayout.IntField ("Scene number:", sceneNumber);
				}
			}

			relativePosition = EditorGUILayout.ToggleLeft ("Position Player relative to Marker?", relativePosition);
			if (relativePosition)
			{
				relativeMarkerParameterID = Action.ChooseParameterGUI ("Relative Marker:", parameters, relativeMarkerParameterID, ParameterType.GameObject);
				if (relativeMarkerParameterID >= 0)
				{
					relativeMarkerID = 0;
					relativeMarker = null;
				}
				else
				{
					relativeMarker = (Marker) EditorGUILayout.ObjectField ("Relative Marker:", relativeMarker, typeof(Marker), true);
					
					relativeMarkerID = FieldToID (relativeMarker, relativeMarkerID);
					relativeMarker = IDToField (relativeMarker, relativeMarkerID, false);
				}
			}
			else
			{
				onlyPreload = EditorGUILayout.ToggleLeft ("Don't change scene, just preload data?", onlyPreload);
			}

			if (onlyPreload && !relativePosition)
			{
				if (AdvGame.GetReferences () != null && AdvGame.GetReferences ().settingsManager != null && AdvGame.GetReferences ().settingsManager.useAsyncLoading)
				{}
				else
				{
					EditorGUILayout.HelpBox ("To pre-load scenes, 'Load scenes asynchronously?' must be enabled in the Settings Manager.", MessageType.Warning);
				}

				AfterRunningOption ();
			}
			else
			{
				assignScreenOverlay = EditorGUILayout.ToggleLeft ("Overlay current screen during switch?", assignScreenOverlay);
			}
		}
		
		
		override public string SetLabel ()
		{
			if (chooseSceneBy == ChooseSceneBy.Name)
			{
				return (" (" + sceneName + ")");
			}
			return (" (" + sceneNumber + ")");
		}

		#endif
		
	}

}