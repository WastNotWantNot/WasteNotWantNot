using UnityEngine;
using UnityEditor;
using System.Collections;

namespace AC
{

	[CustomEditor (typeof(Interaction))]
	[System.Serializable]
	public class InteractionEditor : ActionListEditor
	{

		public override void OnInspectorGUI ()
		{
			Interaction _target = (Interaction) target;
			PropertiesGUI (_target);
			base.DrawSharedElements (_target);
			
			if (GUI.changed)
			{
				EditorUtility.SetDirty (_target);
			}
		}


		public static void PropertiesGUI (Interaction _target)
	    {
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Interaction properties", EditorStyles.boldLabel);
			_target.source = (ActionListSource) EditorGUILayout.EnumPopup ("Actions source:", _target.source);
			if (_target.source == ActionListSource.AssetFile)
			{
				_target.assetFile = (ActionListAsset) EditorGUILayout.ObjectField ("ActionList asset:", _target.assetFile, typeof (ActionListAsset), false);
			}
			_target.actionListType = (ActionListType) EditorGUILayout.EnumPopup ("When running:", _target.actionListType);
			if (_target.actionListType == ActionListType.PauseGameplay)
			{
				_target.isSkippable = EditorGUILayout.Toggle ("Is skippable?", _target.isSkippable);
			}
			EditorGUILayout.EndVertical ();
	    }

	}

}