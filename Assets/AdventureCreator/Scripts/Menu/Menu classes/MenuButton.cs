/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuButton.cs"
 * 
 *	This MenuElement can be clicked on to perform a specified function.
 * 
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;	
#endif

namespace AC
{

	/**
	 * A MenuElement that can be clicked on to perform a specific function.
	 */
	[System.Serializable]
	public class MenuButton : MenuElement
	{

		/** The Unity UI Button this is linked to (Unity UI Menus only) */
		public UnityEngine.UI.Button uiButton;
		/** The text that's displayed on-screen */
		public string label = "Element";
		/** The text that appears in the Hotspot label buffer when the mouse hovers over */
		public string hotspotLabel = "";
		/** The translation ID of the text that appears in the Hotspot label buffer when the mouse hovers over, as set in SpeechManager */
		public int hotspotLabelID = -1;	

		/** The text alignment */
		public TextAnchor anchor;
		/** The special FX applied to the text (None, Outline, Shadow, OutlineAndShadow) */
		public TextEffects textEffects;
		/** The type of reaction that occurs when clicked (TurnOffMenu, Crossfade, OffsetElementSlot, RunActionList, CustomScript, OffsetJournal, SimulateInput) */
		public AC_ButtonClickType buttonClickType;

		/** The ActionListAsset to run when clicked, if buttonClickType = AC_ButtonClickType.RunActionList */
		public ActionListAsset actionList;
		/** The ID of the integer ActionParameter that can be set within actionList, if buttonClickType = AC_ButtonClickType.RunActionList */
		public int parameterID = -1;
		/** The value to set the integer ActionParameter within actionList, if buttonClickType = AC_ButtonClickType.RunActionList */
		public int parameterValue = 0;

		/** If True, and buttonClickType = AC_ButtonClickType.TurnOffMenu, then the Menu will a as it turns off */
		public bool doFade;
		/** The name of the Menu to crossfade to, if buttonClickType = AC_ButtonClickType.Crossfade */
		public string switchMenuTitle;
		/** The name of the MenuElement with slots to shift, if buttonClickType = AC_ButtonClickType.OffsetElementSlot */
		public string inventoryBoxTitle;
		/** The direction to shift slots, if buttonClickType = AC_ButtonClickType.OffsetElementSlot (Left, Right) */
		public AC_ShiftInventory shiftInventory;
		/** The amount to shift slots by, if buttonClickType = AC_ButtonClickType.OffsetElementSlot */
		public int shiftAmount = 1;
		/** If True, and buttonClickType = AC_ButtyonClickType.OffsetElementSlot, then the button will only be visible if the slots it affects can actually be shifted */
		public bool onlyShowWhenEffective;
		/** If True, and buttonClickType = AC_ButtonClickType.OffsetJournal then shifting past the last journal page will open the first */
		public bool loopJournal = false;
		/** The name of the Input to simulate when clicked, if buttonClickType = AC_ButtonClickType.SimulateInput */
		public string inputAxis;
		/** The type of Input to simulate when clicked, if buttonClickType = AC_ButtonClickType.SimulateInput */
		public SimulateInputType simulateInput = SimulateInputType.Button;
		/** The value of the Input to simulate when clicked, if buttonClickType = AC_ButtonClickType.SimulateInput */
		public float simulateValue = 1f;

		/** The texture to overlay when the button is clicked on */
		public Texture2D clickTexture;
		/** If True, then the button will respond to the mouse button being held down */
		public bool allowContinuousClick = false;

		private Text uiText;													
		private MenuElement elementToShift;
		private float clickAlpha = 0f;
		private string fullText;


		/**
		 * Initialises the element when it is created within MenuManager.
		 */
		public override void Declare ()
		{
			uiButton = null;
			uiText = null;
			label = "Button";
			hotspotLabel = "";
			hotspotLabelID = -1;
			isVisible = true;
			isClickable = true;
			textEffects = TextEffects.None;
			buttonClickType = AC_ButtonClickType.RunActionList;
			simulateInput = SimulateInputType.Button;
			simulateValue = 1f;
			numSlots = 1;
			anchor = TextAnchor.MiddleCenter;
			SetSize (new Vector2 (10f, 5f));
			doFade = false;
			switchMenuTitle = "";
			inventoryBoxTitle = "";
			shiftInventory = AC_ShiftInventory.ShiftLeft;
			loopJournal = false;
			actionList = null;
			inputAxis = "";
			clickTexture = null;
			clickAlpha = 0f;
			shiftAmount = 1;
			onlyShowWhenEffective = false;
			allowContinuousClick = false;
			parameterID = -1;
			parameterValue = 0;

			base.Declare ();
		}


		/**
		 * <summary>Creates and returns a new MenuButton that has the same values as itself.</summary>
		 * <returns>A new MenuButton with the same values as itself</returns>
		 */
		public override MenuElement DuplicateSelf ()
		{
			MenuButton newElement = CreateInstance <MenuButton>();
			newElement.Declare ();
			newElement.CopyButton (this);
			return newElement;
		}
		

		/**
		 * <summary>Copies the values from another MenuButton instance.</summary>
		 * <param name = "_element">The MenuButton to copy values from</param>
		 */
		public void CopyButton (MenuButton _element)
		{
			uiButton = _element.uiButton;
			uiText = _element.uiText;
			label = _element.label;
			hotspotLabel = _element.hotspotLabel;
			hotspotLabelID = _element.hotspotLabelID;
			anchor = _element.anchor;
			textEffects = _element.textEffects;
			buttonClickType = _element.buttonClickType;
			simulateInput = _element.simulateInput;
			simulateValue = _element.simulateValue;
			doFade = _element.doFade;
			switchMenuTitle = _element.switchMenuTitle;
			inventoryBoxTitle = _element.inventoryBoxTitle;
			shiftInventory = _element.shiftInventory;
			loopJournal = _element.loopJournal;
			actionList = _element.actionList;
			inputAxis = _element.inputAxis;
			clickTexture = _element.clickTexture;
			clickAlpha = _element.clickAlpha;
			shiftAmount = _element.shiftAmount;
			onlyShowWhenEffective = _element.onlyShowWhenEffective;
			allowContinuousClick = _element.allowContinuousClick;
			parameterID = _element.parameterID;
			parameterValue = _element.parameterValue;
				
			base.Copy (_element);
		}


		/**
		 * <summary>Initialises the linked Unity UI GameObject.</summary>
		 * <param name = "_menu">The element's parent Menu</param>
		 */
		public override void LoadUnityUI (AC.Menu _menu)
		{
			uiButton = LinkUIElement <UnityEngine.UI.Button>();
			if (uiButton)
			{
				if (uiButton.GetComponentInChildren <Text>())
				{
					uiText = uiButton.GetComponentInChildren <Text>();
				}
				uiButton.onClick.AddListener (() => {
					ProcessClick (_menu, 0, KickStarter.playerInput.GetMouseState ());
				});
			}
		}


		/**
		 * <summary>Gets the linked Unity UI GameObject associated with this element.</summary>
		 * <returns>The Unity UI GameObject associated with the element</returns>
		 */
		public override GameObject GetObjectToSelect ()
		{
			if (uiButton)
			{
				return uiButton.gameObject;
			}
			return null;
		}


		/**
		 * <summary>Gets the boundary of the element.</summary>
		 * <param name = "_slot">Ignored by this subclass</param>
		 * <returns>The boundary Rect of the element</returns>
		 */
		public override RectTransform GetRectTransform (int _slot)
		{
			if (uiButton)
			{
				return uiButton.GetComponent <RectTransform>();
			}
			return null;
		}


		public override void SetUIInteractableState (bool state)
		{
			if (uiButton)
			{
				uiButton.interactable = state;
			}
		}


		#if UNITY_EDITOR
		
		public override void ShowGUI (MenuSource source)
		{
			EditorGUILayout.BeginVertical ("Button");

			if (source != MenuSource.AdventureCreator)
			{
				uiButton = LinkedUiGUI <UnityEngine.UI.Button> (uiButton, "Linked Button:", source);
				EditorGUILayout.EndVertical ();
				EditorGUILayout.BeginVertical ("Button");
			}

			label = EditorGUILayout.TextField ("Button text:", label);

			if (source == MenuSource.AdventureCreator)
			{
				anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
				textEffects = (TextEffects) EditorGUILayout.EnumPopup ("Text effect:", textEffects);
			}

			hotspotLabel = EditorGUILayout.TextField ("Hotspot label override:", hotspotLabel);

			if (source == MenuSource.AdventureCreator)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Click texture:", GUILayout.Width (145f));
				clickTexture = (Texture2D) EditorGUILayout.ObjectField (clickTexture, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
				EditorGUILayout.EndHorizontal ();
			}

			buttonClickType = (AC_ButtonClickType) EditorGUILayout.EnumPopup ("Click type:", buttonClickType);
		
			if (buttonClickType == AC_ButtonClickType.TurnOffMenu)
			{
				doFade = EditorGUILayout.Toggle ("Do transition?", doFade);
			}
			else if (buttonClickType == AC_ButtonClickType.Crossfade)
			{
				switchMenuTitle = EditorGUILayout.TextField ("Menu to switch to:", switchMenuTitle);
			}
			else if (buttonClickType == AC_ButtonClickType.OffsetElementSlot)
			{
				inventoryBoxTitle = EditorGUILayout.TextField ("Element to affect:", inventoryBoxTitle);
				shiftInventory = (AC_ShiftInventory) EditorGUILayout.EnumPopup ("Offset type:", shiftInventory);
				shiftAmount = EditorGUILayout.IntField ("Offset amount:", shiftAmount);
				onlyShowWhenEffective = EditorGUILayout.Toggle ("Only show when effective?", onlyShowWhenEffective);
			}
			else if (buttonClickType == AC_ButtonClickType.OffsetJournal)
			{
				inventoryBoxTitle = EditorGUILayout.TextField ("Journal to affect:", inventoryBoxTitle);
				shiftInventory = (AC_ShiftInventory) EditorGUILayout.EnumPopup ("Offset type:", shiftInventory);
				loopJournal = EditorGUILayout.Toggle ("Cycle pages?", loopJournal);
			}
			else if (buttonClickType == AC_ButtonClickType.RunActionList)
			{
				ActionListGUI ();
			}
			else if (buttonClickType == AC_ButtonClickType.CustomScript)
			{
				allowContinuousClick = EditorGUILayout.Toggle ("Accept held-down clicks?", allowContinuousClick);
				ShowClipHelp ();
			}
			else if (buttonClickType == AC_ButtonClickType.SimulateInput)
			{
				simulateInput = (SimulateInputType) EditorGUILayout.EnumPopup ("Simulate:", simulateInput);
				inputAxis = EditorGUILayout.TextField ("Input axis:", inputAxis);
				if (simulateInput == SimulateInputType.Axis)
				{
					simulateValue = EditorGUILayout.FloatField ("Input value:", simulateValue);
				}
			}

			ChangeCursorGUI (source);
			EditorGUILayout.EndVertical ();
			
			base.ShowGUI (source);
		}


		private void ActionListGUI ()
		{
			actionList = ActionListAssetMenu.AssetGUI ("ActionList to run:", actionList);
			if (actionList != null && actionList.useParameters && actionList.parameters.Count > 0)
			{
				EditorGUILayout.BeginVertical ("Button");
				EditorGUILayout.BeginHorizontal ();
				parameterID = Action.ChooseParameterGUI ("", actionList.parameters, parameterID, ParameterType.Integer);
				if (parameterID >= 0)
				{
					parameterValue = EditorGUILayout.IntField (parameterValue);
				}
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.EndVertical ();
			}
		}
		
		#endif


		/**
		 * Shows the assigned clickTexture overlay, which fades out over time.
		 */
		public void ShowClick ()
		{
			if (isClickable)
			{
				clickAlpha = 1f;
			}
		}


		/**
		 * <summary>Performs all calculations necessary to display the element.</summary>
		 * <param name = "_slot">Ignored by this subclass</param>
		 * <param name = "languageNumber">The index number of the language to display text in</param>
		 * <param name = "isActive">If True, then the element will be drawn as though highlighted</param>
		 */
		public override void PreDisplay (int _slot, int languageNumber, bool isActive)
		{
			if (buttonClickType == AC_ButtonClickType.OffsetElementSlot && onlyShowWhenEffective && inventoryBoxTitle != "" && Application.isPlaying)
			{
				if (elementToShift == null)
				{
					foreach (AC.Menu _menu in PlayerMenus.GetMenus ())
					{
						if (_menu != null && _menu.elements.Contains (this))
						{
							elementToShift = PlayerMenus.GetElementWithName (_menu.title, inventoryBoxTitle);
							break;
						}
					}
				}
				if (elementToShift != null)
				{
					isVisible = elementToShift.CanBeShifted (shiftInventory);
				}
			}

			fullText = TranslateLabel (label, languageNumber);

			if (uiButton != null)
			{
				UpdateUIElement (uiButton);
				if (uiText != null)
				{
					uiText.text = fullText;
				}
			}

		}
		

		/**
		 * <summary>Draws the element using OnGUI</summary>
		 * <param name = "_style">The GUIStyle to draw with</param>
		 * <param name = "_slot">Ignored by this subclass</param>
		 * <param name = "zoom">The zoom factor</param>
		 * <param name = "isActive">If True, then the element will be drawn as though highlighted</param>
		 */
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			if (buttonClickType == AC_ButtonClickType.OffsetElementSlot && onlyShowWhenEffective && inventoryBoxTitle != "" && Application.isPlaying)
			{
				if (elementToShift != null)
				{
					if (!elementToShift.CanBeShifted (shiftInventory))
					{
						return;
					}
				}
			}

			base.Display (_style, _slot, zoom, isActive);

			_style.wordWrap = true;
			_style.alignment = anchor;
			if (zoom < 1f)
			{
				_style.fontSize = (int) ((float) _style.fontSize * zoom);
			}
			
			if (textEffects != TextEffects.None)
			{
				AdvGame.DrawTextEffect (ZoomRect (relativeRect, zoom), fullText, _style, Color.black, _style.normal.textColor, 2, textEffects);
			}
			else
			{
				GUI.Label (ZoomRect (relativeRect, zoom), fullText, _style);
			}

			if (clickAlpha > 0f)
			{
				if (clickTexture)
				{
					Color tempColor = GUI.color;
					tempColor.a = clickAlpha;
					GUI.color = tempColor;
					GUI.DrawTexture (ZoomRect (GetSlotRectRelative (_slot), zoom), clickTexture, ScaleMode.StretchToFill, true, 0f);
					tempColor.a = 1f;
					GUI.color = tempColor;
				}
				clickAlpha -= Time.deltaTime;
				if (clickAlpha < 0f)
				{
					clickAlpha = 0f;
				}
			}
		}


		/**
		 * <summary>Gets the display text of the element</summary>
		 * <param name = "slot">Ignored by this subclass</param>
		 * <param name = "languageNumber">The index number of the language number to get the text in</param>
		 * <returns>The display text of the element</returns>
		 */
		public override string GetLabel (int slot, int languageNumber)
		{
			return TranslateLabel (label, languageNumber);
		}

		
		protected override void AutoSize ()
		{
			if (label == "" && backgroundTexture != null)
			{
				GUIContent content = new GUIContent (backgroundTexture);
				AutoSize (content);
			}
			else
			{
				GUIContent content = new GUIContent (TranslateLabel (label, Options.GetLanguage ()));
				AutoSize (content);
			}
		}


		/**
		 * <summary>Recalculates the element's size.
		 * This should be called whenever a Menu's shape is changed.</summary>
		 * <param name = "source">How the parent Menu is displayed (AdventureCreator, UnityUiPrefab, UnityUiInScene)</param>
		 */
		public override void RecalculateSize (MenuSource source)
		{
			clickAlpha = 0f;
			base.RecalculateSize (source);
		}


		/**
		 * <summary>Performs what should happen when the element is clicked on.</summary>
		 * <param name = "_menu">The element's parent Menu</param>
		 * <param name = "_slot">Ignored by this subclass</param>
		 * <param name = "_mouseState">The state of the mouse button</param>
		 */
		public override void ProcessClick (AC.Menu _menu, int _slot, MouseState _mouseState)
		{
			if (KickStarter.stateHandler.gameState == GameState.Cutscene)
			{
				return;
			}

			base.ProcessClick (_menu, _slot, _mouseState);

			ShowClick ();

			if (buttonClickType == AC_ButtonClickType.TurnOffMenu)
			{
				_menu.TurnOff (doFade);
			}
			else if (buttonClickType == AC_ButtonClickType.Crossfade)
			{
				AC.Menu menuToSwitchTo = PlayerMenus.GetMenuWithName (switchMenuTitle);
				
				if (menuToSwitchTo != null)
				{
					KickStarter.playerMenus.CrossFade (menuToSwitchTo);
				}
				else
				{
					ACDebug.LogWarning ("Cannot find any menu of name '" + switchMenuTitle + "'");
				}
			}
			else if (buttonClickType == AC_ButtonClickType.OffsetElementSlot)
			{
				if (elementToShift == null)
				{
					elementToShift = PlayerMenus.GetElementWithName (_menu.title, inventoryBoxTitle);
				}
				if (elementToShift != null)
				{
					elementToShift.Shift (shiftInventory, shiftAmount);
					elementToShift.RecalculateSize (_menu.menuSource);
				}
				else
				{
					ACDebug.LogWarning ("Cannot find '" + inventoryBoxTitle + "' inside '" + _menu.title + "'");
				}
			}
			else if (buttonClickType == AC_ButtonClickType.OffsetJournal)
			{
				MenuJournal journalToShift = (MenuJournal) PlayerMenus.GetElementWithName (_menu.title, inventoryBoxTitle);
				
				if (journalToShift != null)
				{
					journalToShift.Shift (shiftInventory, loopJournal);
					journalToShift.RecalculateSize (_menu.menuSource);
				}
				else
				{
					ACDebug.LogWarning ("Cannot find '" + inventoryBoxTitle + "' inside '" + _menu.title + "'");
				}
			}
			else if (buttonClickType == AC_ButtonClickType.RunActionList)
			{
				if (actionList)
				{
					AdvGame.RunActionListAsset (actionList, parameterID, parameterValue);
				}
			}
			else if (buttonClickType == AC_ButtonClickType.CustomScript)
			{
				MenuSystem.OnElementClick (_menu, this, _slot, (int) _mouseState);
			}
			else if (buttonClickType == AC_ButtonClickType.SimulateInput)
			{
				KickStarter.playerInput.SimulateInput (simulateInput, inputAxis, simulateValue);
			}
		}


		/**
		 * <summary>Performs what should happen when the element is clicked on continuously.</summary>
		 * <param name = "_menu">The element's parent Menu</param>
		 * <param name = "_mouseState">The state of the mouse button</param>
		 */
		public override void ProcessContinuousClick (AC.Menu _menu, MouseState _mouseState)
		{
			if (buttonClickType == AC_ButtonClickType.SimulateInput)
			{
				KickStarter.playerInput.SimulateInput (simulateInput, inputAxis, simulateValue);
			}
			else if (buttonClickType == AC_ButtonClickType.CustomScript && allowContinuousClick)
			{
				MenuSystem.OnElementClick (_menu, this, 0, (int) _mouseState);
			}
		}


		/**
		 * <summary>Gets the text to place in the Hotspot label buffer (in PlayerMenus) when the mouse hovers over the element.</summary>
		 * <param name = "languageNumber">The index of the language to display the text in</param>
		 * <returns>The text to place in the Hotspot label buffer when the mouse hovers over the element</returns>
		 */
		public string GetHotspotLabel (int languageNumber)
		{
			if (languageNumber > 0)
			{
				return KickStarter.runtimeLanguages.GetTranslation (hotspotLabel, hotspotLabelID, languageNumber);
			}
			return hotspotLabel;
		}

	}

}