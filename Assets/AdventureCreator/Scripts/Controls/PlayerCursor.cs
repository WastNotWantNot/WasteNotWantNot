/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"PlayerCursor.cs"
 * 
 *	This script displays a cursor graphic on the screen.
 *	PlayerInput decides if this should be at the mouse position,
 *	or a position based on controller input.
 *	The cursor graphic changes based on what hotspot is underneath it.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	/**
	 * This script displays the cursor on screen.
	 * The available cursors are defined in CursorManager.
	 * It should be placed on the GameEngine prefab.
	 */
	public class PlayerCursor : MonoBehaviour
	{
		
		private int selectedCursor = -1; // -2 = inventory, -1 = pointer, 0+ = cursor array
		private bool showCursor = false;
		private bool canShowHardwareCursor = false;
		private float pulse = 0f;
		private int pulseDirection = 0; // 0 = none, 1 = in, -1 = out
		
		// Animation variables
		private CursorIconBase activeIcon = null;
		private CursorIconBase activeLookIcon = null;
		private string lastCursorName;
		

		private void Awake ()
		{
			if (AdvGame.GetReferences () == null)
			{
				ACDebug.LogError ("A References file is required - please use the Adventure Creator window to create one.");
			}
		}
		

		/**
		 * Updates the cursor. This is called every frame by StateHandler.
		 */
		public void UpdateCursor ()
		{
			if (KickStarter.cursorManager.cursorRendering == CursorRendering.Software)
			{
				bool shouldShowCursor = false;

				if (!canShowHardwareCursor)
				{
					shouldShowCursor = false;
				}
				else if (KickStarter.playerInput.GetDragState () == DragState.Moveable)
				{
					shouldShowCursor = false;
				}
				else if (KickStarter.settingsManager && KickStarter.cursorManager && (!KickStarter.cursorManager.allowMainCursor || KickStarter.cursorManager.pointerIcon.texture == null) && (KickStarter.runtimeInventory.selectedItem == null || KickStarter.cursorManager.inventoryHandling == InventoryHandling.ChangeHotspotLabel) && KickStarter.settingsManager.inputMethod == InputMethod.MouseAndKeyboard && KickStarter.stateHandler.gameState != GameState.Cutscene)
				{
					shouldShowCursor = true;
				}
				else if (KickStarter.cursorManager == null)
				{
					shouldShowCursor = true;
				}
				else
				{
					shouldShowCursor = false;
				}

				#if UNITY_5
				Cursor.visible = shouldShowCursor;
				#else
				Screen.showCursor = shouldShowCursor;
				#endif
			}
			
			if (KickStarter.settingsManager && KickStarter.stateHandler)
			{
				if (KickStarter.stateHandler.gameState == GameState.Cutscene)
				{
					if (KickStarter.cursorManager.waitIcon.texture != null)
					{
						showCursor = true;
					}
					else
					{
						showCursor = false;
					}
				}
				else if (KickStarter.stateHandler.gameState != GameState.Normal && KickStarter.settingsManager.inputMethod == InputMethod.KeyboardOrController)
				{
					showCursor = false;
				}
				else if (KickStarter.cursorManager)
				{
					if (KickStarter.stateHandler.gameState == GameState.Paused && (KickStarter.cursorManager.cursorDisplay == CursorDisplay.OnlyWhenPaused || KickStarter.cursorManager.cursorDisplay == CursorDisplay.Always))
					{
						showCursor = true;
					}
					else if (KickStarter.playerInput.GetDragState () == DragState.Moveable)
					{
						showCursor = false;
					}
					//else if ((KickStarter.stateHandler.gameState == GameState.Normal || KickStarter.stateHandler.gameState == GameState.DialogOptions) && (KickStarter.cursorManager.cursorDisplay == CursorDisplay.Always))
					else if (KickStarter.stateHandler.gameState == GameState.Normal || KickStarter.stateHandler.gameState == GameState.DialogOptions)
					{
						showCursor = true;
					}
					else
					{
						showCursor = false;
					}
				}
				else
				{
					showCursor = true;
				}
				
				if (KickStarter.stateHandler.gameState == GameState.Normal && KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && KickStarter.cursorManager != null &&
				    ((KickStarter.cursorManager.cycleCursors && KickStarter.playerInput.GetMouseState () == MouseState.RightClick) || KickStarter.playerInput.InputGetButtonDown ("CycleCursors")))
				{
					CycleCursors ();
				}
				
				else if (KickStarter.stateHandler.gameState == GameState.Normal && KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot &&
				         (KickStarter.playerInput.GetMouseState () == MouseState.RightClick || KickStarter.playerInput.InputGetButtonDown ("CycleCursors")))
				{
					KickStarter.playerInteraction.SetNextInteraction ();
				}

				else if (KickStarter.stateHandler.gameState == GameState.Normal && KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot &&
				         (KickStarter.playerInput.InputGetButtonDown ("CycleCursorsBack")))
				{
					KickStarter.playerInteraction.SetPreviousInteraction ();
				}
			}
			
			if (KickStarter.cursorManager.cursorRendering == CursorRendering.Hardware)
			{
				#if UNITY_5
				Cursor.visible = showCursor;
				#else
				Screen.showCursor = showCursor;
				#endif
			}
		}
		

		/**
		 * Draws the cursor. This is called from StateHandler's OnGUI() function.
		 */
		public void DrawCursor ()
		{
			if (!showCursor)
			{
				return;
			}

			if (KickStarter.playerInput.IsCursorLocked () && KickStarter.settingsManager.hideLockedCursor)
			{
				canShowHardwareCursor = false;
				return;
			}

			GUI.depth = -1;
			canShowHardwareCursor = true;
			
			if (KickStarter.runtimeInventory.selectedItem != null)
			{
				// Cursor becomes selected inventory
				selectedCursor = -2;
				canShowHardwareCursor = false;
			}
			else if (KickStarter.settingsManager.interactionMethod != AC_InteractionMethod.ChooseInteractionThenHotspot)
			{
				if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive && KickStarter.cursorManager.allowInteractionCursorForInventory && KickStarter.runtimeInventory.hoverItem != null)
				{
					ShowContextIcons (KickStarter.runtimeInventory.hoverItem);
					return;
				}
				else if (KickStarter.playerInteraction.GetActiveHotspot () != null && KickStarter.stateHandler.gameState == GameState.Normal && (KickStarter.playerInteraction.GetActiveHotspot ().HasContextUse () || KickStarter.playerInteraction.GetActiveHotspot ().HasContextLook ()))
				{
					selectedCursor = 0;
					
					if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
					{
						if (KickStarter.cursorManager.allowInteractionCursor)
						{
							canShowHardwareCursor = false;
							ShowContextIcons ();
						}
						else if (KickStarter.cursorManager.mouseOverIcon.texture != null)
						{
							DrawIcon (KickStarter.cursorManager.mouseOverIcon, false);
						}
						else
						{
							DrawMainCursor ();
						}
					}
				}
				else
				{
					selectedCursor = -1;
				}
			}
			else if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
			{
				if (KickStarter.stateHandler.gameState == GameState.DialogOptions || KickStarter.stateHandler.gameState == GameState.Paused)
				{
					selectedCursor = -1;
				}
				else if (KickStarter.playerInteraction.GetActiveHotspot () != null && !KickStarter.playerInteraction.GetActiveHotspot ().IsSingleInteraction () && !KickStarter.cursorManager.allowInteractionCursor && KickStarter.cursorManager.mouseOverIcon.texture != null)
				{
					DrawIcon (KickStarter.cursorManager.mouseOverIcon, false);
					return;
				}
			}
			
			
			if (KickStarter.stateHandler.gameState == GameState.Cutscene && KickStarter.cursorManager.waitIcon.texture != null)
			{
				// Wait
				DrawIcon (KickStarter.cursorManager.waitIcon, false);
			}
			else if (selectedCursor == -2 && KickStarter.runtimeInventory.selectedItem != null)
			{
				// Inventory
				canShowHardwareCursor = false;
				
				if (KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot && KickStarter.settingsManager.cycleInventoryCursors)
				{
					if (KickStarter.playerInteraction.GetActiveHotspot () == null && KickStarter.runtimeInventory.hoverItem == null)
					{
						if (KickStarter.playerInteraction.GetInteractionIndex () >= 0)
						{
							// Item was selected due to cycling icons
							KickStarter.playerInteraction.ResetInteractionIndex ();
							KickStarter.runtimeInventory.SetNull ();
							return;
						}
					}
				}

				if (KickStarter.settingsManager.inventoryDragDrop && KickStarter.playerInput.GetDragState () != DragState.Inventory)
				{
					DrawMainCursor ();
				}
				else if (KickStarter.settingsManager.inventoryActiveEffect != InventoryActiveEffect.None && KickStarter.runtimeInventory.selectedItem.CanBeAnimated () && KickStarter.playerMenus.GetHotspotLabel () != "" &&
				    (KickStarter.settingsManager.activeWhenUnhandled || KickStarter.playerInteraction.DoesHotspotHaveInventoryInteraction () || (KickStarter.runtimeInventory.hoverItem != null && KickStarter.runtimeInventory.hoverItem.DoesHaveInventoryInteraction (KickStarter.runtimeInventory.selectedItem))))
				{
					if (KickStarter.cursorManager.inventoryHandling == InventoryHandling.ChangeHotspotLabel)
					{
						DrawMainCursor ();
					}
					else
					{
						DrawActiveInventoryCursor ();
					}
				}
				else if (KickStarter.runtimeInventory.selectedItem.tex)
				{
					if (KickStarter.cursorManager.inventoryHandling == InventoryHandling.ChangeHotspotLabel)
					{
						DrawMainCursor ();
					}
					else
					{
						DrawInventoryCursor ();
					}
				}
				else
				{
					selectedCursor = -1;
					KickStarter.runtimeInventory.SetNull ();
					pulseDirection = 0;
				}
				
				if (KickStarter.runtimeInventory.selectedItem != null && KickStarter.runtimeInventory.selectedItem.canCarryMultiple)
				{
					KickStarter.runtimeInventory.DrawInventoryCount (KickStarter.playerInput.GetMousePosition (), KickStarter.cursorManager.inventoryCursorSize, KickStarter.runtimeInventory.selectedItem.count);
				}
			}
			else if (KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot)
			{
				ShowCycleCursor (KickStarter.playerInteraction.GetActiveUseButtonIconID ());
			}
			else if (KickStarter.cursorManager.allowMainCursor || KickStarter.settingsManager.inputMethod == InputMethod.KeyboardOrController)
			{
				// Pointer
				pulseDirection = 0;

				if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
				{
					if (KickStarter.runtimeInventory.hoverItem == null && KickStarter.playerInteraction.GetActiveHotspot () != null && (!KickStarter.playerMenus.IsInteractionMenuOn () || KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingMenuAndClickingHotspot))
					{
						if (KickStarter.playerInteraction.GetActiveHotspot ().IsSingleInteraction ())
						{
							ShowContextIcons ();
						}
						else if (KickStarter.cursorManager.mouseOverIcon.texture != null)
						{
							DrawIcon (KickStarter.cursorManager.mouseOverIcon, false);
						}
						else
						{
							DrawMainCursor ();
						}
					}
					else
					{
						DrawMainCursor ();
					}
				}
				else if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
				{
					if (selectedCursor == -1)
					{
						DrawMainCursor ();
					}
					else if (selectedCursor == -2 && KickStarter.runtimeInventory.selectedItem == null)
					{
						selectedCursor = -1;
					}
				}
				else if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
				{
					if (KickStarter.playerInteraction.GetActiveHotspot () != null && KickStarter.playerInteraction.GetActiveHotspot ().IsSingleInteraction ())
					{
						selectedCursor = -1;
						ShowContextIcons ();
					}
					else if (selectedCursor >= 0)
					{
						if (KickStarter.cursorManager.allowInteractionCursor)
						{
							//	Custom icon
							pulseDirection = 0;
							canShowHardwareCursor = false;
							
							DrawIcon (KickStarter.cursorManager.cursorIcons [selectedCursor], false);
						}
						else
						{
							DrawMainCursor ();
						}
					}
					else if (selectedCursor == -1)
					{
						DrawMainCursor ();
					}
					else if (selectedCursor == -2 && KickStarter.runtimeInventory.selectedItem == null)
					{
						selectedCursor = -1;
					}
				}
			}
		}
		
		
		private void DrawMainCursor ()
		{
			if (!showCursor)
			{
				return;
			}

			if (KickStarter.cursorManager.cursorDisplay == CursorDisplay.Never)
			{
				return;
			}
			
			if (KickStarter.stateHandler.gameState != GameState.Paused && KickStarter.cursorManager.cursorDisplay == CursorDisplay.OnlyWhenPaused)
			{
				return;
			}
			
			if (KickStarter.stateHandler.gameState == GameState.Cutscene)
			{
				return;
			}

			bool showWalkCursor = false;
			int elementOverCursorID = KickStarter.playerMenus.GetElementOverCursorID ();

			if (elementOverCursorID >= 0)
			{
				DrawIcon (KickStarter.cursorManager.GetCursorIconFromID (elementOverCursorID), false);
				return;
			}

			if (KickStarter.cursorManager.allowWalkCursor && KickStarter.playerInput && !KickStarter.playerMenus.IsMouseOverMenu () && !KickStarter.playerMenus.IsInteractionMenuOn () && KickStarter.stateHandler && KickStarter.stateHandler.gameState == GameState.Normal)
			{
				if (KickStarter.cursorManager.onlyWalkWhenOverNavMesh)
				{
					if (KickStarter.playerMovement.ClickPoint (KickStarter.playerInput.GetMousePosition (), true) != Vector3.zero)
					{
						showWalkCursor = true;
					}
				}
				else
				{
					showWalkCursor = true;
				}
			}
			
			if (showWalkCursor)
			{
				DrawIcon (KickStarter.cursorManager.walkIcon, false);
			}
			else if (KickStarter.cursorManager.pointerIcon.texture)
			{
				DrawIcon (KickStarter.cursorManager.pointerIcon, false);
			}
			else if (KickStarter.cursorManager.allowMainCursor)
			{
				ACDebug.LogWarning ("Main cursor has no texture - please assign one in the Cursor Manager.");
			}
		}
		
		
		private void ShowContextIcons ()
		{
			Hotspot hotspot = KickStarter.playerInteraction.GetActiveHotspot ();
			if (hotspot == null)
			{
				return;
			}
			
			if (hotspot.HasContextUse ())
			{
				if (!hotspot.HasContextLook ())
				{
					DrawIcon (KickStarter.cursorManager.GetCursorIconFromID (hotspot.GetFirstUseButton ().iconID), false);
					return;
				}
				else
				{
					Button _button = hotspot.GetFirstUseButton ();
					
					if (hotspot.HasContextUse () && hotspot.HasContextLook () && KickStarter.cursorManager.lookUseCursorAction == LookUseCursorAction.DisplayBothSideBySide)
					{
						CursorIcon icon = KickStarter.cursorManager.GetCursorIconFromID (_button.iconID);
						DrawIcon (new Vector2 (-icon.size * Screen.width / 2f, 0f), icon, false);
					}
					else
					{
						DrawIcon (KickStarter.cursorManager.GetCursorIconFromID (_button.iconID), false);
					}
				}
			}
			
			if (hotspot.HasContextLook () &&
			    (!hotspot.HasContextUse () ||
			 (hotspot.HasContextUse () && KickStarter.cursorManager.lookUseCursorAction == LookUseCursorAction.DisplayBothSideBySide)))
			{
				if (KickStarter.cursorManager.cursorIcons.Count > 0)
				{
					CursorIcon icon = KickStarter.cursorManager.GetCursorIconFromID (KickStarter.cursorManager.lookCursor_ID);
					
					if (hotspot.HasContextUse () && hotspot.HasContextLook () && KickStarter.cursorManager.lookUseCursorAction == LookUseCursorAction.DisplayBothSideBySide)
					{
						DrawIcon (new Vector2 (icon.size * Screen.width / 2f, 0f), icon, true);
					}
					else
					{
						DrawIcon (icon, true);
					}
				}
			}	
		}
		
		
		private void ShowContextIcons (InvItem invItem)
		{
			if (KickStarter.cursorManager.cursorIcons.Count > 0)
			{
				if (invItem.lookActionList != null && KickStarter.cursorManager.lookUseCursorAction == LookUseCursorAction.DisplayBothSideBySide)
				{
					CursorIcon icon = KickStarter.cursorManager.GetCursorIconFromID (invItem.useIconID);
					DrawIcon (new Vector2 (-icon.size * Screen.width / 2f, 0f), icon, false);
				}
				else
				{
					DrawIcon (KickStarter.cursorManager.GetCursorIconFromID (invItem.useIconID), false);
				}
				
				if (invItem.lookActionList != null)
				{
					CursorIcon icon = KickStarter.cursorManager.GetCursorIconFromID (KickStarter.cursorManager.lookCursor_ID);
					
					if (invItem.lookActionList != null && KickStarter.cursorManager.lookUseCursorAction == LookUseCursorAction.DisplayBothSideBySide)
					{
						DrawIcon (new Vector2 (icon.size * Screen.width / 2f, 0f), icon, true);
					}
					else
					{
						DrawIcon (icon, true);
					}
				}
			}	
		}
		
		
		private void ShowCycleCursor (int useCursorID)
		{
			if (KickStarter.runtimeInventory.selectedItem != null)
			{
				selectedCursor = -2;
				DrawActiveInventoryCursor ();
			}
			else if (useCursorID >= 0)
			{
				selectedCursor = useCursorID;
				DrawIcon (KickStarter.cursorManager.GetCursorIconFromID (useCursorID), false);
			}
			else if (useCursorID == -1)
			{
				selectedCursor = -1;
				DrawMainCursor ();
			}
		}


		private void DrawInventoryCursor ()
		{
			InvItem invItem = KickStarter.runtimeInventory.selectedItem;
			if (invItem == null)
			{
				return;
			}

			if (invItem.cursorIcon.texture != null)
			{
				if (KickStarter.settingsManager.inventoryActiveEffect != InventoryActiveEffect.None)
				{
					// Only animate when active
					DrawIcon (invItem.cursorIcon, false, false);
				}
				else
				{
					DrawIcon (invItem.cursorIcon, false, true);
				}
			}
			else
			{
				DrawIcon (AdvGame.GUIBox (KickStarter.playerInput.GetMousePosition (), KickStarter.cursorManager.inventoryCursorSize), KickStarter.runtimeInventory.selectedItem.tex);
			}
			pulseDirection = 0;
		}
		
		
		private void DrawActiveInventoryCursor ()
		{
			InvItem invItem = KickStarter.runtimeInventory.selectedItem;
			if (invItem == null)
			{
				return;
			}

			if (invItem.cursorIcon.texture != null)
			{
				DrawIcon (invItem.cursorIcon, false, true);
			}
			else if (invItem.activeTex == null)
			{
				DrawInventoryCursor ();
			}
			else if (KickStarter.settingsManager.inventoryActiveEffect == InventoryActiveEffect.Simple)
			{
				DrawIcon (AdvGame.GUIBox (KickStarter.playerInput.GetMousePosition (), KickStarter.cursorManager.inventoryCursorSize), invItem.activeTex);
			}
			else if (KickStarter.settingsManager.inventoryActiveEffect == InventoryActiveEffect.Pulse && invItem.tex)
			{
				if (pulseDirection == 0)
				{
					pulse = 0f;
					pulseDirection = 1;
				}
				else if (pulse > 1f)
				{
					pulse = 1f;
					pulseDirection = -1;
				}
				else if (pulse < 0f)
				{
					pulse = 0f;
					pulseDirection = 1;
				}
				else if (pulseDirection == 1)
				{
					pulse += KickStarter.settingsManager.inventoryPulseSpeed * Time.deltaTime;
				}
				else if (pulseDirection == -1)
				{
					pulse -= KickStarter.settingsManager.inventoryPulseSpeed * Time.deltaTime;
				}
				
				Color backupColor = GUI.color;
				Color tempColor = GUI.color;
				
				tempColor.a = pulse;
				GUI.color = tempColor;
				DrawIcon (AdvGame.GUIBox (KickStarter.playerInput.GetMousePosition (), KickStarter.cursorManager.inventoryCursorSize), invItem.activeTex);
				GUI.color = backupColor;
				DrawIcon (AdvGame.GUIBox (KickStarter.playerInput.GetMousePosition (), KickStarter.cursorManager.inventoryCursorSize), invItem.tex);
			}
		}
		
		
		private void DrawIcon (Rect _rect, Texture2D _tex)
		{
			if (_tex != null)
			{
				if (KickStarter.cursorManager.cursorRendering == CursorRendering.Hardware)
				{
					Cursor.SetCursor (_tex, Vector2.zero, CursorMode.Auto);
				}
				else
				{
					GUI.DrawTexture (_rect, _tex, ScaleMode.ScaleToFit, true, 0f);
				}
			}
		}
		
		
		private void DrawIcon (Vector2 offset, CursorIconBase icon, bool isLook, bool canAnimate = true)
		{
			if (icon != null)
			{
				bool isNew = false;
				if (isLook && activeLookIcon != icon)
				{
					activeLookIcon = icon;
					isNew = true;
					icon.Reset ();
				}
				else if (!isLook && activeIcon != icon)
				{
					activeIcon = icon;
					isNew = true;
					icon.Reset ();
				}
				
				if (KickStarter.cursorManager.cursorRendering == CursorRendering.Hardware)
				{
					if (icon.isAnimated && canAnimate)
					{
						Texture2D animTex = icon.GetAnimatedTexture ();
						if (icon.GetName () != lastCursorName)
						{
							lastCursorName = icon.GetName ();
							Cursor.SetCursor (animTex, icon.clickOffset, CursorMode.Auto);
						}
					}
					else if (isNew)
					{
						Cursor.SetCursor (icon.texture, icon.clickOffset, CursorMode.Auto);
					}
				}
				else
				{
					icon.Draw (KickStarter.playerInput.GetMousePosition () + offset, canAnimate);
				}
			}
		}
		
		
		private void DrawIcon (CursorIconBase icon, bool isLook, bool canAnimate = true)
		{
			if (icon != null)
			{
				DrawIcon (new Vector2 (0f, 0f), icon, isLook, canAnimate);
			}
		}

		
		private void CycleCursors ()
		{
			if (KickStarter.playerInteraction.GetActiveHotspot () != null && KickStarter.playerInteraction.GetActiveHotspot ().IsSingleInteraction ())
			{
				return;
			}

			if (KickStarter.cursorManager.cursorIcons.Count > 0)
			{
				selectedCursor ++;

				if (selectedCursor < KickStarter.cursorManager.cursorIcons.Count && KickStarter.cursorManager.cursorIcons [selectedCursor].dontCycle)
				{
					while (KickStarter.cursorManager.cursorIcons [selectedCursor].dontCycle)
					{
						selectedCursor ++;

						if (selectedCursor >= KickStarter.cursorManager.cursorIcons.Count)
						{
							selectedCursor = -1;
							break;
						}
					}
				}
				else if (selectedCursor >= KickStarter.cursorManager.cursorIcons.Count)
				{
					selectedCursor = -1;
				}
			}
			else
			{
				// Pointer
				selectedCursor = -1;
			}
		}
		

		/**
		 * <summary>Gets the index number of the currently-selected cursor within CursorManager's cursorIcons List.</summary>
		 * <returns>If = -2, the inventory cursor is showing.
		 * If = -1, the main pointer is showing.
		 * If > 0, the index number of the currently-selected cursor within CursorManager's cursorIcons List</returns>
		 */
		public int GetSelectedCursor ()
		{
			return selectedCursor;
		}
		

		/**
		 * <summary>Gets the ID number of the currently-selected cursor, within CursorManager's cursorIcons List.</summary>
		 * <returns>The ID number of the currently-selected cursor, within CursorManager's cursorIcons List</returns>
		 */
		public int GetSelectedCursorID ()
		{
			if (KickStarter.cursorManager && KickStarter.cursorManager.cursorIcons.Count > 0 && selectedCursor > -1)
			{
				return KickStarter.cursorManager.cursorIcons [selectedCursor].id;
			}
			return -1;
		}
		

		/**
		 * <summary>Resets the currently-selected cursor</summary>
		 */
		public void ResetSelectedCursor ()
		{
			selectedCursor = -1;
		}
		

		/**
		 * <summary>Sets the cursor to an icon defined in CursorManager.</summary>
		 * <param name = "ID">The ID number of the cursor, within CursorManager's cursorIcons List, to select</param>
		 */
		public void SetCursorFromID (int ID)
		{
			if (KickStarter.cursorManager && KickStarter.cursorManager.cursorIcons.Count > 0)
			{
				foreach (CursorIcon cursor in KickStarter.cursorManager.cursorIcons)
				{
					if (cursor.id == ID)
					{
						SetCursor (cursor);
					}
				}
			}
		}


		/**
		 * <summary>Sets the cursor to an icon defined in CursorManager.</summary>
		 * <param name = "_icon">The cursor, within CursorManager's cursorIcons List, to select</param>
		 */
		public void SetCursor (CursorIcon _icon)
		{
			selectedCursor = KickStarter.cursorManager.cursorIcons.IndexOf (_icon);
		}

	}
	
}