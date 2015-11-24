﻿using UnityEngine;
using UnityEditor;
using System.Collections;

namespace AC
{

	[CustomEditor (typeof(DialogueOption))]
	[System.Serializable]
	public class DialogueOptionEditor : ActionListEditor
	{

		public override void OnInspectorGUI ()
		{
			DialogueOption _target = (DialogueOption) target;
			PropertiesGUI (_target);
			base.DrawSharedElements (_target);
			
			if (GUI.changed)
			{
				EditorUtility.SetDirty (_target);
			}
		}


		public static void PropertiesGUI (DialogueOption _target)
	    {
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Dialogue Option properties", EditorStyles.boldLabel);
			_target.source = (ActionListSource) EditorGUILayout.EnumPopup ("Actions source:", _target.source);
			if (_target.source == ActionListSource.AssetFile)
			{
				_target.assetFile = ActionListAssetMenu.AssetGUI ("ActionList asset:", _target.assetFile);
			}
			if (_target.actionListType == ActionListType.PauseGameplay)
			{
				_target.isSkippable = EditorGUILayout.Toggle ("Is skippable?", _target.isSkippable);
			}
			EditorGUILayout.EndVertical ();
		}

	}

}