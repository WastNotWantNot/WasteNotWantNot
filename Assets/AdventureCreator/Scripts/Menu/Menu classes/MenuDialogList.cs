/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"MenuDialogList.cs"
 * 
 *	This MenuElement lists the available options of the active conversation,
 *	and runs them when clicked on.
 * 
 */

using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;	
#endif

namespace AC
{

	/**
	 * A MenuElement that lists the available options in the active Conversation, and runs their interactions when clicked on.
	 */
	public class MenuDialogList : MenuElement
	{

		/** A List of UISlot classes that reference the linked Unity UI GameObjects (Unity UI Menus only) */
		public UISlot[] uiSlots;
		/** The special FX applied to the text (None, Outline, Shadow, OutlineAndShadow) */
		public TextEffects textEffects;
		/** How the Conversation's dialogue options are displayed (IconOnly, TextOnly) */
		public ConversationDisplayType displayType = ConversationDisplayType.TextOnly;
		/** A temporary dialogue option icon, used for test purposes when the game is not running */
		public Texture2D testIcon = null;
		/** The text alignment */
		public TextAnchor anchor;
		/** If True, then only one dialogue option will be shown */
		public bool fixedOption;
		/** The index number of the dialogue option to show, if fixedOption = true */
		public int optionToShow;
		/** The maximum number of dialogue options that can be shown at once */
		public int maxSlots = 10;
		/** If True, then options that have already been clicked can be displayed in a different colour */
		public bool markAlreadyChosen = false;
		/** The font colour for options already chosen (If markAlreadyChosen = True, OnGUI only) */
		public Color alreadyChosenFontColour = Color.white;
		
		private int numOptions = 0;
		private string[] labels = null;
		private Texture2D[] icons;
		private bool[] chosens = null;


		/**
		 * Initialises the element when it is created within MenuManager.
		 */
		public override void Declare ()
		{
			uiSlots = null;

			isVisible = true;
			isClickable = true;
			fixedOption = false;
			displayType = ConversationDisplayType.TextOnly;
			testIcon = null;
			optionToShow = 1;
			numSlots = 0;
			SetSize (new Vector2 (20f, 5f));
			maxSlots = 10;
			anchor = TextAnchor.MiddleLeft;
			textEffects = TextEffects.None;
			markAlreadyChosen = false;
			alreadyChosenFontColour = Color.white;

			base.Declare ();
		}


		/**
		 * <summary>Creates and returns a new MenuCrafting that has the same values as itself.</summary>
		 * <returns>A new MenuCrafting with the same values as itself</returns>
		 */
		public override MenuElement DuplicateSelf ()
		{
			MenuDialogList newElement = CreateInstance <MenuDialogList>();
			newElement.Declare ();
			newElement.CopyDialogList (this);
			return newElement;
		}
		
		
		private void CopyDialogList (MenuDialogList _element)
		{
			uiSlots = _element.uiSlots;

			textEffects = _element.textEffects;
			displayType = _element.displayType;
			testIcon = _element.testIcon;
			anchor = _element.anchor;
			labels = _element.labels;
			fixedOption = _element.fixedOption;
			optionToShow = _element.optionToShow;
			maxSlots = _element.maxSlots;
			markAlreadyChosen = _element.markAlreadyChosen;
			alreadyChosenFontColour = _element.alreadyChosenFontColour;

			base.Copy (_element);
		}


		/**
		 * Hides all linked Unity UI GameObjects associated with the element.
		 */
		public override void HideAllUISlots ()
		{
			LimitUISlotVisibility (uiSlots, 0);
		}


		/**
		 * <summary>Initialises the linked Unity UI GameObjects.</summary>
		 * <param name = "_menu">The element's parent Menu</param>
		 */
		public override void LoadUnityUI (AC.Menu _menu)
		{
			int i=0;
			foreach (UISlot uiSlot in uiSlots)
			{
				uiSlot.LinkUIElements ();
				if (uiSlot != null && uiSlot.uiButton != null)
				{
					int j=i;
					uiSlot.uiButton.onClick.AddListener (() => {
						ProcessClick (_menu, j, KickStarter.playerInput.GetMouseState ());
					});
				}
				i++;
			}
		}


		/**
		 * <summary>Gets the first linked Unity UI GameObject associated with this element.</summary>
		 * <param name = "The first Unity UI GameObject associated with the element</param>
		 */
		public override GameObject GetObjectToSelect ()
		{
			if (uiSlots != null && uiSlots.Length > 0 && uiSlots[0].uiButton != null)
			{
				return uiSlots[0].uiButton.gameObject;
			}
			return null;
		}
		

		/**
		 * <summary>Gets the boundary of a slot</summary>
		 * <param name = "_slot">The index number of the slot to get the boundary of</param>
		 * <returns>The boundary Rect of the slot</returns>
		 */
		public override RectTransform GetRectTransform (int _slot)
		{
			if (uiSlots != null && uiSlots.Length > _slot)
			{
				return uiSlots[_slot].GetRectTransform ();
			}
			return null;
		}


		public override void SetUIInteractableState (bool state)
		{
			SetUISlotsInteractableState (uiSlots, state);
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI (MenuSource source)
		{
			EditorGUILayout.BeginVertical ("Button");
			fixedOption = EditorGUILayout.Toggle ("Fixed option number?", fixedOption);
			if (fixedOption)
			{
				numSlots = 1;
				slotSpacing = 0f;
				optionToShow = EditorGUILayout.IntSlider ("Option to display:", optionToShow, 1, 10);
			}
			else
			{
				maxSlots = EditorGUILayout.IntField ("Maximum no. of slots:", maxSlots);

				if (source == MenuSource.AdventureCreator)
				{
					numSlots = EditorGUILayout.IntSlider ("Test slots:", numSlots, 1, maxSlots);
					slotSpacing = EditorGUILayout.Slider ("Slot spacing:", slotSpacing, 0f, 20f);
					orientation = (ElementOrientation) EditorGUILayout.EnumPopup ("Slot orientation:", orientation);
					if (orientation == ElementOrientation.Grid)
					{
						gridWidth = EditorGUILayout.IntSlider ("Grid size:", gridWidth, 1, 10);
					}
				}
			}
			displayType = (ConversationDisplayType) EditorGUILayout.EnumPopup ("Display type:", displayType);

			markAlreadyChosen = EditorGUILayout.Toggle ("Mark options already used?", markAlreadyChosen);
			if (markAlreadyChosen)
			{
				alreadyChosenFontColour = (Color) EditorGUILayout.ColorField ("'Already chosen' colour:", alreadyChosenFontColour);
			}

			if (source == MenuSource.AdventureCreator)
			{
				if (displayType == ConversationDisplayType.IconOnly)
				{
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Test icon:", GUILayout.Width (145f));
					testIcon = (Texture2D) EditorGUILayout.ObjectField (testIcon, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
					EditorGUILayout.EndHorizontal ();
				}
				else
				{
					anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
					textEffects = (TextEffects) EditorGUILayout.EnumPopup ("Text effect:", textEffects);
				}
			}
			else
			{
				EditorGUILayout.EndVertical ();
				EditorGUILayout.BeginVertical ("Button");
				uiHideStyle = (UIHideStyle) EditorGUILayout.EnumPopup ("When invisible:", uiHideStyle);
				EditorGUILayout.LabelField ("Linked button objects", EditorStyles.boldLabel);

				if (fixedOption)
				{
					uiSlots = ResizeUISlots (uiSlots, 1);
				}
				else
				{
					uiSlots = ResizeUISlots (uiSlots, maxSlots);
				}

				for (int i=0; i<uiSlots.Length; i++)
				{
					uiSlots[i].LinkedUiGUI (i, source);
				}
			}

			ChangeCursorGUI (source);
			EditorGUILayout.EndVertical ();
			
			base.ShowGUI (source);
		}
		
		#endif
		

		/**
		 * <summary>Performs all calculations necessary to display the element.</summary>
		 * <param name = "_slot">The index number of the slot to display</param>
		 * <param name = "languageNumber">The index number of the language to display text in</param>
		 * <param name = "isActive">If True, then the element will be drawn as though highlighted</param>
		 */
		public override void PreDisplay (int _slot, int languageNumber, bool isActive)
		{
			if (fixedOption)
			{
				_slot = 0;
			}

			if (Application.isPlaying)
			{
				if (uiSlots != null && uiSlots.Length > _slot)
				{
					LimitUISlotVisibility (uiSlots, numSlots);

					if (displayType == ConversationDisplayType.IconOnly)
					{
						uiSlots[_slot].SetImage (icons [_slot]);
					}
					else
					{
						uiSlots[_slot].SetText (labels [_slot]);
					}
				}
			}
			else
			{
				string fullText = "";
				if (fixedOption)
				{
					fullText = "Dialogue option " + optionToShow.ToString ();
				}
				else
				{
					fullText = "Dialogue option " + _slot.ToString ();
				}
				if (labels == null || labels.Length != numSlots)
				{
					labels = new string[numSlots];
				}
				chosens = new bool[numSlots];
				labels [_slot] = fullText;
			}
		}
		

		/**
		 * <summary>Draws the element using OnGUI</summary>
		 * <param name = "_style">The GUIStyle to draw with</param>
		 * <param name = "_slot">The index number of the slot to display</param>
		 * <param name = "zoom">The zoom factor</param>
		 * <param name = "isActive If True, then the element will be drawn as though highlighted</param>
		 */
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display (_style, _slot, zoom, isActive);

			if (fixedOption)
			{
				_slot = 0;
			}

			if (markAlreadyChosen)
			{
				if (chosens[_slot])
				{
					_style.normal.textColor = alreadyChosenFontColour;
				}
				else
				{
					_style.normal.textColor = fontColor;
				}
			}

			_style.wordWrap = true;
			_style.alignment = anchor;
			if (zoom < 1f)
			{
				_style.fontSize = (int) ((float) _style.fontSize * zoom);
			}

			if (displayType == ConversationDisplayType.TextOnly)
			{
				if (textEffects != TextEffects.None)
				{
					AdvGame.DrawTextEffect (ZoomRect (GetSlotRectRelative (_slot), zoom), labels [_slot], _style, Color.black, _style.normal.textColor, 2, textEffects);
				}
				else
				{
					GUI.Label (ZoomRect (GetSlotRectRelative (_slot), zoom), labels [_slot], _style);
				}
			}
			else
			{
				if (Application.isPlaying && icons[_slot] != null)
				{
					GUI.DrawTexture (ZoomRect (GetSlotRectRelative (_slot), zoom), icons[_slot], ScaleMode.StretchToFill, true, 0f);
				}
				else if (testIcon != null)
				{
					GUI.DrawTexture (ZoomRect (GetSlotRectRelative (_slot), zoom), testIcon, ScaleMode.StretchToFill, true, 0f);
				}
				
				GUI.Label (ZoomRect (GetSlotRectRelative (_slot), zoom), "", _style);
			}
		}
		

		/**
		 * <summary>Recalculates the element's size.
		 * This should be called whenever a Menu's shape is changed.</summary>
		 * <param name = "source">How the parent Menu is displayed (AdventureCreator, UnityUiPrefab, UnityUiInScene)</param>
		 */
		public override void RecalculateSize (MenuSource source)
		{
			if (Application.isPlaying)
			{
				if (KickStarter.playerInput.activeConversation)
				{
					numOptions = KickStarter.playerInput.activeConversation.GetCount ();
					
					if (fixedOption)
					{
						if (numOptions < optionToShow)
						{
							numSlots = 0;
						}
						else
						{
							numSlots = 1;
							labels = new string[numSlots];
							labels[0] = KickStarter.playerInput.activeConversation.GetOptionName (optionToShow - 1);
							
							icons = new Texture2D[numSlots];
							icons[0] = KickStarter.playerInput.activeConversation.GetOptionIcon (optionToShow - 1);

							chosens = new bool[numSlots];
							chosens[0] = KickStarter.playerInput.activeConversation.OptionHasBeenChosen (optionToShow -1);
						}
					}
					else
					{
						numSlots = numOptions;
						if (numSlots > maxSlots)
						{
							numSlots = maxSlots;
						}

						labels = new string[numSlots];
						icons = new Texture2D[numSlots];
						chosens = new bool[numSlots];
						for (int i=0; i<numSlots; i++)
						{
							labels[i] = KickStarter.playerInput.activeConversation.GetOptionName (i + offset);
							icons[i] = KickStarter.playerInput.activeConversation.GetOptionIcon (i + offset);
							chosens[i] = KickStarter.playerInput.activeConversation.OptionHasBeenChosen (i + offset);

							if (markAlreadyChosen && source != MenuSource.AdventureCreator)
							{
								if (chosens[i+offset])
								{
									uiSlots[i+offset].SetColour (alreadyChosenFontColour);
								}
								else
								{
									uiSlots[i+offset].RestoreColour ();
								}
							}
						}
						
						LimitOffset (numOptions);
					}
				}
				else
				{
					numSlots = 0;
				}
			}
			else if (fixedOption)
			{
				numSlots = 1;
				offset = 0;
				labels = new string[numSlots];
				icons = new Texture2D[numSlots];
				chosens = new bool[numSlots];
			}

			if (Application.isPlaying && uiSlots != null)
			{
				ClearSpriteCache (uiSlots);
			}

			if (!isVisible)
			{
				LimitUISlotVisibility (uiSlots, 0);
			}

			base.RecalculateSize (source);
		}
		

		/**
		 * <summary>Shifts which slots are on display, if the number of slots the element has exceeds the number of slots it can show at once.</summary>
		 * <param name = "shiftType">The direction to shift slots in (Left, Right)</param>
		 * <param name = "amount">The amount to shift slots by</param>
		 */
		public override void Shift (AC_ShiftInventory shiftType, int amount)
		{
			if (isVisible && numSlots >= maxSlots)
			{
				Shift (shiftType, maxSlots, numOptions, amount);
			}
		}
		

		/**
		 * <summary>Checks if the element's slots can be shifted in a particular direction.</summary>
		 * <param name = "shiftType">The direction to shift slots in (Left, Right)</param>
		 * <returns>True if the element's slots can be shifted in the particular direction</returns>
		 */
		public override bool CanBeShifted (AC_ShiftInventory shiftType)
		{
			if (shiftType == AC_ShiftInventory.ShiftLeft)
			{
				if (offset == 0)
				{
					return false;
				}
			}
			else
			{
				if ((maxSlots + offset) >= numOptions)
				{
					return false;
				}
			}
			return true;
		}
		

		/**
		 * <summary>Gets the display text of the element</summary>
		 * <param name = "slot">The index number of the slot</param>
		 * <param name = "languageNumber">The index number of the language number to get the text in</param>
		 * <returns>The display text of the element's slot, or the whole element if it only has one slot</returns>
		 */
		public override string GetLabel (int slot, int languageNumber)
		{
			if (labels.Length > slot)
			{
				return labels[slot];
			}
			
			return "";
		}
		

		/**
		 * <summary>Performs what should happen when the element is clicked on.</summary>
		 * <param name = "_menu">The element's parent Menu</param>
		 * <param name = "_slot">The index number of ths slot that was clicked</param>
		 * <param name = "_mouseState">The state of the mouse button</param>
		 */
		public override void ProcessClick (AC.Menu _menu, int _slot, MouseState _mouseState)
		{
			if (KickStarter.stateHandler.gameState != GameState.DialogOptions)
			{
				return;
			}
			base.ProcessClick (_menu, _slot, _mouseState);

			if (KickStarter.playerInput && KickStarter.playerInput.activeConversation)
			{
				if (fixedOption)
				{
					KickStarter.playerInput.activeConversation.RunOption (optionToShow - 1);
				}
				else
				{
					KickStarter.playerInput.activeConversation.RunOption (_slot + offset);
				}
			}

			offset = 0;
		}
		
		
		protected override void AutoSize ()
		{
			if (displayType == ConversationDisplayType.IconOnly)
			{
				AutoSize (new GUIContent (testIcon));
			}
			else
			{
				AutoSize (new GUIContent ("Dialogue option 0"));
			}
		}
		
	}
	
}
