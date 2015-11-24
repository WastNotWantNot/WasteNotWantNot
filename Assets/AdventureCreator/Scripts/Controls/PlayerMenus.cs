/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"PlayerMenus.cs"
 * 
 *	This script handles the displaying of each of the menus defined in MenuManager.
 *	It avoids referencing specific menus and menu elements as much as possible,
 *	so that the menu can be completely altered using just the MenuSystem script.
 * 
 */

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	/**
	 * This script handles the initialisation, position and display of all Menus defined in MenuManager.
	 * Menus are transferred from MenuManager to a local List within this script when the game begins.
	 * It must be placed on the PersistentEngine prefab.
	 */
	public class PlayerMenus : MonoBehaviour
	{

		private bool mouseOverMenu = false;
		private bool mouseOverInteractionMenu = false;
		private bool interactionMenuIsOn = false;

		private bool lockSave = false;
		private int selected_option;

		private bool foundMouseOverMenu = false;
		private bool foundMouseOverInteractionMenu = false;
		private bool foundMouseOverInventory = false;
		private bool mouseOverInventory = false;

		private bool isPaused;
		private string hotspotLabel = "";
		private float pauseAlpha = 0f;
		private List<Menu> menus = new List<Menu>();
		private List<Menu> dupMenus = new List<Menu>();
		private Texture2D pauseTexture;
		private string elementIdentifier;
		private string lastElementIdentifier;
		private MenuInput selectedInputBox;
		private string selectedInputBoxMenuName;
		private Vector2 activeInventoryBoxCentre = Vector2.zero;
		private InvItem oldHoverItem;
		private int doResizeMenus = 0;
		
		private Menu crossFadeTo;
		private Menu crossFadeFrom;
		private UnityEngine.EventSystems.EventSystem eventSystem;

		private int elementOverCursorID = -1;

		private GUIStyle normalStyle = new GUIStyle ();
		private GUIStyle highlightedStyle = new GUIStyle();
		
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		private TouchScreenKeyboard keyboard;
		#endif

		
		private void Start ()
		{
			RebuildMenus ();
		}


		/**
		 * <summary>Rebuilds the game's Menus, either from the existing MenuManager asset, or from a new one.</summary>
		 * <param name = "menuManager">The Menu Manager to use for Menu generation. If left empty, the default Menu Manager will be used.</param>
		 */
		public void RebuildMenus (MenuManager menuManager = null)
		{
			if (menuManager != null)
			{
				KickStarter.menuManager = menuManager;
			}

			foreach (Menu menu in menus)
			{
				if (menu.menuSource == MenuSource.UnityUiPrefab && menu.canvas != null && menu.canvas.gameObject != null)
				{
					Destroy (menu.canvas.gameObject);
				}
			}

			menus = new List<Menu>();
			
			if (KickStarter.menuManager)
			{
				pauseTexture = KickStarter.menuManager.pauseTexture;
				foreach (AC.Menu _menu in KickStarter.menuManager.menus)
				{
					Menu newMenu = ScriptableObject.CreateInstance <Menu>();
					newMenu.Copy (_menu);
					
					if (_menu.appearType == AppearType.WhenSpeechPlays && _menu.oneMenuPerSpeech)
					{
						// Don't make canvas object yet!
					}
					else if (newMenu.IsUnityUI ())
					{
						newMenu.LoadUnityUI ();
					}
					newMenu.Initalise ();
					menus.Add (newMenu);
				}
			}
			
			CreateEventSystem ();
			
			foreach (AC.Menu menu in menus)
			{
				menu.Recalculate ();
			}
			
			#if UNITY_WEBPLAYER && !UNITY_EDITOR
			// WebPlayer takes another second to get the correct screen dimensions
			foreach (AC.Menu menu in menus)
			{
				menu.Recalculate ();
			}
			#endif
		}


		private void CreateEventSystem ()
		{
			if (GameObject.FindObjectOfType <UnityEngine.EventSystems.EventSystem>() == null)
			{
				UnityEngine.EventSystems.EventSystem _eventSystem = null;

				if (KickStarter.menuManager.eventSystem != null)
				{
					_eventSystem = (UnityEngine.EventSystems.EventSystem) Instantiate (KickStarter.menuManager.eventSystem);
					_eventSystem.gameObject.name = KickStarter.menuManager.eventSystem.name;
				}
				else if (AreAnyMenusUI ())
				{
					GameObject eventSystemObject = new GameObject ();
					eventSystemObject.name = "EventSystem";
					_eventSystem = eventSystemObject.AddComponent <UnityEngine.EventSystems.EventSystem>();
					eventSystemObject.AddComponent <StandaloneInputModule>();
					#if !UNITY_5_3
					eventSystemObject.AddComponent <TouchInputModule>();
					#endif
				}

				if (_eventSystem != null)
				{
					if (GameObject.Find ("_UI"))
					{
						_eventSystem.transform.SetParent (GameObject.Find ("_UI").transform);
					}
					eventSystem = _eventSystem;
				}
			}
		}


		private bool AreAnyMenusUI ()
		{
			foreach (AC.Menu menu in menus)
			{
				if (menu.menuSource == MenuSource.UnityUiInScene || menu.menuSource == MenuSource.UnityUiPrefab)
				{
					return true;
				}
			}
			return false;
		}
		
		
		private void OnLevelWasLoaded ()
		{
			if (KickStarter.settingsManager != null && KickStarter.settingsManager.IsInLoadingScene ())
			{
				return;
			}

			CreateEventSystem ();

			foreach (AC.Menu _menu in menus)
			{
				if (_menu.menuSource == MenuSource.UnityUiInScene)
				{
					_menu.LoadUnityUI ();
					_menu.Initalise ();
				}
				else if (_menu.menuSource == MenuSource.UnityUiPrefab)
				{
					_menu.SetParent ();
				}
			}
		}


		/**
		 * Clears the parents of any Unity UI-based Menu Canvases.
		 * This makes them able to survive a scene change.
		 */
		public void ClearParents ()
		{
			foreach (AC.Menu _menu in menus)
			{
				if (_menu.IsUnityUI () && _menu.canvas != null)
				{
					_menu.ClearParent ();
				}
			}
		}


		private void ShowPauseBackground (bool fadeIn)
		{
			float fadeSpeed = 0.5f;
			if (fadeIn)
			{
				if (pauseAlpha < 1f)
				{
					pauseAlpha += (0.2f * fadeSpeed);
				}				
				else
				{
					pauseAlpha = 1f;
				}
			}
			
			else
			{
				if (pauseAlpha > 0f)
				{
					pauseAlpha -= (0.2f * fadeSpeed);
				}
				else
				{
					pauseAlpha = 0f;
				}
			}
			
			Color tempColor = GUI.color;
			tempColor.a = pauseAlpha;
			GUI.color = tempColor;
			GUI.DrawTexture (AdvGame.GUIRect (0.5f, 0.5f, 1f, 1f), pauseTexture, ScaleMode.ScaleToFit, true, 0f);
		}


		/**
		 * Draws any OnGUI-based Menus set to appear while the game is loading.
		 */
		public void DrawLoadingMenus ()
		{
			foreach (AC.Menu menu in menus)
			{
				int languageNumber = Options.GetLanguage ();
				if (menu.appearType == AppearType.WhileLoading)
				{
					DrawMenu (menu, languageNumber);
				}
			}
		}
		

		/**
		 * Draws all OnGUI-based Menus.
		 */
		public void DrawMenus ()
		{
			if (doResizeMenus > 0)
			{
				return;
			}

			elementOverCursorID = -1;

			if (KickStarter.playerInteraction && KickStarter.playerInput && KickStarter.menuSystem && KickStarter.stateHandler && KickStarter.settingsManager)
			{
				GUI.depth = KickStarter.menuManager.globalDepth;
				
				if (pauseTexture)
				{
					isPaused = false;
					foreach (AC.Menu menu in menus)
					{
						if (menu.IsEnabled () && menu.IsBlocking ())
						{
							isPaused = true;
						}
					}
					
					if (isPaused)
					{
						ShowPauseBackground (true);
					}
					else
					{
						ShowPauseBackground (false);
					}
				}
				
				if (selectedInputBox)
				{
					Event currentEvent = Event.current;
					if (currentEvent.isKey && currentEvent.type == EventType.KeyDown)
					{
						selectedInputBox.CheckForInput (currentEvent.keyCode.ToString (), currentEvent.shift, selectedInputBoxMenuName);
					}
				}
				
				int languageNumber = Options.GetLanguage ();

				foreach (AC.Menu menu in menus)
				{
					DrawMenu (menu, languageNumber);
				}

				foreach (AC.Menu menu in dupMenus)
				{
					DrawMenu (menu, languageNumber);
				}
			}
		}


		private void DrawMenu (AC.Menu menu, int languageNumber)
		{
			Color tempColor = GUI.color;
			
			bool isACMenu = !menu.IsUnityUI ();
			
			if (menu.IsEnabled ())
			{
				if (!menu.HasTransition () && menu.IsFading ())
				{
					// Stop until no longer "fading" so that it appears in right place
					return;
				}
				
				if (isACMenu)
				{
					if (menu.transitionType == MenuTransition.Fade || menu.transitionType == MenuTransition.FadeAndPan)
					{
						tempColor.a = 1f - menu.GetFadeProgress ();
						GUI.color = tempColor;
					}
					else
					{
						tempColor.a = 1f;
						GUI.color = tempColor;
					}
					
					menu.StartDisplay ();
				}

				foreach (MenuElement element in menu.elements)
				{
					if (element.isVisible)
					{
						if (isACMenu)
						{
							SetStyles (element);
						}
						
						for (int i=0; i<element.GetNumSlots (); i++)
						{
							if (menu.IsEnabled () && KickStarter.stateHandler.gameState != GameState.Cutscene && KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingMenuAndClickingHotspot && menu.appearType == AppearType.OnInteraction)
							{
								if (element is MenuInteraction)
								{
									MenuInteraction menuInteraction = (MenuInteraction) element;
									if (menuInteraction.iconID == KickStarter.playerInteraction.GetActiveUseButtonIconID ())
									{
										if (KickStarter.cursorManager.addHotspotPrefix)
										{
											if (KickStarter.runtimeInventory.hoverItem != null)
											{
												hotspotLabel = KickStarter.cursorManager.GetLabelFromID (menuInteraction.iconID, languageNumber) + KickStarter.runtimeInventory.hoverItem.GetLabel (languageNumber);
											}
											else
											{
												hotspotLabel = KickStarter.cursorManager.GetLabelFromID (menuInteraction.iconID, languageNumber) + KickStarter.playerInteraction.GetLabel (languageNumber);
											}
										}
										if (isACMenu)
										{
											element.Display (highlightedStyle, i, menu.GetZoom (), true);
										}
									}
									else
									{
										if (isACMenu)
										{
											element.Display (normalStyle, i, menu.GetZoom (), false);
										}
									}
								}
								else if (element is MenuInventoryBox)
								{
									MenuInventoryBox menuInventoryBox = (MenuInventoryBox) element;
									if (menuInventoryBox.inventoryBoxType == AC_InventoryBoxType.HotspotBased && menuInventoryBox.items[i].id == KickStarter.playerInteraction.GetActiveInvButtonID ())
									{
										if (KickStarter.cursorManager.addHotspotPrefix)
										{
											hotspotLabel = KickStarter.runtimeInventory.GetHotspotPrefixLabel (menuInventoryBox.GetItem (i), menuInventoryBox.GetLabel (i, languageNumber), languageNumber);

											if (KickStarter.runtimeInventory.selectedItem != null)
											{
												hotspotLabel += KickStarter.runtimeInventory.selectedItem.GetLabel (languageNumber);
											}
											else
											{
												hotspotLabel += KickStarter.playerInteraction.GetLabel (languageNumber);
											}
										}
										if (isACMenu)
										{
											element.Display (highlightedStyle, i, menu.GetZoom (), true);
										}
									}
									else if (isACMenu)
									{
										element.Display (normalStyle, i, menu.GetZoom (), false);
									}
								}
								else if (isACMenu)
								{
									element.Display (normalStyle, i, menu.GetZoom (), false);
								}
							}
							
							else if (menu.IsVisible () && !menu.ignoreMouseClicks && element.isClickable && KickStarter.playerInput.IsCursorReadable () && KickStarter.stateHandler.gameState != GameState.Cutscene && 
							         ((KickStarter.settingsManager.inputMethod == InputMethod.MouseAndKeyboard && menu.IsPointerOverSlot (element, i, KickStarter.playerInput.GetInvertedMouse ())) ||
							 (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen && menu.IsPointerOverSlot (element, i, KickStarter.playerInput.GetInvertedMouse ())) ||
							 (KickStarter.settingsManager.inputMethod == InputMethod.KeyboardOrController && KickStarter.stateHandler.gameState == GameState.Normal && menu.IsPointerOverSlot (element, i, KickStarter.playerInput.GetInvertedMouse ())) ||
							 ((KickStarter.settingsManager.inputMethod == InputMethod.KeyboardOrController && KickStarter.stateHandler.gameState != GameState.Normal && menu.selected_element == element && menu.selected_slot == i))))
							{
								if (isACMenu)
								{
									float zoom = 1;
									if (menu.transitionType == MenuTransition.Zoom)
									{
										zoom = menu.GetZoom ();
									}

									if ((!interactionMenuIsOn || menu.appearType == AppearType.OnInteraction)
									    && (KickStarter.playerInput.GetDragState () == DragState.None || (KickStarter.playerInput.GetDragState () == DragState.Inventory && CanElementBeDroppedOnto (element))))
									{
										element.Display (highlightedStyle, i, zoom, true);

										if (element.changeCursor)
										{
											elementOverCursorID = element.cursorID;
										}
									}
									else
									{
										element.Display (normalStyle, i, zoom, false);
									}
								}
								else
								{
									// Unity UI
									if ((!interactionMenuIsOn || menu.appearType == AppearType.OnInteraction)
									    && (KickStarter.playerInput.GetDragState () == DragState.None || (KickStarter.playerInput.GetDragState () == DragState.Inventory && CanElementBeDroppedOnto (element))))
									{
										if (element.changeCursor)
										{
											elementOverCursorID = element.cursorID;
										}
									}
								}
							}
							else if (isACMenu)
							{
								element.Display (normalStyle, i, menu.GetZoom (), false);
							}
						}
						
						if (element is MenuInput)
						{
							if (selectedInputBox == null)
							{
								if (!menu.IsUnityUI ())
								{
									MenuInput input = (MenuInput) element;
									SelectInputBox (input);
								}
								
								selectedInputBoxMenuName = menu.title;
							}
						}
					}
				}
				
				if (isACMenu)
				{
					menu.EndDisplay ();
				}
			}
			
			if (isACMenu)
			{
				tempColor.a = 1f;
				GUI.color = tempColor;
			}
		}
		

		/**
		 * <summary>Updates a Menu's position.</summary>
		 * <param name = "menu">The Menu to reposition</param>
		 * <param name = "invertedMouse">The y-inverted mouse position</param>
		 */
		public void UpdateMenuPosition (AC.Menu menu, Vector2 invertedMouse)
		{
			if (menu.IsUnityUI ())
			{
				if (Application.isPlaying)
				{
					Vector2 screenPosition = Vector2.zero;

					if (menu.uiPositionType == UIPositionType.Manual)
					{
						return;
					}
					else if (menu.uiPositionType == UIPositionType.FollowCursor)
					{
						screenPosition = new Vector2 (invertedMouse.x, Screen.height + 1f - invertedMouse.y);
						menu.SetCentre (screenPosition);
					}
					else if (menu.uiPositionType == UIPositionType.OnHotspot)
					{
						if (!menu.IsFadingOut ())
						{
							if (mouseOverInventory)
							{
								screenPosition = new Vector2 (activeInventoryBoxCentre.x, Screen.height + 1f - activeInventoryBoxCentre.y);
								menu.SetCentre (screenPosition);
							}
							else if (KickStarter.playerInteraction.GetActiveHotspot ())
							{
								if (menu.canvas.renderMode == RenderMode.WorldSpace)
								{
									menu.SetCentre (KickStarter.playerInteraction.GetActiveHotspot ().transform.position);
								}
								else
								{
									screenPosition = KickStarter.playerInteraction.GetHotspotScreenCentre ();
									screenPosition = new Vector2 (screenPosition.x * Screen.width, (1f - screenPosition.y) * Screen.height);
									menu.SetCentre (screenPosition);
								}
							}
						}
					}
					else if (menu.uiPositionType == UIPositionType.AboveSpeakingCharacter)
					{
						Char speaker = null;
						if (dupMenus.Contains (menu))
						{
							if (menu.speech != null)
							{
								speaker = menu.speech.GetSpeakingCharacter ();
							}
						}
						else
						{
							speaker = KickStarter.dialog.GetSpeakingCharacter ();
						}

						if (speaker != null)
						{
							if (menu.canvas.renderMode == RenderMode.WorldSpace)
							{
								menu.SetCentre (speaker.transform.position);
							}
							else
							{
								screenPosition = speaker.GetScreenCentre ();
								screenPosition = new Vector2 (screenPosition.x * Screen.width, (1f - screenPosition.y) * Screen.height);
								menu.SetCentre (screenPosition);
							}
						}
					}
					else if (menu.uiPositionType == UIPositionType.AbovePlayer)
					{
						if (KickStarter.player)
						{
							if (menu.canvas.renderMode == RenderMode.WorldSpace)
							{
								menu.SetCentre (KickStarter.player.transform.position);
							}
							else
							{
								screenPosition = KickStarter.player.GetScreenCentre ();
								screenPosition = new Vector2 (screenPosition.x * Screen.width, (1f - screenPosition.y) * Screen.height);
								menu.SetCentre (screenPosition);
							}
						}
					}

				}

				return;
			}

			if (menu.sizeType == AC_SizeType.Automatic && menu.autoSizeEveryFrame)
			{
				menu.Recalculate ();
			}

			if (invertedMouse == Vector2.zero)
			{
				invertedMouse = KickStarter.playerInput.GetInvertedMouse ();
			}
			
			if (menu.positionType == AC_PositionType.FollowCursor)
			{
				menu.SetCentre (new Vector2 ((invertedMouse.x / Screen.width) + (menu.manualPosition.x / 100f) - 0.5f,
				                             (invertedMouse.y / Screen.height) + (menu.manualPosition.y / 100f) - 0.5f));
			}
			else if (menu.positionType == AC_PositionType.OnHotspot)
			{
				if (!menu.IsFadingOut ())
				{
					if (mouseOverInventory)
					{
						Vector2 screenPosition = new Vector2 (activeInventoryBoxCentre.x / Screen.width, activeInventoryBoxCentre.y / Screen.height);
						menu.SetCentre (new Vector2 (screenPosition.x + (menu.manualPosition.x / 100f) - 0.5f,
						                             screenPosition.y + (menu.manualPosition.y / 100f) - 0.5f));
					}
					else if (KickStarter.playerInteraction.GetActiveHotspot ())
					{
						Vector2 screenPosition = KickStarter.playerInteraction.GetHotspotScreenCentre ();
						menu.SetCentre (new Vector2 (screenPosition.x + (menu.manualPosition.x / 100f) - 0.5f,
						                             screenPosition.y + (menu.manualPosition.y / 100f) - 0.5f));
					}
				}
			}
			else if (menu.positionType == AC_PositionType.AboveSpeakingCharacter)
			{
				Char speaker = null;
				if (dupMenus.Contains (menu))
				{
					if (menu.speech != null)
					{
						speaker = menu.speech.GetSpeakingCharacter ();
					}
				}
				else
				{
					speaker = KickStarter.dialog.GetSpeakingCharacter ();
				}

				if (speaker != null)
				{
					Vector2 screenPosition = speaker.GetScreenCentre ();
					menu.SetCentre (new Vector2 (screenPosition.x + (menu.manualPosition.x / 100f) - 0.5f,
					                             screenPosition.y + (menu.manualPosition.y / 100f) - 0.5f));
				}
			}
			else if (menu.positionType == AC_PositionType.AbovePlayer)
			{
				if (KickStarter.player)
				{
					Vector2 screenPosition = KickStarter.player.GetScreenCentre ();
					menu.SetCentre (new Vector2 (screenPosition.x + (menu.manualPosition.x / 100f) - 0.5f,
					                             screenPosition.y + (menu.manualPosition.y / 100f) - 0.5f));
				}
			}
		}


		private void UpdateMenu (AC.Menu menu)
		{
			Vector2 invertedMouse = KickStarter.playerInput.GetInvertedMouse ();
			UpdateMenuPosition (menu, invertedMouse);
			
			menu.HandleTransition ();

			if (KickStarter.settingsManager.inputMethod == InputMethod.KeyboardOrController && menu.IsEnabled ())
			{
				KickStarter.playerInput.InputControlMenu (menu);
			}

			if (menu.appearType == AppearType.Manual)
			{
				if (menu.IsVisible () && !menu.isLocked && menu.IsPointInside (invertedMouse) && !menu.ignoreMouseClicks)
				{
					foundMouseOverMenu = true;
				}
			}

			else if (menu.appearType == AppearType.DuringGameplay)
			{
				if (KickStarter.stateHandler.gameState == GameState.Normal && !menu.isLocked)
				{
					if (menu.IsOff ())
					{
						menu.TurnOn (true);
					}

					if (menu.IsOn () && menu.IsPointInside (invertedMouse))
					{
						foundMouseOverMenu = true;
					}
				}
				else if (KickStarter.stateHandler.gameState == GameState.Paused)
				{
					menu.TurnOff (true);
				}
				else if (menu.IsOn () && KickStarter.actionListManager.IsGameplayBlocked ())
				{
					menu.TurnOff (true);
				}
			}
			
			else if (menu.appearType == AppearType.MouseOver)
			{
				if (KickStarter.stateHandler.gameState == GameState.Normal && !menu.isLocked && menu.IsPointInside (invertedMouse))
				{
					if (menu.IsOff ())
					{
						menu.TurnOn (true);
					}
					
					if (!menu.ignoreMouseClicks)
					{
						foundMouseOverMenu = true;
					}
				}
				else if (KickStarter.stateHandler.gameState == GameState.Paused)
				{
					menu.ForceOff ();
				}
				else
				{
					menu.TurnOff (true);
				}
			}
			
			else if (menu.appearType == AppearType.OnContainer)
			{
				if (KickStarter.playerInput.activeContainer != null && !menu.isLocked && (KickStarter.stateHandler.gameState == GameState.Normal || (KickStarter.stateHandler.gameState == AC.GameState.Paused && menu.pauseWhenEnabled)))
				{
					if (menu.IsVisible () && menu.IsPointInside (invertedMouse) && !menu.ignoreMouseClicks)
					{
						foundMouseOverMenu = true;
					}
					menu.TurnOn (true);
				}
				else
				{
					menu.TurnOff (true);
				}
			}
			
			else if (menu.appearType == AppearType.DuringConversation)
			{
				if (KickStarter.playerInput.activeConversation != null && KickStarter.stateHandler.gameState == GameState.DialogOptions)
				{
					menu.TurnOn (true);
				}
				else if (KickStarter.stateHandler.gameState == GameState.Paused)
				{
					menu.ForceOff ();
				}
				else
				{
					menu.TurnOff (true);
				}
			}
			
			else if (menu.appearType == AppearType.OnInputKey)
			{
				if (menu.IsEnabled () && !menu.isLocked && menu.IsPointInside (invertedMouse) && !menu.ignoreMouseClicks)
				{
					foundMouseOverMenu = true;
				}
				
				try
				{
					if (KickStarter.playerInput.InputGetButtonDown (menu.toggleKey, true))
					{
						if (!menu.IsEnabled ())
						{
							if (KickStarter.stateHandler.gameState == GameState.Paused)
							{
								CrossFade (menu);
							}
							else
							{
								menu.TurnOn (true);
							}
						}
						else
						{
							menu.TurnOff (true);
						}
					}
				}
				catch
				{
					if (KickStarter.settingsManager.inputMethod != InputMethod.TouchScreen)
					{
						ACDebug.LogWarning ("No '" + menu.toggleKey + "' button exists - please define one in the Input Manager.");
					}
				}
			}
			
			else if (menu.appearType == AppearType.OnHotspot)
			{
				if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive && !menu.isLocked && KickStarter.runtimeInventory.selectedItem == null)
				{
					Hotspot hotspot = KickStarter.playerInteraction.GetActiveHotspot ();
					if (hotspot != null)
					{
						menu.HideInteractions ();
						
						if (hotspot.HasContextUse ())
						{
							menu.MatchUseInteraction (hotspot.GetFirstUseButton ());
						}
						
						if (hotspot.HasContextLook ())
						{
							menu.MatchLookInteraction (hotspot.lookButton);
						}
						
						menu.Recalculate ();
					}
				}

				if (hotspotLabel != "" && !menu.isLocked && KickStarter.stateHandler.gameState != GameState.Cutscene)
				    //(KickStarter.stateHandler.gameState == GameState.Normal || KickStarter.stateHandler.gameState == GameState.DialogOptions))
				{
					if (!menu.IsOn ())
					{
						menu.TurnOn (true);
						if (menu.IsUnityUI ())
						{
							// Update position before next frame (Unity UI bug)
							UpdateMenuPosition (menu, invertedMouse);
						}
					}
				}
				//else if (KickStarter.stateHandler.gameState == GameState.Paused)
				else if (KickStarter.stateHandler.gameState == GameState.Cutscene)
				{
					menu.ForceOff ();
				}
				else
				{
					menu.TurnOff (true);
				}
			}
			
			else if (menu.appearType == AppearType.OnInteraction)
			{
				if (KickStarter.settingsManager.CanClickOffInteractionMenu ())
				{
					if (menu.IsEnabled () && (KickStarter.stateHandler.gameState == GameState.Normal || menu.pauseWhenEnabled))
					{
						interactionMenuIsOn = true;

						if (menu.IsPointInside (invertedMouse) && !menu.ignoreMouseClicks)
						{
							foundMouseOverInteractionMenu = true;
						}
						else if (KickStarter.playerInput.GetMouseState () == MouseState.SingleClick)
						{
							KickStarter.playerInput.ResetMouseClick ();
							interactionMenuIsOn = false;
							menu.TurnOff (true);
						}
					}
					else if (KickStarter.stateHandler.gameState == GameState.Paused)
					{
						interactionMenuIsOn = false;
						menu.ForceOff ();
					}
					else if (KickStarter.playerInteraction.GetActiveHotspot () == null)
					{
						interactionMenuIsOn = false;
						menu.TurnOff (true);
					}
				}
				else
				{
					if (menu.IsEnabled () && (KickStarter.stateHandler.gameState == GameState.Normal || menu.pauseWhenEnabled))
					{
						if (menu.IsPointInside (invertedMouse) && !menu.ignoreMouseClicks)
						{
							foundMouseOverInteractionMenu = true;
						}
						else if (!menu.IsPointInside (invertedMouse) && !menu.ignoreMouseClicks && KickStarter.playerInteraction.GetActiveHotspot () == null && KickStarter.runtimeInventory.hoverItem == null &&
						    (KickStarter.settingsManager.interactionMethod != AC_InteractionMethod.ChooseHotspotThenInteraction || KickStarter.settingsManager.cancelInteractions == CancelInteractions.CursorLeavesMenuOrHotspot))
						{
							interactionMenuIsOn = false;
							menu.TurnOff (true);
						}
						else if (!menu.IsPointInside (invertedMouse) && !menu.ignoreMouseClicks && KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && KickStarter.settingsManager.cancelInteractions == CancelInteractions.CursorLeavesMenu && !menu.IsFadingIn ())
						{
							interactionMenuIsOn = false;
							menu.TurnOff (true);
						}
						else if (KickStarter.playerInteraction.GetActiveHotspot () == null && KickStarter.runtimeInventory.hoverItem == null &&
						    KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && KickStarter.settingsManager.selectInteractions == AC.SelectInteractions.CyclingMenuAndClickingHotspot)
						{
							interactionMenuIsOn = false;
							menu.TurnOff (true);
						}
						else if (KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingMenuAndClickingHotspot && KickStarter.playerInteraction.GetActiveHotspot () != null)
						{}
						else if (KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingMenuAndClickingHotspot && KickStarter.runtimeInventory.hoverItem != null)
						{}
						else if (KickStarter.playerInteraction.GetActiveHotspot () == null || KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen)
						{}
						else if (KickStarter.runtimeInventory.selectedItem == null && KickStarter.playerInteraction.GetActiveHotspot () != null && KickStarter.runtimeInventory.hoverItem != null)
						{
							interactionMenuIsOn = false;
							menu.TurnOff (true);
						}
						else if (KickStarter.runtimeInventory.selectedItem != null && KickStarter.runtimeInventory.selectedItem != KickStarter.runtimeInventory.hoverItem)
						{
							interactionMenuIsOn = false;
							menu.TurnOff (true);
						}
					}
					else if (KickStarter.stateHandler.gameState == GameState.Paused)
					{
						interactionMenuIsOn = false;
						menu.ForceOff ();
					}
					else if (KickStarter.playerInteraction.GetActiveHotspot () == null)
					{
						interactionMenuIsOn = false;
						menu.TurnOff (true);
					}
				}
			}
			
			else if (menu.appearType == AppearType.WhenSpeechPlays)
			{
				if (KickStarter.stateHandler.gameState == GameState.Paused)
				{
					menu.TurnOff ();
				}
				else
				{
					Speech speech = menu.speech;
					if (!menu.oneMenuPerSpeech)
					{
						speech = KickStarter.dialog.GetLatestSpeech ();
					}

					if (speech != null &&
					   (menu.speechMenuType == SpeechMenuType.All ||
					     (menu.speechMenuType == SpeechMenuType.CharactersOnly && speech.GetSpeakingCharacter () != null) ||
					 	   (menu.speechMenuType == SpeechMenuType.NarrationOnly && speech.GetSpeakingCharacter () == null)) &&
					   (menu.speechMenuLimit == SpeechMenuLimit.All ||
						 (menu.speechMenuLimit == SpeechMenuLimit.BlockingOnly && !speech.isBackground) ||
						   (menu.speechMenuLimit == SpeechMenuLimit.BackgroundOnly && speech.isBackground)))
					{
						if (Options.optionsData == null || (Options.optionsData != null && Options.optionsData.showSubtitles) || (KickStarter.speechManager.forceSubtitles && !KickStarter.dialog.FoundAudio ())) 
						{
							menu.TurnOn (true);
						}
						else
						{
							menu.TurnOff (true);	
						}
					}
					else
					{
						menu.TurnOff (true);
					}
				}
			}

			else if (menu.appearType == AppearType.WhileLoading)
			{
				if (KickStarter.sceneChanger.IsLoading ())
				{
					menu.TurnOn (true);
				}
				else
				{
					menu.TurnOff (true);
				}
			}
		}
		
		
		private void UpdateElements (AC.Menu menu, int languageNumber)
		{
			if (!menu.HasTransition () && menu.IsFading ())
			{
				// Stop until no longer "fading" so that it appears in right place
				return;
			}
			
			if (KickStarter.settingsManager.inputMethod == InputMethod.MouseAndKeyboard && menu.IsPointInside (KickStarter.playerInput.GetInvertedMouse ()))
			{
				elementIdentifier = menu.id.ToString ();
			}

			foreach (MenuElement element in menu.elements)
			{
				if ((element.GetNumSlots () == 0 || !element.isVisible) && menu.menuSource != MenuSource.AdventureCreator)
				{
					element.HideAllUISlots ();
				}

				for (int i=0; i<element.GetNumSlots (); i++)
				{
					if (KickStarter.stateHandler.gameState == GameState.Cutscene)
					{
						element.PreDisplay (i, languageNumber, false);
					}
					else
					{
						element.PreDisplay (i, languageNumber, menu.IsPointerOverSlot (element, i, KickStarter.playerInput.GetInvertedMouse ()));
					}

					if (element.isVisible && SlotIsInteractive (menu, element, i))
					{
						if ((!interactionMenuIsOn || menu.appearType == AppearType.OnInteraction)
						    && (KickStarter.playerInput.GetDragState () == DragState.None || (KickStarter.playerInput.GetDragState () == DragState.Inventory && CanElementBeDroppedOnto (element))))
						{
							if (KickStarter.sceneSettings && element.hoverSound && lastElementIdentifier != (menu.id.ToString () + element.ID.ToString () + i.ToString ()))
							{
								KickStarter.sceneSettings.PlayDefaultSound (element.hoverSound, false);
							}
							
							elementIdentifier = menu.id.ToString () + element.ID.ToString () + i.ToString ();
						}

						if (KickStarter.stateHandler.gameState != GameState.Cutscene)
						{
							if (element is MenuInventoryBox)
							{
								//if (KickStarter.stateHandler.gameState == GameState.Normal)
								if (KickStarter.stateHandler.gameState == GameState.Normal || KickStarter.stateHandler.gameState == GameState.Paused)
								{
									if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Single && KickStarter.runtimeInventory.selectedItem == null)
									{
										KickStarter.playerCursor.ResetSelectedCursor ();
									}
									
									MenuInventoryBox inventoryBox = (MenuInventoryBox) element;
									if (inventoryBox.inventoryBoxType == AC_InventoryBoxType.HotspotBased)
									{
										if (KickStarter.cursorManager.addHotspotPrefix)
										{
											if (KickStarter.runtimeInventory.hoverItem != null)
											{
												hotspotLabel = KickStarter.runtimeInventory.hoverItem.GetLabel (languageNumber);
											}
											else
											{
												hotspotLabel = KickStarter.playerInteraction.GetLabel (languageNumber);
											}
											
											if ((KickStarter.runtimeInventory.selectedItem == null && !interactionMenuIsOn) || interactionMenuIsOn)
											{
												hotspotLabel = KickStarter.runtimeInventory.GetHotspotPrefixLabel (inventoryBox.GetItem (i), inventoryBox.GetLabel (i, languageNumber), languageNumber) + hotspotLabel;
											}
										}
									}
									else
									{
										foundMouseOverInventory = true;

										if (!mouseOverInteractionMenu)
										{
											InvItem newHoverItem = inventoryBox.GetItem (i);
											KickStarter.runtimeInventory.SetHoverItem (newHoverItem, inventoryBox);
											if (oldHoverItem != newHoverItem)
											{
												KickStarter.runtimeInventory.MatchInteractions ();
												KickStarter.playerInteraction.RestoreInventoryInteraction ();
												activeInventoryBoxCentre = menu.GetSlotCentre (inventoryBox, i);

												if (interactionMenuIsOn)
												{
													SetInteractionMenus (false);
												}
											}
										}

										if (KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot)
										{}
										else
										{
											if (!interactionMenuIsOn)
											{
												if (inventoryBox.displayType == ConversationDisplayType.IconOnly)
												{
													if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
													{
														if (KickStarter.playerCursor.GetSelectedCursor () >= 0)
														{
															hotspotLabel = KickStarter.cursorManager.GetCursorIconFromID (KickStarter.playerCursor.GetSelectedCursorID ()).label + " " + inventoryBox.GetLabel (i, languageNumber);
														}
														else if (KickStarter.runtimeInventory.selectedItem == null)
														{
															hotspotLabel = inventoryBox.GetLabel (i, languageNumber);
														}
													}
													else
													{
														if (KickStarter.runtimeInventory.hoverItem != null && KickStarter.runtimeInventory.hoverItem == KickStarter.runtimeInventory.selectedItem)
														{
															hotspotLabel = inventoryBox.GetLabel (i, languageNumber);
														}
													}
												}
											}
											else if (KickStarter.runtimeInventory.selectedItem != null)
											{
												hotspotLabel = KickStarter.runtimeInventory.selectedItem.GetLabel (languageNumber);
											}
										}
									}
								}
							}
							else if (element is MenuCrafting)
							{
								if (KickStarter.stateHandler.gameState == GameState.Normal)
								{
									MenuCrafting crafting = (MenuCrafting) element;
									KickStarter.runtimeInventory.SetHoverItem (crafting.GetItem (i), crafting);
									
									if (KickStarter.runtimeInventory.hoverItem != null)
									{
										if (!interactionMenuIsOn)
										{
											hotspotLabel = crafting.GetLabel (i, languageNumber);
										}
										else if (KickStarter.runtimeInventory.selectedItem != null)
										{
											hotspotLabel = KickStarter.runtimeInventory.selectedItem.GetLabel (languageNumber);
										}
									}
								}
							}
							else if (element is MenuInteraction)
							{
								if (KickStarter.runtimeInventory.hoverItem != null)
								{
									hotspotLabel = KickStarter.runtimeInventory.hoverItem.GetLabel (languageNumber);
								}
								else
								{
									hotspotLabel = KickStarter.playerInteraction.GetLabel (languageNumber);
								}

								if (KickStarter.cursorManager.addHotspotPrefix && interactionMenuIsOn && KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.ClickingMenu)
								{
									MenuInteraction interaction = (MenuInteraction) element;
									hotspotLabel = KickStarter.cursorManager.GetLabelFromID (interaction.iconID, languageNumber) + hotspotLabel;
								}
							}
							else if (element is MenuDialogList)
							{
								if (KickStarter.stateHandler.gameState == GameState.DialogOptions)
								{
									MenuDialogList dialogList = (MenuDialogList) element;
									if (dialogList.displayType == ConversationDisplayType.IconOnly)
									{
										hotspotLabel = dialogList.GetLabel (i, languageNumber);
									}
								}
							}
							else if (element is MenuButton)
							{
								MenuButton button = (MenuButton) element;
								if (button.hotspotLabel != "")
								{
									hotspotLabel = button.GetHotspotLabel (languageNumber);
								}
							}
						}
					}
				}
			}
		}
		
		
		private bool SlotIsInteractive (AC.Menu menu, MenuElement element, int i)
		{
			if (menu.IsVisible () && element.isClickable && 
			    ((KickStarter.settingsManager.inputMethod == InputMethod.MouseAndKeyboard && menu.IsPointerOverSlot (element, i, KickStarter.playerInput.GetInvertedMouse ())) ||
			 (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen && menu.IsPointerOverSlot (element, i, KickStarter.playerInput.GetInvertedMouse ())) ||
			 (KickStarter.settingsManager.inputMethod == InputMethod.KeyboardOrController && KickStarter.stateHandler.gameState == GameState.Normal && menu.IsPointerOverSlot (element, i, KickStarter.playerInput.GetInvertedMouse ())) ||
			 ((KickStarter.settingsManager.inputMethod == InputMethod.KeyboardOrController && KickStarter.stateHandler.gameState != GameState.Normal && menu.selected_element == element && menu.selected_slot == i))))
			{
				return true;
			}
			return false;
		}
		
		
		private void CheckClicks (AC.Menu menu)
		{
			if (!menu.HasTransition () && menu.IsFading ())
			{
				// Stop until no longer "fading" so that it appears in right place
				return;
			}
			
			if (KickStarter.settingsManager.inputMethod == InputMethod.MouseAndKeyboard && menu.IsPointInside (KickStarter.playerInput.GetInvertedMouse ()))
			{
				elementIdentifier = menu.id.ToString ();
			}
			
			foreach (MenuElement element in menu.elements)
			{
				if (element.isVisible)
				{
					for (int i=0; i<element.GetNumSlots (); i++)
					{
						if (SlotIsInteractive (menu, element, i))
						{
							if (!menu.IsUnityUI () && KickStarter.playerInput.GetMouseState () != MouseState.Normal && (KickStarter.playerInput.GetDragState () == DragState.None || KickStarter.playerInput.GetDragState () == DragState.Menu))
							{
								if (KickStarter.playerInput.GetMouseState () == MouseState.SingleClick || KickStarter.playerInput.GetMouseState () == MouseState.LetGo || KickStarter.playerInput.GetMouseState () == MouseState.RightClick)
								{
									if (element is MenuInput) {}
									else DeselectInputBox ();
									
									CheckClick (menu, element, i, KickStarter.playerInput.GetMouseState ());
								}
								else if (KickStarter.playerInput.GetMouseState () == MouseState.HeldDown)
								{
									CheckContinuousClick (menu, element, i, KickStarter.playerInput.GetMouseState ());
								}
							}
							else if (menu.IsUnityUI () && KickStarter.runtimeInventory.selectedItem == null && KickStarter.settingsManager.inventoryDragDrop && KickStarter.playerInput.GetMouseState () == MouseState.HeldDown && KickStarter.playerInput.GetDragState () == DragState.None)
							{
								if (element is MenuInventoryBox || element is MenuCrafting)
								{
									// Begin UI drag drop
									CheckClick (menu, element, i, MouseState.SingleClick);
								}
							}
							else if (KickStarter.playerInteraction.IsDroppingInventory () && CanElementBeDroppedOnto (element))
							{
								if (menu.IsUnityUI () && KickStarter.settingsManager.inventoryDragDrop && (element is MenuInventoryBox || element is MenuCrafting))
								{
									// End UI drag drop
									element.ProcessClick (menu, i, MouseState.SingleClick);
								}
								else
								{
									DeselectInputBox ();
									CheckClick (menu, element, i, MouseState.SingleClick);
								}
							}
						}
					}
				}
			}
		}


		/**
		 * Refreshes any active MenuDialogList elements, after changing the state of dialogue options.
		 */
		public void RefreshDialogueOptions ()
		{
			foreach (Menu menu in menus)
			{
				menu.RefreshDialogueOptions ();
			}
		}


		/**
		 * Updates the state of all Menus set to appear while the game is loading.
		 */
		public void UpdateLoadingMenus ()
		{
			int languageNumber = Options.GetLanguage ();
			foreach (AC.Menu menu in menus)
			{
				if (menu.appearType == AppearType.WhileLoading)
				{
					UpdateMenu (menu);
					if (menu.IsEnabled ())
					{
						UpdateElements (menu, languageNumber);
					}
				}
			}
		}
		

		/**
		 * Updates the state of all Menus.
		 * This is called every frame by StateHandler.
		 */
		public void UpdateAllMenus ()
		{
			#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
			if (keyboard != null && selectedInputBox != null)
			{
				selectedInputBox.label = keyboard.text;
			}
			#endif
			
			if (doResizeMenus > 0)
			{
				doResizeMenus ++;
				
				if (doResizeMenus == 4)
				{
					doResizeMenus = 0;
					foreach (AC.Menu menu in PlayerMenus.GetMenus ())
					{
						menu.Recalculate ();
						menu.UpdateAspectRect ();
						KickStarter.mainCamera.SetCameraRect ();
						menu.Recalculate ();
					}
				}
			}
			
			if (Time.time > 0f)
			{
				int languageNumber = Options.GetLanguage ();
				hotspotLabel = KickStarter.playerInteraction.GetLabel (languageNumber);

				if (!interactionMenuIsOn || !mouseOverInteractionMenu)
				{
					oldHoverItem = KickStarter.runtimeInventory.hoverItem;
					KickStarter.runtimeInventory.hoverItem = null;
				}
				
				if (KickStarter.stateHandler.gameState == GameState.Paused)
				{
					if (Time.timeScale != 0f)
					{
						KickStarter.sceneSettings.PauseGame ();
					}
				}
				/*else if (Time.timeScale == 0f)
				{
					KickStarter.sceneSettings.UnpauseGame (KickStarter.playerInput.timeScale);
				}*/
				
				foundMouseOverMenu = false;
				foundMouseOverInteractionMenu = false;
				foundMouseOverInventory = false;
				
				foreach (AC.Menu menu in menus)
				{
					UpdateMenu (menu);
					if (menu.IsEnabled ())
					{
						UpdateElements (menu, languageNumber);
					}
				}

				for (int i=0; i<dupMenus.Count; i++)
				{
					UpdateMenu (dupMenus[i]);
					UpdateElements (dupMenus[i], languageNumber);

					if (dupMenus[i].IsOff () && KickStarter.stateHandler.gameState != GameState.Paused)
					{
						Menu oldMenu = dupMenus[i];
						dupMenus.RemoveAt (i);
						if (oldMenu.menuSource != MenuSource.AdventureCreator && oldMenu.canvas && oldMenu.canvas.gameObject.activeInHierarchy)
						{
							DestroyImmediate (oldMenu.canvas.gameObject);
						}
						DestroyImmediate (oldMenu);
						i=0;
					}

				}

				mouseOverMenu = foundMouseOverMenu;
				mouseOverInteractionMenu = foundMouseOverInteractionMenu;
				mouseOverInventory = foundMouseOverInventory;

				lastElementIdentifier = elementIdentifier;
				
				// Check clicks in reverse order
				for (int i=menus.Count-1; i>=0; i--)
				{
					if (menus[i].IsEnabled () && !menus[i].ignoreMouseClicks/* && !menus[i].IsUnityUI ()*/)
					{
						CheckClicks (menus[i]);
					}
				}
			}
		}
		

		/**
		 * <summary>Begins fading in the second Menu in a crossfade if the first Menu matches the supplied parameter.</summary>
		 * <param name = "_menu">The Menu to check for. If this menu is crossfading out, then it will be turned off, and the second Menu will fade in</param>
		 */
		public void CheckCrossfade (AC.Menu _menu)
		{
			if (crossFadeFrom == _menu && crossFadeTo != null)
			{
				crossFadeFrom.ForceOff ();
				crossFadeTo.TurnOn (true);
				crossFadeTo = null;
			}
		}
		

		/**
		 * <summary>Selects a MenuInput element, allowing the player to enter text into it.</summary>
		 * <param name = "input">The input box to select</param>
		 */
		public void SelectInputBox (MenuInput input)
		{
			selectedInputBox = input;
			
			// Mobile keyboard
			#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
			if (input.inputType == AC_InputType.NumbericOnly)
			{
				keyboard = TouchScreenKeyboard.Open (input.label, TouchScreenKeyboardType.NumberPad, false, false, false, false, "");
			}
			else
			{
				keyboard = TouchScreenKeyboard.Open (input.label, TouchScreenKeyboardType.ASCIICapable, false, false, false, false, "");
			}
			#endif
		}
		
		
		private void DeselectInputBox ()
		{
			if (selectedInputBox)
			{
				selectedInputBox.Deselect ();
				selectedInputBox = null;
				
				// Mobile keyboard
				#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
				if (keyboard != null)
				{
					keyboard.active = false;
					keyboard = null;
				}
				#endif
			}
		}
		
		
		private void CheckClick (AC.Menu _menu, MenuElement _element, int _slot, MouseState _mouseState)
		{
			KickStarter.playerInput.ResetMouseClick ();

			if (_mouseState == MouseState.LetGo)
			{
				if (_menu.appearType == AppearType.OnInteraction && KickStarter.settingsManager.ReleaseClickInteractions () && !KickStarter.settingsManager.CanDragCursor () && KickStarter.runtimeInventory.selectedItem == null)
				{
					_mouseState = MouseState.SingleClick;
				}
				else if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen && !KickStarter.settingsManager.CanDragCursor () && KickStarter.runtimeInventory.selectedItem == null && !(_element is MenuInventoryBox) && !(_element is MenuCrafting))
				{
					_mouseState = MouseState.SingleClick;
				}
				else
				{
					_mouseState = MouseState.Normal;
					return;
				}
			}
			
			if (_mouseState != MouseState.Normal)
			{
				_element.ProcessClick (_menu, _slot, _mouseState);
				PlayerMenus.ResetInventoryBoxes ();
			}
		}
		
		
		private void CheckContinuousClick (AC.Menu _menu, MenuElement _element, int _slot, MouseState _mouseState)
		{
			_element.ProcessContinuousClick (_menu, _mouseState);
		}


		/**
		 * <summary>Unassigns a Speech line from any temporarily-duplicated Menus. This will signal such Menus that they can be removed.</summary>
		 * <param name = "speech">The Speech line to unassign</param>
		 */
		public void RemoveSpeechFromMenu (Speech speech)
		{
			foreach (Menu menu in dupMenus)
			{
				if (menu.speech == speech)
				{
					menu.speech = null;
				}
			}
		}


		/**
		 * <summary>Duplicates any Menu set to display a single speech line.</summary>
		 * <param name = "speech">The Speech line to assign to any duplicated Menu</param>
		 */
		public void AssignSpeechToMenu (Speech speech)
		{
			foreach (Menu menu in menus)
			{
				if (menu.appearType == AppearType.WhenSpeechPlays && menu.oneMenuPerSpeech)
				{
					Menu dupMenu = ScriptableObject.CreateInstance <Menu>();
					dupMenu.Copy (menu);
					if (dupMenu.IsUnityUI ())
					{
						dupMenu.LoadUnityUI ();
					}
					dupMenu.title += " (Duplicate)";
					dupMenu.SetSpeech (speech);
					dupMenu.TurnOn (true);
					dupMenus.Add (dupMenu);
				}
			}
		}
		

		/**
		 * <summary>Crossfades to a Menu. Any other Menus will be turned off.</summary>
		 * <param name = "_menuTo">The Menu to crossfade to</param>
		 */
		public void CrossFade (AC.Menu _menuTo)
		{
			if (_menuTo.isLocked)
			{
				ACDebug.Log ("Cannot crossfade to menu " + _menuTo.title + " as it is locked.");
			}
			else if (!_menuTo.IsEnabled())
			{
				// Turn off all other menus
				crossFadeFrom = null;
				
				foreach (AC.Menu menu in menus)
				{
					if (menu.IsVisible ())
					{
						if (menu.appearType == AppearType.OnHotspot || menu.fadeSpeed == 0 || !menu.HasTransition ())
						{
							menu.ForceOff ();
						}
						else
						{
							menu.TurnOff (true);
							crossFadeFrom = menu;
						}
					}
					else
					{
						menu.ForceOff ();
					}
				}
				
				if (crossFadeFrom != null)
				{
					crossFadeTo = _menuTo;
				}
				else
				{
					_menuTo.TurnOn (true);
				}
			}
		}
		

		/**
		 * <summary>Shows or hides any Menus with appearType = AppearType.OnInteraction.</summary>
		 * <param name = "turnOn">If True, such Menus will be enabled. If False, they will be disabled.</param>
		 */
		public void SetInteractionMenus (bool turnOn)
		{
			interactionMenuIsOn = turnOn;

			foreach (AC.Menu _menu in menus)
			{
				if (_menu.appearType == AppearType.OnInteraction)
				{
					if (turnOn)
					{
						StopCoroutine ("SwapInteractionMenu");
						StartCoroutine ("SwapInteractionMenu", _menu);
					}
					else
					{
						_menu.TurnOff (true);
					}
				}
			}
		}


		private IEnumerator SwapInteractionMenu (Menu _menu)
		{
			_menu.TurnOff (true);

			while (_menu.IsFading ())
			{
				yield return new WaitForFixedUpdate ();
			}

			KickStarter.playerInteraction.ResetInteractionIndex ();
			if (KickStarter.runtimeInventory.hoverItem != null)
			{
				_menu.MatchInteractions (KickStarter.runtimeInventory.hoverItem, KickStarter.settingsManager.cycleInventoryCursors);
			}
			else if (KickStarter.playerInteraction.GetActiveHotspot ())
			{
				_menu.MatchInteractions (KickStarter.playerInteraction.GetActiveHotspot ().useButtons, KickStarter.settingsManager.cycleInventoryCursors);
			}

			_menu.TurnOn (true);
		}
		

		/**
		 * Turns off any Menus with appearType = AppearType.OnHotspot.
		 */
		public void DisableHotspotMenus ()
		{
			foreach (AC.Menu _menu in menus)
			{
				if (_menu.appearType == AppearType.OnHotspot)
				{
					_menu.ForceOff ();
				}
			}
		}
		

		/**
		 * <summary>Gets the complete Hotspot label to be displayed in a MenuLabel element with labelType = AC_LabelType.Hotspot.</summary>
		 * <returns>The complete Hotspot label to be displayed in a MenuLabel element with labelType = AC_LabelType.Hotspot</returns>
		 */
		public string GetHotspotLabel ()
		{
			return hotspotLabel;
		}
		
		
		private void SetStyles (MenuElement element)
		{
			normalStyle.normal.textColor = element.fontColor;
			normalStyle.font = element.font;
			normalStyle.fontSize = element.GetFontSize ();
			normalStyle.alignment = TextAnchor.MiddleCenter;
			
			highlightedStyle.font = element.font;
			highlightedStyle.fontSize = element.GetFontSize ();
			highlightedStyle.normal.textColor = element.fontHighlightColor;
			highlightedStyle.normal.background = element.highlightTexture;
			highlightedStyle.alignment = TextAnchor.MiddleCenter;
		}
		
		
		private bool CanElementBeDroppedOnto (MenuElement element)
		{
			if (element is MenuInventoryBox)
			{
				MenuInventoryBox inventoryBox = (MenuInventoryBox) element;
				if (inventoryBox.inventoryBoxType == AC_InventoryBoxType.Default || inventoryBox.inventoryBoxType == AC_InventoryBoxType.Container || inventoryBox.inventoryBoxType == AC_InventoryBoxType.CustomScript)
				{
					return true;
				}
			}
			else if (element is MenuCrafting)
			{
				MenuCrafting crafting = (MenuCrafting) element;
				if (crafting.craftingType == CraftingElementType.Ingredients)
				{
					return true;
				}
			}
			
			return false;
		}
		
		
		private void OnDestroy ()
		{
			menus = null;
		}
		

		/**
		 * <summary>Gets a List of all defined Menus.</summary>
		 * <returns>A List of all defined Menus</returns>
		 */
		public static List<Menu> GetMenus ()
		{
			if (KickStarter.playerMenus)
			{
				if (KickStarter.playerMenus.menus.Count == 0 && KickStarter.menuManager != null && KickStarter.menuManager.menus.Count > 0)
				{
					ACDebug.LogError ("A custom script is calling 'PlayerMenus.GetMenus ()' before the Menus have been initialised - consider adjusting your script's Script Execution Order.");
					return null;
				}

				return KickStarter.playerMenus.menus;
			}
			return null;
		}
		

		/**
		 * <summary>Gets a Menu with a specific name.</summary>
		 * <param name = "menuName">The name (title) of the Menu to find</param>
		 * <returns>The Menu with the specific name</returns>
		 */
		public static Menu GetMenuWithName (string menuName)
		{
			if (KickStarter.playerMenus && KickStarter.playerMenus.menus != null)
			{
				if (KickStarter.playerMenus.menus.Count == 0 && KickStarter.menuManager != null && KickStarter.menuManager.menus.Count > 0)
				{
					ACDebug.LogError ("A custom script is calling 'PlayerMenus.GetMenuWithName ()' before the Menus have been initialised - consider adjusting your script's Script Execution Order.");
					return null;
				}

				foreach (AC.Menu menu in KickStarter.playerMenus.menus)
				{
					if (menu.title == menuName)
					{
						return menu;
					}
				}
			}
			return null;
		}
		

		/**
		 * <summary>Gets a MenuElement with a specific name.</summary>
		 * <param name = "menuName">The name (title) of the Menu to find</param>
		 * <param name = "menuElementName">The name (title) of the MenuElement with the Menu to find</param>
		 * <returns>The MenuElement with the specific name</returns>
		 */
		public static MenuElement GetElementWithName (string menuName, string menuElementName)
		{
			if (KickStarter.playerMenus && KickStarter.playerMenus.menus != null)
			{
				if (KickStarter.playerMenus.menus.Count == 0 && KickStarter.menuManager != null && KickStarter.menuManager.menus.Count > 0)
				{
					ACDebug.LogError ("A custom script is calling 'PlayerMenus.GetElementWithName ()' before the Menus have been initialised - consider adjusting your script's Script Execution Order.");
					return null;
				}

				foreach (AC.Menu menu in KickStarter.playerMenus.menus)
				{
					if (menu.title == menuName)
					{
						foreach (MenuElement menuElement in menu.elements)
						{
							if (menuElement.title == menuElementName)
							{
								return menuElement;
							}
						}
					}
				}
			}
			
			return null;
		}
		

		/**
		 * <summary>Checks if saving cannot be performed at this time.</summary>
		 * <param title = "_actionToIgnore">Any gameplay-blocking ActionList that contains this Action will be excluded from the check</param>
		 * <returns>True if saving cannot be performed at this time</returns>
		 */
		public static bool IsSavingLocked (Action _actionToIgnore = null)
		{
			if (KickStarter.stateHandler.gameState == GameState.DialogOptions)
			{
				return true;
			}

			if (KickStarter.actionListManager.IsGameplayBlocked (_actionToIgnore))
			{
				return true;
			}

			return KickStarter.playerMenus.lockSave;
		}
		

		/**
		 * Calls RecalculateSize() on all MenuInventoryBox elements.
		 */
		public static void ResetInventoryBoxes ()
		{
			if (KickStarter.playerMenus)
			{
				foreach (AC.Menu menu in KickStarter.playerMenus.menus)
				{
					foreach (MenuElement menuElement in menu.elements)
					{
						if (menuElement is MenuInventoryBox)
						{
							menuElement.RecalculateSize (menu.menuSource);
						}
					}
				}
			}
		}
		

		/**
		 * Takes the ingredients supplied to a MenuCrafting element and sets the appropriate outcome of another MenuCrafting element with craftingType = CraftingElementType.Output.
		 */
		public static void CreateRecipe ()
		{
			if (KickStarter.playerMenus)
			{
				foreach (AC.Menu menu in KickStarter.playerMenus.menus)
				{
					foreach (MenuElement menuElement in menu.elements)
					{
						if (menuElement is MenuCrafting)
						{
							MenuCrafting crafting = (MenuCrafting) menuElement;
							crafting.SetOutput (menu.menuSource, false);
						}
					}
				}
			}
		}
		

		/**
		 * <summary>Instantly turns off all Menus.</summary>
		 * <param name = "onlyPausing">If True, then only Menus with pauseWhenEnabled = True will be turned off</param>
		 */
		public static void ForceOffAllMenus (bool onlyPausing = false)
		{
			if (KickStarter.playerMenus)
			{
				foreach (AC.Menu menu in KickStarter.playerMenus.menus)
				{
					if (menu.IsEnabled ())
					{
						if (!onlyPausing || (onlyPausing && menu.pauseWhenEnabled))
						{
							menu.ForceOff ();
						}
					}
				}
			}
		}
		

		/**
		 * <summary>Simulates the clicking of a MenuElement.</summary>
		 * <param name = "menuName">The name (title) of the Menu that contains the MenuElement</param>
		 * <param name = "menuElementName">The name (title) of the MenuElement</param>
		 * <param name = "slot">The index number of the slot, if the MenuElement has multiple slots</param>
		 */
		public static void SimulateClick (string menuName, string menuElementName, int slot = 1)
		{
			if (KickStarter.playerMenus)
			{
				AC.Menu menu = PlayerMenus.GetMenuWithName (menuName);
				MenuElement element = PlayerMenus.GetElementWithName (menuName, menuElementName);
				KickStarter.playerMenus.CheckClick (menu, element, slot, MouseState.SingleClick);
			}
		}
		

		/**
		 * <summary>Simulates the clicking of a MenuElement.</summary>
		 * <param name = "menuName">The name (title) of the Menu that contains the MenuElement</param>
		 * <param name = "_element">The MenuElement</param>
		 * <param name = "slot">The index number of the slot, if the MenuElement has multiple slots</param>
		 */
		public static void SimulateClick (string menuName, MenuElement _element, int _slot = 1)
		{
			if (KickStarter.playerMenus)
			{
				AC.Menu menu = PlayerMenus.GetMenuWithName (menuName);
				KickStarter.playerMenus.CheckClick (menu, _element, _slot, MouseState.SingleClick);
			}
		}
		

		/**
		 * <summary>Checks if any Menus that pause the game are currently turned on.</summary>
		 * <param name ="excludingMenu">If assigned, this Menu will be excluded from the check</param>
		 * <returns>True if any Menus that pause the game are currently turned on</returns>
		 */
		public bool ArePauseMenusOn (Menu excludingMenu)
		{
			foreach (AC.Menu menu in menus)
			{
				if (menu.IsEnabled () && menu.IsBlocking () && menu != excludingMenu)
				{
					return true;
				}
			}
			return false;
		}
		

		/**
		 * Instantly turns off all Menus that have appearType = AppearType.WhenSpeechPlays.
		 */
		public void ForceOffSubtitles ()
		{
			foreach (AC.Menu menu in menus)
			{
				if (menu.IsEnabled () && menu.appearType == AppearType.WhenSpeechPlays)
				{
					menu.ForceOff ();
				}
			}
		}
		

		/**
		 * Recalculates the position, size and display of all Menus.
		 * This is an intensive process, and should not be called every fame.
		 */
		public void RecalculateAll ()
		{
			doResizeMenus = 1;

			// Border camera
			if (KickStarter.mainCamera)
			{
				KickStarter.mainCamera.SetCameraRect ();
			}
		}


		/**
		 * Instantly turns off all Menus that contain a MenuSaveList with savesListType = AC_SavesListType.Save.
		 */
		public void HideSaveMenus ()
		{
			foreach (AC.Menu menu in menus)
			{
				foreach (MenuElement element in menu.elements)
				{
					if (element is MenuSavesList && menu.IsManualControlled ())
					{
						MenuSavesList saveList = (MenuSavesList) element;
						if (saveList.saveListType == AC_SaveListType.Save)
						{
							menu.ForceOff ();
							break;
						}
					}
				}
			}
		}


		/**
		 * Selects the first element GameObject in a Unity UI-based Menu.
		 */
		public void FindFirstSelectedElement ()
		{
			if (eventSystem == null || menus.Count == 0)
			{
				return;
			}

			GameObject objectToSelect = null;
			for (int i=menus.Count-1; i>=0; i--)
			{
				Menu menu = menus[i];

				if (menu.IsEnabled ())
				{
					objectToSelect = menu.GetObjectToSelect ();
					if (objectToSelect != null)
					{
						break;
					}
				}
			}

			eventSystem.SetSelectedGameObject (objectToSelect);
		}


		/**
		 * <summary>Gets the ID number of the CursorIcon, defined in CursorManager, to switch to based on what MenuElement the cursor is currently over</summary>
		 * <returns>The ID number of the CursorIcon, defined in CursorManager, to switch to based on what MenuElement the cursor is currently over</returns>
		 */
		public int GetElementOverCursorID ()
		{
			return elementOverCursorID;
		}


		/**
		 * <summary>Sets the state of the manual save lock.</summary>
		 * <param name = "state">If True, then saving will be manually disabled</param>
		 */
		public void SetManualSaveLock (bool state)
		{
			lockSave = state;
		}


		/**
		 * <summary>Checks if the cursor is hovering over a Menu.</summary>
		 * <returns>True if the cursor is hovering over a Menu</returns>
		 */
		public bool IsMouseOverMenu ()
		{
			return mouseOverMenu;
		}


		/**
		 * <summary>Checks if the cursor is hovering over a Menu with appearType = AppearType.OnInteraction.</summary>
		 * <returns>True if the cursor is hovering over a Menu with appearType = AppearType.OnInteraction.</returns>
		 */
		public bool IsMouseOverInteractionMenu ()
		{
			return mouseOverInteractionMenu;
		}

		/**
		 * <summary>Checks if any Menu with appearType = AppearType.OnInteraction is on.</summary>
		 * <returns>True if any Menu with appearType = AppearType.OnInteraction is on.</returns>
		 */
		public bool IsInteractionMenuOn ()
		{
			return interactionMenuIsOn;
		}


		/**
		 * Makes all Menus linked to Unity UI interactive.
		 */
		public void MakeUIInteractive ()
		{
			foreach (Menu menu in menus)
			{
				menu.MakeUIInteractive ();
			}
		}
		
		
		/**
		 * Makes all Menus linked to Unity UI non-interactive.
		 */
		public void MakeUINonInteractive ()
		{
			foreach (Menu menu in menus)
			{
				menu.MakeUINonInteractive ();
			}
		}


		/**
		 * <summary>Updates a MainData class with it's own variables that need saving.</summary>
		 * <param name = "mainData">The original MainData class</param>
		 * <returns>The updated MainData class</returns>
		 */
		public MainData SaveMainData (MainData mainData)
		{
			mainData.menuLockData = CreateMenuLockData ();
			mainData.menuVisibilityData = CreateMenuVisibilityData ();
			mainData.menuElementVisibilityData = CreateMenuElementVisibilityData ();
			mainData.menuJournalData = CreateMenuJournalData ();

			return mainData;
		}
		
		
		/**
		 * <summary>Updates it's own variables from a MainData class.</summary>
		 * <param name = "mainData">The MainData class to load from</param>
		 */
		public void LoadMainData (MainData mainData)
		{
			foreach (Menu menu in menus)
			{
				foreach (MenuElement element in menu.elements)
				{
					if (element is MenuInventoryBox)
					{
						MenuInventoryBox invBox = (MenuInventoryBox) element;
						invBox.ResetOffset ();
					}
				}
			}
			
			AssignMenuLocks (mainData.menuLockData);
			AssignMenuVisibility (mainData.menuVisibilityData);
			AssignMenuElementVisibility ( mainData.menuElementVisibilityData);
			AssignMenuJournals (mainData.menuJournalData);
		}


		private string CreateMenuLockData ()
		{
			System.Text.StringBuilder menuString = new System.Text.StringBuilder ();
			
			foreach (AC.Menu _menu in menus)
			{
				menuString.Append (_menu.id.ToString ());
				menuString.Append (":");
				menuString.Append (_menu.isLocked.ToString ());
				menuString.Append ("|");
			}
			
			if (menus.Count > 0)
			{
				menuString.Remove (menuString.Length-1, 1);
			}
			
			return menuString.ToString ();
		}
		
		
		private string CreateMenuVisibilityData ()
		{
			System.Text.StringBuilder menuString = new System.Text.StringBuilder ();
			bool changeMade = false;
			foreach (AC.Menu _menu in menus)
			{
				if (_menu.IsManualControlled ())
				{
					changeMade = true;
					menuString.Append (_menu.id.ToString ());
					menuString.Append (":");
					menuString.Append (_menu.IsEnabled ().ToString ());
					menuString.Append ("|");
				}
			}
			
			if (changeMade)
			{
				menuString.Remove (menuString.Length-1, 1);
			}
			return menuString.ToString ();
		}
		
		
		private string CreateMenuElementVisibilityData ()
		{
			System.Text.StringBuilder visibilityString = new System.Text.StringBuilder ();
			
			foreach (AC.Menu _menu in menus)
			{
				if (_menu.elements.Count > 0)
				{
					visibilityString.Append (_menu.id.ToString ());
					visibilityString.Append (":");
					
					foreach (MenuElement _element in _menu.elements)
					{
						visibilityString.Append (_element.ID.ToString ());
						visibilityString.Append ("=");
						visibilityString.Append (_element.isVisible.ToString ());
						visibilityString.Append ("+");
					}
					
					visibilityString.Remove (visibilityString.Length-1, 1);
					visibilityString.Append ("|");
				}
			}
			
			if (menus.Count > 0)
			{
				visibilityString.Remove (visibilityString.Length-1, 1);
			}
			
			return visibilityString.ToString ();
		}
		
		
		private string CreateMenuJournalData ()
		{
			System.Text.StringBuilder journalString = new System.Text.StringBuilder ();
			
			foreach (AC.Menu _menu in menus)
			{
				foreach (MenuElement _element in _menu.elements)
				{
					if (_element is MenuJournal)
					{
						MenuJournal journal = (MenuJournal) _element;
						journalString.Append (_menu.id.ToString ());
						journalString.Append (":");
						journalString.Append (journal.ID);
						journalString.Append (":");
						
						foreach (JournalPage page in journal.pages)
						{
							journalString.Append (page.lineID);
							journalString.Append ("*");
							journalString.Append (page.text);
							journalString.Append ("~");
						}
						
						if (journal.pages.Count > 0)
						{
							journalString.Remove (journalString.Length-1, 1);
						}
						
						journalString.Append ("|");
					}
				}
			}
			
			if (journalString.ToString () != "")
			{
				journalString.Remove (journalString.Length-1, 1);
			}
			
			return journalString.ToString ();
		}

		private void AssignMenuLocks (string menuLockData)
		{
			if (menuLockData.Length > 0)
			{
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
		}
		
		
		private void AssignMenuVisibility (string menuVisibilityData)
		{
			if (menuVisibilityData.Length > 0)
			{
				string[] visArray = menuVisibilityData.Split ("|"[0]);
				
				foreach (string chunk in visArray)
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
							if (_menu.IsManualControlled ())
							{
								if (_lock)
								{
									_menu.TurnOn (false);
								}
								else
								{
									_menu.TurnOff (false);
								}
							}
							break;
						}
					}
				}
			}
		}
		
		
		private void AssignMenuElementVisibility (string menuElementVisibilityData)
		{
			if (menuElementVisibilityData.Length > 0)
			{
				string[] visArray = menuElementVisibilityData.Split ("|"[0]);
				
				foreach (string chunk in visArray)
				{
					string[] chunkData = chunk.Split (":"[0]);
					
					int _menuID = 0;
					int.TryParse (chunkData[0], out _menuID);
					
					foreach (AC.Menu _menu in menus)
					{
						if (_menu.id == _menuID)
						{
							// Found a match
							string[] perMenuData = chunkData[1].Split ("+"[0]);
							
							foreach (string perElementData in perMenuData)
							{
								string [] chunkData2 = perElementData.Split ("="[0]);
								
								int _elementID = 0;
								int.TryParse (chunkData2[0], out _elementID);
								
								bool _elementVisibility = false;
								bool.TryParse (chunkData2[1], out _elementVisibility);
								
								foreach (MenuElement _element in _menu.elements)
								{
									if (_element.ID == _elementID && _element.isVisible != _elementVisibility)
									{
										_element.isVisible = _elementVisibility;
										break;
									}
								}
							}
							
							_menu.ResetVisibleElements ();
							_menu.Recalculate ();
							break;
						}
					}
				}
			}
		}
		
		
		private void AssignMenuJournals (string menuJournalData)
		{
			if (menuJournalData.Length > 0)
			{
				string[] journalArray = menuJournalData.Split ("|"[0]);
				
				foreach (string chunk in journalArray)
				{
					string[] chunkData = chunk.Split (":"[0]);
					
					int menuID = 0;
					int.TryParse (chunkData[0], out menuID);
					
					int elementID = 0;
					int.TryParse (chunkData[1], out elementID);
					
					foreach (AC.Menu _menu in menus)
					{
						if (_menu.id == menuID)
						{
							foreach (MenuElement _element in _menu.elements)
							{
								if (_element.ID == elementID && _element is MenuJournal)
								{
									MenuJournal journal = (MenuJournal) _element;
									journal.pages = new List<JournalPage>();
									journal.showPage = 1;
									
									string[] pageArray = chunkData[2].Split ("~"[0]);
									
									foreach (string chunkData2 in pageArray)
									{
										string[] chunkData3 = chunkData2.Split ("*"[0]);
										
										int lineID = -1;
										int.TryParse (chunkData3[0], out lineID);
										
										journal.pages.Add (new JournalPage (lineID, chunkData3[1]));
									}
									
									break;
								}
							}
						}
					}
				}
			}
		}

	}
	
}