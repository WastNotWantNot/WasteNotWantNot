/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"CursorManager.cs"
 * 
 *	This script handles the "Cursor" tab of the main wizard.
 *	It is used to define cursor icons and the method in which
 *	interactions are triggered by the player.
 * 
 */

using UnityEngine;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/**
	 * Handles the "Cursor" tab of the Game Editor window.
	 * All possible cursors that the mouse can have (excluding inventory items) are defined here, as are the various ways in which these cursors are displayed.
	 */
	[System.Serializable]
	public class CursorManager : ScriptableObject
	{

		/** The rendering method of all cursors (Software, Hardware) */
		public CursorRendering cursorRendering = CursorRendering.Software;
		/** The rule that defines when the main cursor is shown (Always, Never, OnlyWhenPaused) */
		public CursorDisplay cursorDisplay = CursorDisplay.Always;
		/** If True, then the system's default hardware cursor will replaced with a custom one */
		public bool allowMainCursor = false;

		/** If True, then a separate cursor will display when in "walk mode" */
		public bool allowWalkCursor = false;
		/** If True, then a prefix can be added to the Hotspot label when in "walk mode" */
		public bool addWalkPrefix = false;
		/** The prefix to add to the Hotspot label when in "walk mode", if addWalkPrefix = True */
		public HotspotPrefix walkPrefix = new HotspotPrefix ("Walk to");

		/** If True, then the Cursor's interaction verb will prefix the Hotspot label when hovering over Hotspots */
		public bool addHotspotPrefix = false;
		/** If True, then the cursor will be controlled by the current Interaction when hovering over a Hotspot */
		public bool allowInteractionCursor = false;
		/** If True, then the cursor will be controlled by the current Interaction when hovering over an inventory item (see InvItem) */
		public bool allowInteractionCursorForInventory = false;
		/** If True, then cursor modes can by clicked by right-clicking, if interactionMethod = AC_InteractionMethod.ChooseInteractionThenHotspot in SettingsManager */
		public bool cycleCursors = false;
		/** If True, then left-clicking a Hotspot will examine it if no "use" Interaction exists (if interactionMethod = AC_InteractionMethod.ContextSensitive in SettingsManager) */
		public bool leftClickExamine = false;
		/** If True, and allowWalkCursor = True, then the walk cursor will only show when the cursor is hovering over a NavMesh */
		public bool onlyWalkWhenOverNavMesh = false;
		/** If True, then Hotspot labels will not show when an inventory item is selected unless the cursor is over another inventory item or a Hotspot */
		public bool onlyShowInventoryLabelOverHotspots = false;
		/** The size of selected inventory item graphics when used as a cursor */
		public float inventoryCursorSize = 0.06f;

		/** The cursor while the game is running a gameplay-blocking cutscene */
		public CursorIconBase waitIcon = new CursorIcon ();
		/** The cursor while the game is paused but Menus are interactive */
		public CursorIconBase pointerIcon = new CursorIcon ();
		/** The cursor when in "walk mode", if allowWalkCursor = True */
		public CursorIconBase walkIcon = new CursorIcon ();
		/** The cursor when hovering over a Hotspot */
		public CursorIconBase mouseOverIcon = new CursorIcon ();

		/** What happens to the cursor when an inventory item is selected (ChangeCursor, ChangeHotspotLabel, ChangeCursorAndHotspotLabel) */
		public InventoryHandling inventoryHandling = InventoryHandling.ChangeCursor;
		/** The "Use" in the syntax "Use item on Hotspot" */
		public HotspotPrefix hotspotPrefix1 = new HotspotPrefix ("Use");
		/** The "on" in the syntax "Use item on Hotspot" */
		public HotspotPrefix hotspotPrefix2 = new HotspotPrefix ("on");
		/** The "Give" in the syntax "Give item to NPC" */
		public HotspotPrefix hotspotPrefix3 = new HotspotPrefix ("Give");
		/** The "to" in the syntax "Give item to NPC" */
		public HotspotPrefix hotspotPrefix4 = new HotspotPrefix ("to");

		/** A List of all CursorIcon instances that represent the various Interaction types */
		public List<CursorIcon> cursorIcons = new List<CursorIcon>();
		/** A List of ActionListAsset files that get run when an unhandled Interaction is triggered */
		public List<ActionListAsset> unhandledCursorInteractions = new List<ActionListAsset>();

		/** What to display when hovering over a Hotspot that has both a Use and Examine Interaction (DisplayUseIcon, DisplayBothSideBySide) */
		public LookUseCursorAction lookUseCursorAction = LookUseCursorAction.DisplayBothSideBySide;
		/** The ID number of the CursorIcon (in cursorIcons) that represents the "Examine" Interaction */
		public int lookCursor_ID = 0;

		private SettingsManager settingsManager;
		
		
		#if UNITY_EDITOR
		
		private static GUIContent
			insertContent = new GUIContent("+", "Insert variable"),
			deleteContent = new GUIContent("-", "Delete variable");

		private static GUILayoutOption
			buttonWidth = GUILayout.MaxWidth (20f);


		/**
		 * Shows the GUI.
		 */
		public void ShowGUI ()
		{
			settingsManager = AdvGame.GetReferences().settingsManager;

			cursorRendering = (CursorRendering) EditorGUILayout.EnumPopup ("Cursor rendering:", cursorRendering);
			EditorGUILayout.Space ();
			
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Main cursor", EditorStyles.boldLabel);
			cursorDisplay = (CursorDisplay) EditorGUILayout.EnumPopup ("Display cursor:", cursorDisplay);
			if (cursorDisplay != CursorDisplay.Never)
			{
				allowMainCursor = EditorGUILayout.Toggle ("Replace mouse cursor?", allowMainCursor);
				if (allowMainCursor || (settingsManager && settingsManager.inputMethod == InputMethod.KeyboardOrController))
				{
					IconBaseGUI ("Main cursor:", pointerIcon);
				}
			}
			EditorGUILayout.EndVertical ();
			
			EditorGUILayout.Space ();
			
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Walk settings", EditorStyles.boldLabel);
			if (allowMainCursor)
			{
				allowWalkCursor = EditorGUILayout.Toggle ("Provide walk cursor?", allowWalkCursor);
				if (allowWalkCursor)
				{
					if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
					{
						EditorGUILayout.LabelField ("Input button:", "Icon_Walk");
					}
					IconBaseGUI ("Walk cursor:", walkIcon);
					onlyWalkWhenOverNavMesh = EditorGUILayout.ToggleLeft ("Only show 'Walk' Cursor when over NavMesh?", onlyWalkWhenOverNavMesh);
				}
			}
			addWalkPrefix = EditorGUILayout.Toggle ("Prefix cursor labels?", addWalkPrefix);
			if (addWalkPrefix)
			{
				walkPrefix.label = EditorGUILayout.TextField ("Walk prefix:", walkPrefix.label);
			}
			EditorGUILayout.EndVertical ();
			
			EditorGUILayout.Space ();

			EditorGUILayout.BeginVertical ("Button");
				EditorGUILayout.LabelField ("Hotspot settings", EditorStyles.boldLabel);
				addHotspotPrefix = EditorGUILayout.Toggle ("Prefix cursor labels?", addHotspotPrefix);
				IconBaseGUI ("Mouse-over cursor:", mouseOverIcon);
			EditorGUILayout.EndVertical ();

			EditorGUILayout.Space ();
			
			EditorGUILayout.BeginVertical ("Button");
				EditorGUILayout.LabelField ("Inventory cursor", EditorStyles.boldLabel);
				inventoryHandling = (InventoryHandling) EditorGUILayout.EnumPopup ("When inventory selected:", inventoryHandling);
				if (inventoryHandling != InventoryHandling.ChangeCursor)
				{
					onlyShowInventoryLabelOverHotspots = EditorGUILayout.ToggleLeft ("Only show label when over Hotspots and Inventory?", onlyShowInventoryLabelOverHotspots);
				}
				if (inventoryHandling != InventoryHandling.ChangeHotspotLabel)
				{
					inventoryCursorSize = EditorGUILayout.FloatField ("Inventory cursor size:", inventoryCursorSize);
				}
				EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Use syntax:", GUILayout.Width (100f));
					hotspotPrefix1.label = EditorGUILayout.TextField (hotspotPrefix1.label, GUILayout.MaxWidth (80f));
					EditorGUILayout.LabelField ("(item)", GUILayout.MaxWidth (40f));
					hotspotPrefix2.label = EditorGUILayout.TextField (hotspotPrefix2.label, GUILayout.MaxWidth (80f));
					EditorGUILayout.LabelField ("(hotspot)", GUILayout.MaxWidth (55f));
				EditorGUILayout.EndHorizontal ();
				if (AdvGame.GetReferences ().settingsManager && AdvGame.GetReferences ().settingsManager.CanGiveItems ())
				{
					EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField ("Give syntax:", GUILayout.Width (100f));
						hotspotPrefix3.label = EditorGUILayout.TextField (hotspotPrefix3.label, GUILayout.MaxWidth (80f));
						EditorGUILayout.LabelField ("(item)", GUILayout.MaxWidth (40f));
						hotspotPrefix4.label = EditorGUILayout.TextField (hotspotPrefix4.label, GUILayout.MaxWidth (80f));
						EditorGUILayout.LabelField ("(hotspot)", GUILayout.MaxWidth (55f));
					EditorGUILayout.EndHorizontal ();
				}
			EditorGUILayout.EndVertical ();
			
			EditorGUILayout.Space ();
			
			EditorGUILayout.BeginVertical ("Button");
				EditorGUILayout.LabelField ("Interaction icons", EditorStyles.boldLabel);
				
				if (settingsManager == null || settingsManager.interactionMethod != AC_InteractionMethod.ChooseHotspotThenInteraction)
				{
					allowInteractionCursor = EditorGUILayout.ToggleLeft ("Change cursor based on Interaction?", allowInteractionCursor);
					if (allowInteractionCursor && (settingsManager == null || settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive))
					{
						allowInteractionCursorForInventory = EditorGUILayout.ToggleLeft ("Change when over Inventory items too?", allowInteractionCursorForInventory);
					}
					if (settingsManager && settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
					{
						cycleCursors = EditorGUILayout.ToggleLeft ("Cycle Interactions with right-click?", cycleCursors);
					}
				}
				
				IconsGUI ();
			
				EditorGUILayout.Space ();
			
				if (settingsManager == null || settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
				{
					LookIconGUI ();
				}
			EditorGUILayout.EndVertical ();

			EditorGUILayout.Space ();

			EditorGUILayout.BeginVertical ("Button");
				IconBaseGUI ("Wait cursor", waitIcon);
			EditorGUILayout.EndVertical ();

			if (GUI.changed)
			{
				EditorUtility.SetDirty (this);
			}
		}
		
		
		private void IconsGUI ()
		{
			// Make sure unhandledCursorInteractions is the same length as cursorIcons
			while (unhandledCursorInteractions.Count < cursorIcons.Count)
			{
				unhandledCursorInteractions.Add (null);
			}
			while (unhandledCursorInteractions.Count > cursorIcons.Count)
			{
				unhandledCursorInteractions.RemoveAt (unhandledCursorInteractions.Count + 1);
			}

			// List icons
			foreach (CursorIcon _cursorIcon in cursorIcons)
			{
				int i = cursorIcons.IndexOf (_cursorIcon);
				GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Icon ID:", GUILayout.MaxWidth (145));
				EditorGUILayout.LabelField (_cursorIcon.id.ToString (), GUILayout.MaxWidth (120));

				if (GUILayout.Button (insertContent, EditorStyles.miniButtonLeft, buttonWidth))
				{
					Undo.RecordObject (this, "Add icon");
					cursorIcons.Insert (i+1, new CursorIcon (GetIDArray ()));
					unhandledCursorInteractions.Insert (i+1, null);
					break;
				}
				if (GUILayout.Button (deleteContent, EditorStyles.miniButtonRight, buttonWidth))
				{
					Undo.RecordObject (this, "Delete icon: " + _cursorIcon.label);
					cursorIcons.Remove (_cursorIcon);
					unhandledCursorInteractions.RemoveAt (i);
					break;
				}
				EditorGUILayout.EndHorizontal ();

				_cursorIcon.label = EditorGUILayout.TextField ("Label:", _cursorIcon.label);
				if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
				{
					EditorGUILayout.LabelField ("Input button:", _cursorIcon.GetButtonName ());
				}
				_cursorIcon.ShowGUI (true, cursorRendering);

				if (settingsManager && settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
				{
					unhandledCursorInteractions[i] = ActionListAssetMenu.AssetGUI ("Unhandled interaction", unhandledCursorInteractions[i]);
					_cursorIcon.dontCycle = EditorGUILayout.Toggle ("Leave out of Cursor cycle?", _cursorIcon.dontCycle);
				}
			}

			if (GUILayout.Button("Create new icon"))
			{
				Undo.RecordObject (this, "Add icon");
				cursorIcons.Add (new CursorIcon (GetIDArray ()));
			}
		}
		
		
		private void LookIconGUI ()
		{
			if (cursorIcons.Count > 0)
			{
				int lookCursor_int = GetIntFromID (lookCursor_ID);
				lookCursor_int = EditorGUILayout.Popup ("Examine icon:", lookCursor_int, GetLabelsArray ());
				lookCursor_ID = cursorIcons[lookCursor_int].id;

				if (cursorRendering == CursorRendering.Software)
				{
					EditorGUILayout.LabelField ("When Use and Examine interactions are both available:");
					lookUseCursorAction = (LookUseCursorAction) EditorGUILayout.EnumPopup (" ", lookUseCursorAction);
				}

				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Left-click to examine when no use interaction exists?", GUILayout.Width (300f));
				leftClickExamine = EditorGUILayout.Toggle (leftClickExamine);
				EditorGUILayout.EndHorizontal ();
			}
		}


		private void IconBaseGUI (string fieldLabel, CursorIconBase icon)
		{
			EditorGUILayout.LabelField (fieldLabel, EditorStyles.boldLabel);
			icon.ShowGUI (true, cursorRendering);
			GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
		}

		#endif
		

		/**
		 * <summary>Gets an array of the CursorIcon labels defined in cursorIcons.</summary>
		 * <returns>An array of the CursorIcon labels defined in cursorIcons</returns>
		 */
		public string[] GetLabelsArray ()
		{
			List<string> iconLabels = new List<string>();
			foreach (CursorIcon cursorIcon in cursorIcons)
			{
				iconLabels.Add (cursorIcon.label);
			}
			return (iconLabels.ToArray());
		}
		

		/**
		 * <summary>Gets a label of the CursorIcon defined in cursorIcons.</summary>
		 * <param name = "_ID">The ID number of the CursorIcon to find</param>
		 * <param name = "languageNumber">The index number of the language to get the label in</param>
		 * <returns>The label of the CursorIcon</returns>
		 */
		public string GetLabelFromID (int _ID, int languageNumber)
		{
			foreach (CursorIcon cursorIcon in cursorIcons)
			{
				if (cursorIcon.id == _ID)
				{
					if (Application.isPlaying)
					{
						return (KickStarter.runtimeLanguages.GetTranslation (cursorIcon.label, cursorIcon.lineID, languageNumber) + " ");
					}
					return cursorIcon.label;
				}
			}
			
			return ("");
		}
		

		/**
		 * <summary>Gets a CursorIcon defined in cursorIcons.</summary>
		 * <param name = "_ID">The ID number of the CursorIcon to find</param>
		 * <returns>The CursorIcon</returns>
		 */
		public CursorIcon GetCursorIconFromID (int _ID)
		{
			foreach (CursorIcon cursorIcon in cursorIcons)
			{
				if (cursorIcon.id == _ID)
				{
					return (cursorIcon);
				}
			}
			
			return (null);
		}
		

		/**
		 * <summary>Gets the index number (in cursorIcons) of a CursorIcon.</summary>
		 * <param name = "_ID">The ID number of the CursorIcon to find</param>
		 * <returns>The index number (in cursorIcons) of the CursorIcon</returns>
		 */
		public int GetIntFromID (int _ID)
		{
			int i = 0;
			int requestedInt = -1;
			
			foreach (CursorIcon cursorIcon in cursorIcons)
			{
				if (cursorIcon.id == _ID)
				{
					requestedInt = i;
				}
				
				i++;
			}
			
			if (requestedInt == -1)
			{
				// Wasn't found (icon was deleted?), so revert to zero
				requestedInt = 0;
			}
		
			return (requestedInt);
		}


		/**
		 * <summary>Gets the ActionListAsset that is used as a CursorIcon's unhandled event.</summary>
		 * <param name = "_ID">The ID number of the CursorIcon to find</param>
		 * <returns>The ActionListAsset that is used as the CursorIcon's unhandled event</returns>
		 */
		public ActionListAsset GetUnhandledInteraction (int _ID)
		{
			foreach (CursorIcon cursorIcon in cursorIcons)
			{
				if (cursorIcon.id == _ID)
				{
					int i = cursorIcons.IndexOf (cursorIcon);
					if (unhandledCursorInteractions.Count > i)
					{
						return unhandledCursorInteractions [i];
					}
					return null;
				}
			}
			return null;
		}
		
		
		private int[] GetIDArray ()
		{
			// Returns a list of id's in the list
			
			List<int> idArray = new List<int>();
			
			foreach (CursorIcon cursorIcon in cursorIcons)
			{
				idArray.Add (cursorIcon.id);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}

	}

}