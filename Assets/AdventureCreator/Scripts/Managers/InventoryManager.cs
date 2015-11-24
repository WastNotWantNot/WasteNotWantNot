/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionsManager.cs"
 * 
 *	This script handles the "Inventory" tab of the main wizard.
 *	Inventory items are defined with this.
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
	 * Handles the "Inventory" tab of the Game Editor window.
	 * All inventory items, inventory categories and recipes are defined here.
	 */
	[System.Serializable]
	public class InventoryManager : ScriptableObject
	{
		
		/** The game's full list of inventory items */
		public List<InvItem> items = new List<InvItem>();
		/** The game's full list of inventory item categories */
		public List<InvBin> bins = new List<InvBin>();
		/** The game's full list of inventory item properties */
		public List<InvVar> invVars = new List<InvVar>();
		/** The default ActionListAsset to run if an inventory combination is unhandled */
		public ActionListAsset unhandledCombine;
		/** The default ActionListAsset to run if using an inventory item on a Hotspot is unhandled */
		public ActionListAsset unhandledHotspot;
		/** The default ActionListAsset to run if giving an inventory item to an NPC is unhandled */
		public ActionListAsset unhandledGive;
		/** The game's full list of available recipes */
		public List<Recipe> recipes = new List<Recipe>();
		
		#if UNITY_EDITOR
		
		private Texture2D sideIcon;
		private SettingsManager settingsManager;
		private CursorManager cursorManager;
		
		private FilterInventoryItem filterType;
		private string nameFilter = "";
		private int categoryFilter = -1;
		private bool filterOnStart = false;
		
		private InvItem selectedItem;
		private InvVar selectedInvVar;
		private Recipe selectedRecipe;
		private int sideItem = -1;
		private int invNumber = 0;
		private int binNumber = -1;
		
		private Vector2 scrollPos;
		private bool showItems = true;
		private bool showBins = false;
		private bool showCrafting = false;
		private bool showProperties = false;
		
		private string[] boolType = {"False", "True"};
		
		private static GUILayoutOption
			buttonWidth = GUILayout.MaxWidth (20f);
		
		private static GUIContent
			deleteContent = new GUIContent("-", "Delete item");
		
		
		/**
		 * Shows the GUI.
		 */
		public void ShowGUI ()
		{
			if (!sideIcon)
			{
				sideIcon = (Texture2D) AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Graphics/Textures/inspector-use.png", typeof (Texture2D));
			}
			
			if (AdvGame.GetReferences ())
			{
				if (AdvGame.GetReferences ().settingsManager)
				{
					settingsManager = AdvGame.GetReferences ().settingsManager;
				}
				if (AdvGame.GetReferences ().cursorManager)
				{
					cursorManager = AdvGame.GetReferences ().cursorManager;
				}
			}
			
			EditorGUILayout.Space ();
			GUILayout.BeginHorizontal ();
			if (GUILayout.Toggle (showItems, "Items", "toolbarbutton"))
			{
				SetTab (0);
			}
			if (GUILayout.Toggle (showBins, "Categories", "toolbarbutton"))
			{
				SetTab (1);
			}
			if (GUILayout.Toggle (showCrafting, "Crafting", "toolbarbutton"))
			{
				SetTab (2);
			}
			if (GUILayout.Toggle (showProperties, "Properties", "toolbarbutton"))
			{
				SetTab (3);
			}
			GUILayout.EndHorizontal ();
			EditorGUILayout.Space ();
			
			if (showBins)
			{
				BinsGUI ();
			}
			else if (showCrafting)
			{
				CraftingGUI ();
			}
			else if (showItems)
			{
				ItemsGUI ();
			}
			else if (showProperties)
			{
				PropertiesGUI ();
			}
			
			if (GUI.changed)
			{
				EditorUtility.SetDirty (this);
			}
		}
		
		
		private void ItemsGUI ()
		{
			EditorGUILayout.LabelField ("Unhandled events", EditorStyles.boldLabel);
			unhandledCombine = ActionListAssetMenu.AssetGUI ("Combine:", unhandledCombine);
			unhandledHotspot = ActionListAssetMenu.AssetGUI ("Use on hotspot:", unhandledHotspot);
			if (settingsManager != null && settingsManager.CanGiveItems ())
			{
				unhandledGive = ActionListAssetMenu.AssetGUI ("Give to NPC:", unhandledGive);
			}
			
			List<string> binList = new List<string>();
			foreach (InvBin bin in bins)
			{
				binList.Add (bin.label);
			}
			
			EditorGUILayout.Space ();
			CreateItemsGUI (binList.ToArray ());
			EditorGUILayout.Space ();
			
			if (selectedItem != null && items.Contains (selectedItem))
			{
				EditorGUILayout.LabelField ("Inventory item '" + selectedItem.label + "' settings", EditorStyles.boldLabel);
				
				EditorGUILayout.BeginVertical("Button");
				selectedItem.label = EditorGUILayout.TextField ("Name:", selectedItem.label);
				selectedItem.altLabel = EditorGUILayout.TextField ("Label (if not name):", selectedItem.altLabel);
				
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Category:", GUILayout.Width (146f));
				if (bins.Count > 0)
				{
					binNumber = GetBinSlot (selectedItem.binID);
					binNumber = EditorGUILayout.Popup (binNumber, binList.ToArray());
					selectedItem.binID = bins[binNumber].id;
				}
				else
				{
					selectedItem.binID = -1;
					EditorGUILayout.LabelField ("No categories defined!", EditorStyles.miniLabel, GUILayout.Width (146f));
				}
				EditorGUILayout.EndHorizontal ();	

				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Main graphic:", GUILayout.Width (145));
				selectedItem.tex = (Texture2D) EditorGUILayout.ObjectField (selectedItem.tex, typeof (Texture2D), false, GUILayout.Width (70), GUILayout.Height (70));
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Active graphic:", GUILayout.Width (145));
				selectedItem.activeTex = (Texture2D) EditorGUILayout.ObjectField (selectedItem.activeTex, typeof (Texture2D), false, GUILayout.Width (70), GUILayout.Height (70));
				EditorGUILayout.EndHorizontal ();

				if (AdvGame.GetReferences ().settingsManager != null && AdvGame.GetReferences ().settingsManager.selectInventoryDisplay == SelectInventoryDisplay.ShowSelectedGraphic)
				{
					selectedItem.selectedTex = (Texture2D) EditorGUILayout.ObjectField ("Selected graphic:", selectedItem.selectedTex, typeof (Texture2D), false);
				}
				if (AdvGame.GetReferences ().cursorManager != null)
				{
					CursorManager cursorManager = AdvGame.GetReferences ().cursorManager;
					if (cursorManager.inventoryHandling == InventoryHandling.ChangeCursor || cursorManager.inventoryHandling == InventoryHandling.ChangeCursorAndHotspotLabel)
					{
						GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
						selectedItem.cursorIcon.ShowGUI (true, cursorManager.cursorRendering, "Cursor (optional):");
						GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
					}
				}

				selectedItem.carryOnStart = EditorGUILayout.Toggle ("Carry on start?", selectedItem.carryOnStart);
				if (selectedItem.carryOnStart && AdvGame.GetReferences ().settingsManager && AdvGame.GetReferences ().settingsManager.playerSwitching == PlayerSwitching.Allow && !AdvGame.GetReferences ().settingsManager.shareInventory)
				{
					selectedItem.carryOnStartNotDefault = EditorGUILayout.Toggle ("Give to non-default player?", selectedItem.carryOnStartNotDefault);
					if (selectedItem.carryOnStartNotDefault)
					{
						selectedItem.carryOnStartID = ChoosePlayerGUI (selectedItem.carryOnStartID);
					}
				}
				
				selectedItem.canCarryMultiple = EditorGUILayout.Toggle ("Can carry multiple?", selectedItem.canCarryMultiple);
				if (selectedItem.canCarryMultiple)
				{
					selectedItem.useSeparateSlots = EditorGUILayout.Toggle ("Place in separate slots?", selectedItem.useSeparateSlots);
				}
				if (selectedItem.carryOnStart && selectedItem.canCarryMultiple)
				{
					selectedItem.count = EditorGUILayout.IntField ("Quantity on start:", selectedItem.count);
				}
				else
				{
					selectedItem.count = 1;
				}
				
				selectedItem.overrideUseSyntax = EditorGUILayout.Toggle ("Override 'Use' syntax?", selectedItem.overrideUseSyntax);
				if (selectedItem.overrideUseSyntax)
				{
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Use syntax:", GUILayout.Width (100f));
					selectedItem.hotspotPrefix1.label = EditorGUILayout.TextField (selectedItem.hotspotPrefix1.label, GUILayout.MaxWidth (80f));
					EditorGUILayout.LabelField ("(item)", GUILayout.MaxWidth (40f));
					selectedItem.hotspotPrefix2.label = EditorGUILayout.TextField (selectedItem.hotspotPrefix2.label, GUILayout.MaxWidth (80f));
					EditorGUILayout.LabelField ("(hotspot)", GUILayout.MaxWidth (55f));
					EditorGUILayout.EndHorizontal ();
				}
				
				EditorGUILayout.Space ();
				EditorGUILayout.LabelField ("Standard interactions", EditorStyles.boldLabel);
				if (settingsManager && settingsManager.interactionMethod != AC_InteractionMethod.ContextSensitive && settingsManager.inventoryInteractions == InventoryInteractions.Multiple && AdvGame.GetReferences ().cursorManager)
				{
					CursorManager cursorManager = AdvGame.GetReferences ().cursorManager;
					
					List<string> iconList = new List<string>();
					foreach (CursorIcon icon in cursorManager.cursorIcons)
					{
						iconList.Add (icon.label);
					}
					
					if (cursorManager.cursorIcons.Count > 0)
					{
						foreach (InvInteraction interaction in selectedItem.interactions)
						{
							EditorGUILayout.BeginHorizontal ();
							invNumber = GetIconSlot (interaction.icon.id);
							invNumber = EditorGUILayout.Popup (invNumber, iconList.ToArray());
							interaction.icon = cursorManager.cursorIcons[invNumber];
							
							interaction.actionList = ActionListAssetMenu.AssetGUI ("", interaction.actionList);
							
							if (GUILayout.Button (deleteContent, EditorStyles.miniButtonRight, buttonWidth))
							{
								Undo.RecordObject (this, "Delete interaction");
								selectedItem.interactions.Remove (interaction);
								break;
							}
							EditorGUILayout.EndHorizontal ();
						}
					}
					else
					{
						EditorGUILayout.HelpBox ("No interaction icons defined - please use the Cursor Manager", MessageType.Warning);
					}
					if (GUILayout.Button ("Add interaction"))
					{
						Undo.RecordObject (this, "Add new interaction");
						selectedItem.interactions.Add (new InvInteraction (cursorManager.cursorIcons[0]));
					}
				}
				else
				{
					selectedItem.useActionList = ActionListAssetMenu.AssetGUI ("Use:", selectedItem.useActionList);
					if (cursorManager && cursorManager.allowInteractionCursorForInventory && cursorManager.cursorIcons.Count > 0)
					{
						int useCursor_int = cursorManager.GetIntFromID (selectedItem.useIconID);
						useCursor_int = EditorGUILayout.Popup ("Use cursor icon:", useCursor_int, cursorManager.GetLabelsArray ());
						selectedItem.useIconID = cursorManager.cursorIcons[useCursor_int].id;
					}
					else
					{
						selectedItem.useIconID = 0;
					}
					selectedItem.lookActionList = ActionListAssetMenu.AssetGUI ("Examine:", selectedItem.lookActionList);
				}
				
				if (settingsManager.CanSelectItems (false))
				{
					EditorGUILayout.Space ();
					EditorGUILayout.LabelField ("Unhandled interactions", EditorStyles.boldLabel);
					selectedItem.unhandledActionList = ActionListAssetMenu.AssetGUI ("Unhandled use on Hotspot:", selectedItem.unhandledActionList);
					selectedItem.unhandledCombineActionList = ActionListAssetMenu.AssetGUI("Unhandled combine:", selectedItem.unhandledCombineActionList);
				}
				
				EditorGUILayout.Space ();
				EditorGUILayout.LabelField ("Combine interactions", EditorStyles.boldLabel);
				for (int i=0; i<selectedItem.combineActionList.Count; i++)
				{
					EditorGUILayout.BeginHorizontal ();
					invNumber = GetArraySlot (selectedItem.combineID[i]);
					invNumber = EditorGUILayout.Popup (invNumber, GetLabelList ());
					selectedItem.combineID[i] = items[invNumber].id;
					
					selectedItem.combineActionList[i] = ActionListAssetMenu.AssetGUI ("", selectedItem.combineActionList[i]);
					
					if (GUILayout.Button (deleteContent, EditorStyles.miniButtonRight, buttonWidth))
					{
						Undo.RecordObject (this, "Delete combine event");
						selectedItem.combineActionList.RemoveAt (i);
						selectedItem.combineID.RemoveAt (i);
						break;
					}
					EditorGUILayout.EndHorizontal ();
				}
				if (GUILayout.Button ("Add combine event"))
				{
					Undo.RecordObject (this, "Add new combine event");
					selectedItem.combineActionList.Add (null);
					selectedItem.combineID.Add (0);
				}
				
				// List all "reverse" inventory combinations
				string reverseCombinations = "";
				foreach (InvItem otherItem in items)
				{
					if (otherItem != selectedItem)
					{
						if (otherItem.combineID.Contains (selectedItem.id))
						{
							reverseCombinations += "- " + otherItem.label + "\n";
							continue;
						}
					}
				}
				if (reverseCombinations.Length > 0)
				{
					EditorGUILayout.Space ();
					EditorGUILayout.HelpBox ("The following inventory items have combine interactions that reference this item:\n" + reverseCombinations, MessageType.Info);
				}
				
				if (invVars.Count > 0)
				{
					EditorGUILayout.Space ();
					EditorGUILayout.LabelField ("Properties", EditorStyles.boldLabel);
					
					RebuildProperties (selectedItem);
					
					// UI for setting property values
					if (selectedItem.vars.Count > 0)
					{
						foreach (InvVar invVar in selectedItem.vars)
						{
							string label = invVar.label + ":";
							if (invVar.label.Length == 0)
							{
								label = "Property " + invVar.id.ToString () + ":";
							}
							
							if (invVar.type == VariableType.Boolean)
							{
								if (invVar.val != 1)
								{
									invVar.val = 0;
								}
								invVar.val = EditorGUILayout.Popup (label, invVar.val, boolType);
							}
							else if (invVar.type == VariableType.Integer)
							{
								invVar.val = EditorGUILayout.IntField (label, invVar.val);
							}
							else if (invVar.type == VariableType.PopUp)
							{
								invVar.val = EditorGUILayout.Popup (label, invVar.val, invVar.popUps);
							}
							else if (invVar.type == VariableType.String)
							{
								invVar.textVal = EditorGUILayout.TextField (label, invVar.textVal);
							}
							else if (invVar.type == VariableType.Float)
							{
								invVar.floatVal = EditorGUILayout.FloatField (label, invVar.floatVal);
							}
						}
					}
					else
					{
						EditorGUILayout.HelpBox ("No properties have been defined that this inventory item can use.", MessageType.Info);
					}
				}
				
				EditorGUILayout.EndVertical();
			}
		}
		
		
		private void BinsGUI ()
		{
			EditorGUILayout.LabelField ("Categories", EditorStyles.boldLabel);
			
			foreach (InvBin bin in bins)
			{
				EditorGUILayout.BeginHorizontal ();
				bin.label = EditorGUILayout.TextField (bin.label);
				
				if (GUILayout.Button (deleteContent, EditorStyles.miniButton, GUILayout.MaxWidth(20f)))
				{
					Undo.RecordObject (this, "Delete category: " + bin.label);
					bins.Remove (bin);
					break;
				}
				EditorGUILayout.EndHorizontal ();
				
			}
			if (GUILayout.Button ("Create new category"))
			{
				Undo.RecordObject (this, "Add category");
				List<int> idArray = new List<int>();
				foreach (InvBin bin in bins)
				{
					idArray.Add (bin.id);
				}
				idArray.Sort ();
				bins.Add (new InvBin (idArray.ToArray ()));
			}
		}
		
		
		private void PropertiesGUI ()
		{
			List<string> binList = new List<string>();
			foreach (InvBin bin in bins)
			{
				binList.Add (bin.label);
			}
			
			EditorGUILayout.Space ();
			CreatePropertiesGUI ();
			EditorGUILayout.Space ();
			
			if (selectedInvVar != null && invVars.Contains (selectedInvVar))
			{
				EditorGUILayout.LabelField ("Inventory property '" + selectedInvVar.label + "' properties", EditorStyles.boldLabel);
				
				EditorGUILayout.BeginVertical("Button");
				selectedInvVar.label = EditorGUILayout.TextField ("Name:", selectedInvVar.label);
				selectedInvVar.type = (VariableType) EditorGUILayout.EnumPopup ("Type:", selectedInvVar.type);
				if (selectedInvVar.type == VariableType.PopUp)
				{
					selectedInvVar.popUps = VariablesManager.PopupsGUI (selectedInvVar.popUps);
				}
				
				selectedInvVar.limitToCategories = EditorGUILayout.BeginToggleGroup ("Limit to set categories?", selectedInvVar.limitToCategories);

				if (bins.Count > 0)
				{
					List<int> newCategoryIDs = new List<int>();
					foreach (InvBin bin in bins)
					{
						bool usesCategory = false;
						if (selectedInvVar.categoryIDs.Contains (bin.id))
						{
							usesCategory = true;
						}
						usesCategory = EditorGUILayout.Toggle ("Use in '" + bin.label + "'?", usesCategory);
						
						if (usesCategory)
						{
							newCategoryIDs.Add (bin.id);
						}
					}
					selectedInvVar.categoryIDs = newCategoryIDs;
				}
				else if (selectedInvVar.limitToCategories)
				{
					EditorGUILayout.HelpBox ("No categories are defined!", MessageType.Warning);
				}
				EditorGUILayout.EndToggleGroup ();
				
				EditorGUILayout.EndVertical ();
			}
			
			if (GUI.changed)
			{
				foreach (InvItem item in items)
				{
					RebuildProperties (item);
				}
			}
		}
		
		
		private void RebuildProperties (InvItem item)
		{
			// Which properties are available?
			List<int> availableVarIDs = new List<int>();
			foreach (InvVar invVar in invVars)
			{
				if (!invVar.limitToCategories || bins.Count == 0 || invVar.categoryIDs.Contains (item.binID))
				{
					availableVarIDs.Add (invVar.id);
				}
			}
			
			// Create new properties / transfer existing values
			List<InvVar> newInvVars = new List<InvVar>();
			foreach (InvVar invVar in invVars)
			{
				if (availableVarIDs.Contains (invVar.id))
				{
					InvVar newInvVar = new InvVar (invVar);
					InvVar oldInvVar = item.GetProperty (invVar.id);
					if (oldInvVar != null)
					{
						newInvVar.TransferValues (oldInvVar);
					}
					newInvVars.Add (newInvVar);
				}
			}
			
			item.vars = newInvVars;
		}
		
		
		private void ResetFilter ()
		{
			nameFilter = "";
			categoryFilter = -1;
			filterOnStart = false;
		}
		
		
		private void CreateItemsGUI (string[] binList)
		{
			EditorGUILayout.LabelField ("Inventory items", EditorStyles.boldLabel);
			
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Filter by:", GUILayout.Width (100f));
			filterType = (FilterInventoryItem) EditorGUILayout.EnumPopup (filterType, GUILayout.Width (80f));
			if (filterType == FilterInventoryItem.Name)
			{
				nameFilter = EditorGUILayout.TextField (nameFilter);
			}
			else if (filterType == FilterInventoryItem.Category)
			{
				if (bins == null || bins.Count == 0)
				{
					categoryFilter = -1;
					EditorGUILayout.HelpBox ("No categories defined!", MessageType.Info);
				}
				else
				{
					categoryFilter = EditorGUILayout.Popup (categoryFilter, binList);
				}
			}
			EditorGUILayout.EndHorizontal ();
			filterOnStart = EditorGUILayout.Toggle ("Filter by 'Carry on start?'?", filterOnStart);
			
			EditorGUILayout.Space ();
			
			scrollPos = EditorGUILayout.BeginScrollView (scrollPos, GUILayout.Height (Mathf.Min (items.Count * 21, 235f)+5));
			foreach (InvItem item in items)
			{
				if ((filterType == FilterInventoryItem.Name && (nameFilter == "" || item.label.ToLower ().Contains (nameFilter.ToLower ()))) ||
				    (filterType == FilterInventoryItem.Category && (categoryFilter == -1 || GetBinSlot (item.binID) == categoryFilter)))
				{
					if (!filterOnStart || item.carryOnStart)
					{
						EditorGUILayout.BeginHorizontal ();
						
						string buttonLabel = item.label;
						if (buttonLabel == "")
						{
							buttonLabel = "(Untitled)";	
						}
						
						if (GUILayout.Toggle (item.isEditing, item.id + ": " + buttonLabel, "Button"))
						{
							if (selectedItem != item)
							{
								DeactivateAllItems ();
								ActivateItem (item);
							}
						}
						
						if (GUILayout.Button (sideIcon, GUILayout.Width (20f), GUILayout.Height (15f)))
						{
							SideMenu (item);
						}
						
						EditorGUILayout.EndHorizontal ();
					}
				}
			}
			EditorGUILayout.EndScrollView ();
			
			if (GUILayout.Button("Create new item"))
			{
				Undo.RecordObject (this, "Create inventory item");
				
				ResetFilter ();
				InvItem newItem = new InvItem (GetIDArray ());
				items.Add (newItem);
				DeactivateAllItems ();
				ActivateItem (newItem);
			}
		}
		
		
		private void CreatePropertiesGUI ()
		{
			EditorGUILayout.LabelField ("Inventory properties", EditorStyles.boldLabel);
			
			EditorGUILayout.Space ();
			
			scrollPos = EditorGUILayout.BeginScrollView (scrollPos, GUILayout.Height (Mathf.Min (invVars.Count * 21, 235f)+5));
			foreach (InvVar invVar in invVars)
			{
				EditorGUILayout.BeginHorizontal ();
				
				string buttonLabel = invVar.label;
				if (buttonLabel == "")
				{
					buttonLabel = "(Untitled)";	
				}
				
				if (GUILayout.Toggle (invVar.isEditing, invVar.id + ": " + buttonLabel, "Button"))
				{
					if (selectedInvVar != invVar)
					{
						DeactivateAllInvVars ();
						ActivateItem (invVar);
					}
				}
				
				if (GUILayout.Button (sideIcon, GUILayout.Width (20f), GUILayout.Height (15f)))
				{
					SideMenu (invVar);
				}
				
				EditorGUILayout.EndHorizontal ();
			}
			EditorGUILayout.EndScrollView ();
			
			if (GUILayout.Button("Create new property"))
			{
				Undo.RecordObject (this, "Create inventory property");
				
				InvVar newInvVar = new InvVar (GetIDArrayProperty ());
				invVars.Add (newInvVar);
				DeactivateAllInvVars ();
				ActivateItem (newInvVar);
			}
		}
		
		
		private void ActivateItem (InvItem item)
		{
			item.isEditing = true;
			selectedItem = item;
		}
		
		
		private void ActivateItem (InvVar invVar)
		{
			invVar.isEditing = true;
			selectedInvVar = invVar;
		}
		
		
		private void DeactivateAllItems ()
		{
			foreach (InvItem item in items)
			{
				item.isEditing = false;
			}
			selectedItem = null;
		}
		
		
		private void DeactivateAllInvVars ()
		{
			foreach (InvVar invVar in invVars)
			{
				invVar.isEditing = false;
			}
			selectedInvVar = null;
		}
		
		
		private void ActivateRecipe (Recipe recipe)
		{
			recipe.isEditing = true;
			selectedRecipe = recipe;
		}
		
		
		private void DeactivateAllRecipes ()
		{
			foreach (Recipe recipe in recipes)
			{
				recipe.isEditing = false;
			}
			selectedRecipe = null;
		}
		
		
		private void SideMenu (InvItem item)
		{
			GenericMenu menu = new GenericMenu ();
			sideItem = items.IndexOf (item);
			
			menu.AddItem (new GUIContent ("Insert after"), false, Callback, "Insert after");
			if (items.Count > 0)
			{
				menu.AddItem (new GUIContent ("Delete"), false, Callback, "Delete");
			}
			if (sideItem > 0 || sideItem < items.Count-1)
			{
				menu.AddSeparator ("");
			}
			if (sideItem > 0)
			{
				menu.AddItem (new GUIContent ("Move up"), false, Callback, "Move up");
			}
			if (sideItem < items.Count-1)
			{
				menu.AddItem (new GUIContent ("Move down"), false, Callback, "Move down");
			}
			
			menu.ShowAsContext ();
		}
		
		
		private void SideMenu (InvVar invVar)
		{
			GenericMenu menu = new GenericMenu ();
			sideItem = invVars.IndexOf (invVar);
			
			menu.AddItem (new GUIContent ("Insert after"), false, PropertyCallback, "Insert after");
			if (invVars.Count > 0)
			{
				menu.AddItem (new GUIContent ("Delete"), false, PropertyCallback, "Delete");
			}
			if (sideItem > 0 || sideItem < invVars.Count-1)
			{
				menu.AddSeparator ("");
			}
			if (sideItem > 0)
			{
				menu.AddItem (new GUIContent ("Move up"), false, PropertyCallback, "Move up");
			}
			if (sideItem < invVars.Count-1)
			{
				menu.AddItem (new GUIContent ("Move down"), false, PropertyCallback, "Move down");
			}
			
			menu.ShowAsContext ();
		}
		
		
		private void Callback (object obj)
		{
			if (sideItem >= 0)
			{
				ResetFilter ();
				InvItem tempItem = items[sideItem];
				
				switch (obj.ToString ())
				{
				case "Insert after":
					Undo.RecordObject (this, "Insert item");
					items.Insert (sideItem+1, new InvItem (GetIDArray ()));
					break;
					
				case "Delete":
					Undo.RecordObject (this, "Delete item");
					DeactivateAllItems ();
					items.RemoveAt (sideItem);
					break;
					
				case "Move up":
					Undo.RecordObject (this, "Move item up");
					items.RemoveAt (sideItem);
					items.Insert (sideItem-1, tempItem);
					break;
					
				case "Move down":
					Undo.RecordObject (this, "Move item down");
					items.RemoveAt (sideItem);
					items.Insert (sideItem+1, tempItem);
					break;
				}
			}
			
			EditorUtility.SetDirty (this);
			AssetDatabase.SaveAssets ();
			
			sideItem = -1;
		}
		
		
		private void PropertyCallback (object obj)
		{
			if (sideItem >= 0)
			{
				ResetFilter ();
				InvVar tempVar = invVars[sideItem];
				
				switch (obj.ToString ())
				{
				case "Insert after":
					Undo.RecordObject (this, "Insert item");
					invVars.Insert (sideItem+1, new InvVar (GetIDArrayProperty ()));
					break;
					
				case "Delete":
					Undo.RecordObject (this, "Delete item");
					DeactivateAllInvVars ();
					invVars.RemoveAt (sideItem);
					break;
					
				case "Move up":
					Undo.RecordObject (this, "Move item up");
					invVars.RemoveAt (sideItem);
					invVars.Insert (sideItem-1, tempVar);
					break;
					
				case "Move down":
					Undo.RecordObject (this, "Move item down");
					invVars.RemoveAt (sideItem);
					invVars.Insert (sideItem+1, tempVar);
					break;
				}
			}
			
			EditorUtility.SetDirty (this);
			AssetDatabase.SaveAssets ();
			
			sideItem = -1;
		}
		
		
		private void CraftingGUI ()
		{
			EditorGUILayout.LabelField ("Crafting", EditorStyles.boldLabel);
			
			if (items.Count == 0)
			{
				EditorGUILayout.HelpBox ("No inventory items defined!", MessageType.Info);
				return;
			}
			
			foreach (Recipe recipe in recipes)
			{
				EditorGUILayout.BeginHorizontal ();
				
				string buttonLabel = recipe.label;
				if (buttonLabel == "")
				{
					buttonLabel = "(Untitled)";	
				}
				
				if (GUILayout.Toggle (recipe.isEditing, recipe.id + ": " + buttonLabel, "Button"))
				{
					if (selectedRecipe != recipe)
					{
						DeactivateAllRecipes ();
						ActivateRecipe (recipe);
					}
				}
				
				if (GUILayout.Button ("-", GUILayout.Width (20f), GUILayout.Height (15f)))
				{
					Undo.RecordObject (this, "Delete recipe");
					DeactivateAllRecipes ();
					recipes.Remove (recipe);
					AssetDatabase.SaveAssets();
					break;
				}
				
				EditorGUILayout.EndHorizontal ();
			}
			
			if (GUILayout.Button("Create new recipe"))
			{
				Undo.RecordObject (this, "Create inventory recipe");
				
				Recipe newRecipe = new Recipe (GetIDArrayRecipe ());
				recipes.Add (newRecipe);
				DeactivateAllRecipes ();
				ActivateRecipe (newRecipe);
			}
			
			if (selectedRecipe != null && recipes.Contains (selectedRecipe))
			{
				EditorGUILayout.Space ();
				EditorGUILayout.LabelField ("Recipe '" + selectedRecipe.label + "' properties", EditorStyles.boldLabel);
				
				EditorGUILayout.BeginVertical("Button");
				selectedRecipe.label = EditorGUILayout.TextField ("Name:", selectedRecipe.label);
				
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Resulting item:", GUILayout.Width (146f));
				int i = GetArraySlot (selectedRecipe.resultID);
				i = EditorGUILayout.Popup (i, GetLabelList ());
				selectedRecipe.resultID = items[i].id;
				EditorGUILayout.EndHorizontal ();
				
				selectedRecipe.autoCreate = EditorGUILayout.Toggle ("Result is automatic?", selectedRecipe.autoCreate);
				selectedRecipe.useSpecificSlots = EditorGUILayout.Toggle ("Requires specific pattern?", selectedRecipe.useSpecificSlots);
				selectedRecipe.actionListOnCreate = ActionListAssetMenu.AssetGUI ("ActionList when create:", selectedRecipe.actionListOnCreate);
				
				selectedRecipe.onCreateRecipe = (OnCreateRecipe) EditorGUILayout.EnumPopup ("When click on result:", selectedRecipe.onCreateRecipe);
				if (selectedRecipe.onCreateRecipe == OnCreateRecipe.RunActionList)
				{
					selectedRecipe.invActionList = ActionListAssetMenu.AssetGUI ("ActionList when click:", selectedRecipe.invActionList);
				}
				
				EditorGUILayout.Space ();
				EditorGUILayout.LabelField ("Ingredients", EditorStyles.boldLabel);
				
				foreach (Ingredient ingredient in selectedRecipe.ingredients)
				{
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Ingredient:", GUILayout.Width (70f));
					i = GetArraySlot (ingredient.itemID);
					i = EditorGUILayout.Popup (i, GetLabelList ());
					ingredient.itemID = items[i].id;
					
					if (items[i].canCarryMultiple)
					{
						EditorGUILayout.LabelField ("Amount:", GUILayout.Width (50f));
						ingredient.amount = EditorGUILayout.IntField (ingredient.amount, GUILayout.Width (30f));
					}
					
					if (selectedRecipe.useSpecificSlots)
					{
						EditorGUILayout.LabelField ("Slot:", GUILayout.Width (30f));
						ingredient.slotNumber = EditorGUILayout.IntField (ingredient.slotNumber, GUILayout.Width (30f));
					}
					
					if (GUILayout.Button ("-", GUILayout.Width (20f), GUILayout.Height (15f)))
					{
						Undo.RecordObject (this, "Delete ingredient");
						selectedRecipe.ingredients.Remove (ingredient);
						AssetDatabase.SaveAssets();
						break;
					}
					
					EditorGUILayout.EndHorizontal ();
				}
				
				if (GUILayout.Button("Add new ingredient"))
				{
					Undo.RecordObject (this, "Add recipe ingredient");
					
					Ingredient newIngredient = new Ingredient ();
					selectedRecipe.ingredients.Add (newIngredient);
				}
				
				EditorGUILayout.EndVertical ();
			}
		}
		
		
		private int[] GetIDArray ()
		{
			List<int> idArray = new List<int>();
			foreach (InvItem item in items)
			{
				idArray.Add (item.id);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}
		
		
		private int[] GetIDArrayProperty ()
		{
			List<int> idArray = new List<int>();
			foreach (InvVar invVar in invVars)
			{
				idArray.Add (invVar.id);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}
		
		
		private int[] GetIDArrayRecipe ()
		{
			List<int> idArray = new List<int>();
			foreach (Recipe recipe in recipes)
			{
				idArray.Add (recipe.id);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}
		
		
		private int GetIconSlot (int _id)
		{
			int i = 0;
			foreach (CursorIcon icon in AdvGame.GetReferences ().cursorManager.cursorIcons)
			{
				if (icon.id == _id)
				{
					return i;
				}
				i++;
			}
			
			return 0;
		}
		
		
		private int GetArraySlot (int _id)
		{
			int i = 0;
			foreach (InvItem item in items)
			{
				if (item.id == _id)
				{
					return i;
				}
				i++;
			}
			
			return 0;
		}
		
		
		private string[] GetLabelList ()
		{
			List<string> labelList = new List<string>();
			foreach (InvItem _item in items)
			{
				labelList.Add (_item.label);
			}
			return labelList.ToArray ();
		}
		
		
		private int GetBinSlot (int _id)
		{
			int i = 0;
			foreach (InvBin bin in bins)
			{
				if (bin.id == _id)
				{
					return i;
				}
				i++;
			}
			
			return 0;
		}
		
		
		private int ChoosePlayerGUI (int playerID)
		{
			List<string> labelList = new List<string>();
			int i = 0;
			int playerNumber = -1;
			
			if (AdvGame.GetReferences ().settingsManager.players.Count > 0)
			{
				foreach (PlayerPrefab playerPrefab in settingsManager.players)
				{
					if (playerPrefab.playerOb != null)
					{
						labelList.Add (playerPrefab.playerOb.name);
					}
					else
					{
						labelList.Add ("(Undefined prefab)");
					}
					
					// If a player has been removed, make sure selected player is still valid
					if (playerPrefab.ID == playerID)
					{
						playerNumber = i;
					}
					
					i++;
				}
				
				if (playerNumber == -1)
				{
					// Wasn't found (item was possibly deleted), so revert to zero
					ACDebug.LogWarning ("Previously chosen Player no longer exists!");
					
					playerNumber = 0;
					playerID = 0;
				}
				
				playerNumber = EditorGUILayout.Popup ("Item is carried by:", playerNumber, labelList.ToArray());
				playerID = settingsManager.players[playerNumber].ID;
			}
			return playerID;
		}
		
		
		private void SetTab (int tab)
		{
			showItems = (tab == 0) ? true : false;
			showBins = (tab == 1) ? true : false;
			showCrafting = (tab == 2) ? true : false;
			showProperties = (tab == 3) ? true : false;
		}
		
		#endif
		
		
		/**
		 * <summary>Gets an inventory item's label.</summary>
		 * <param name = "_id">The ID number of the InvItem to find</param>
		 * <returns>The inventory item's label</returns>
		 */
		public string GetLabel (int _id)
		{
			string result = "";
			foreach (InvItem item in items)
			{
				if (item.id == _id)
				{
					result = item.label;
				}
			}
			
			return result;
		}
		
		
		/**
		 * <summary>Gets an inventory item.</summary>
		 * <param name = "_id">The ID number of the InvItem to find</param>
		 * <returns>The inventory item</returns>
		 */
		public InvItem GetItem (int _id)
		{
			foreach (InvItem item in items)
			{
				if (item.id == _id)
				{
					return item;
				}
			}
			return null;
		}
		
		
		/**
		 * <summary>Checks if multiple instances of an inventory item can exist.</summary>
		 * <param name = "_id">The ID number of the InvItem to find</param>
		 * <returns>True if multiple instances of the inventory item can exist</returns>
		 */
		public bool CanCarryMultiple (int _id)
		{
			foreach (InvItem item in items)
			{
				if (item.id == _id)
				{
					return item.canCarryMultiple;
				}
			}
			
			return false;
		}
		
	}
	
}