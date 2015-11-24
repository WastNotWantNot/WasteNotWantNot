/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuInteraction.cs"
 * 
 *	This MenuElement displays a cursor icon inside a menu.
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
	 * A MenuElement that displays available interactions inside a Menu.
	 * It is used to allow the player to run Hotspot interactions from within a UI.
	 */
	public class MenuInteraction : MenuElement
	{

		/** The Unity UI Button this is linked to (Unity UI Menus only) */
		public UnityEngine.UI.Button uiButton;
		/** How interactions are displayed (IconOnly, TextOnly, IconAndText) */
		public AC_DisplayType displayType = AC_DisplayType.IconOnly;
		/** The text alignment */
		public TextAnchor anchor;
		/** The special FX applied to the text (None, Outline, Shadow, OutlineAndShadow) */
		public TextEffects textEffects;
		/** The ID number of the interaction's associated CursorIcon */
		public int iconID;

		private Text uiText;
		private CursorIcon icon;
		private string label = "";

		private CursorManager cursorManager;


		/**
		 * Initialises the element when it is created within MenuManager.
		 */
		public override void Declare ()
		{
			uiButton = null;
			uiText = null;
			isVisible = true;
			isClickable = true;
			numSlots = 1;
			anchor = TextAnchor.MiddleCenter;
			SetSize (new Vector2 (5f, 5f));
			iconID = -1;
			textEffects = TextEffects.None;
			
			base.Declare ();
		}


		/**
		 * <summary>Creates and returns a new MenuInteraction that has the same values as itself.</summary>
		 * <returns>A new MenuInteraction with the same values as itself</returns>
		 */
		public override MenuElement DuplicateSelf ()
		{
			MenuInteraction newElement = CreateInstance <MenuInteraction>();
			newElement.Declare ();
			newElement.CopyInteraction (this);
			return newElement;
		}
		
		
		private void CopyInteraction (MenuInteraction _element)
		{
			uiButton = _element.uiButton;
			uiText = null;
			displayType = _element.displayType;
			anchor = _element.anchor;
			textEffects = _element.textEffects;
			iconID = _element.iconID;
			
			base.Copy (_element);
		}


		/**
		 * <summary>Initialises the linked Unity UI GameObject.</summary>
		 * <param name = "_menu">The element's parent Menu<param>
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
		 * <summary>Gets the boundary of the element</summary>
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

			if (source == MenuSource.AdventureCreator)
			{
				displayType = (AC_DisplayType) EditorGUILayout.EnumPopup ("Display type:", displayType);
				GetCursorGUI ();
				
				if (displayType != AC_DisplayType.IconOnly)
				{
					anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
					textEffects = (TextEffects) EditorGUILayout.EnumPopup ("Text effect:", textEffects);
				}
			}
			else
			{
				uiButton = LinkedUiGUI <UnityEngine.UI.Button> (uiButton, "Linked Button:", source);
				EditorGUILayout.EndVertical ();
				EditorGUILayout.BeginVertical ("Button");
				displayType = (AC_DisplayType) EditorGUILayout.EnumPopup ("Display type:", displayType);
				GetCursorGUI ();
			}
			EditorGUILayout.EndVertical ();

			base.ShowGUI (source);
		}
		
		
		private void GetCursorGUI ()
		{
			if (AdvGame.GetReferences ().cursorManager && AdvGame.GetReferences().cursorManager.cursorIcons.Count > 0)
			{
				int iconInt = AdvGame.GetReferences ().cursorManager.GetIntFromID (iconID);
				iconInt = EditorGUILayout.Popup ("Cursor:", iconInt, AdvGame.GetReferences ().cursorManager.GetLabelsArray ());
				iconID = AdvGame.GetReferences ().cursorManager.cursorIcons [iconInt].id;
			}
			else
			{
				iconID = -1;
			}
		}
		
		#endif


		/**
		 * <summary>Performs all calculations necessary to display the element.</summary>
		 * <param name = "_slot">Ignored by this subclass</param>
		 * <param name = "languageNumber">The index number of the language to display text in</param>
		 * <param name = "isActive">If True, then the element will be drawn as though highlighted</param>
		 */
		public override void PreDisplay (int _slot, int languageNumber, bool isActive)
		{
			if (uiButton != null)
			{
				UpdateUIElement (uiButton);

				if (displayType != AC_DisplayType.IconOnly && uiText != null)
				{
					uiText.text = label;
				}
			}
		}
		

		/**
		 * <summary>Draws the element using OnGUI</summary>
		 * <param name = "_style">The GUIStyle to draw with</param>
		 * <param name = "_slot">Ignored by this subclass</param>
		 * <param name = "zoom">The zoom factor</param>
		 * <param name = "isActive If True, then the element will be drawn as though highlighted</param>
		 */
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display (_style, _slot, zoom, isActive);

			_style.wordWrap = true;
			_style.alignment = anchor;
			if (zoom < 1f)
			{
				_style.fontSize = (int) ((float) _style.fontSize * zoom);
			}

			if (displayType != AC_DisplayType.IconOnly)
			{
				if (textEffects != TextEffects.None)
				{
					AdvGame.DrawTextEffect (ZoomRect (relativeRect, zoom), label, _style, Color.black, _style.normal.textColor, 2, textEffects);
				}
				else
				{
					GUI.Label (ZoomRect (relativeRect, zoom), label, _style);
				}
			}
			else
			{
				GUI.Label (ZoomRect (relativeRect, zoom), "", _style);
			}

			if (displayType != AC_DisplayType.TextOnly && icon != null)
			{
				icon.DrawAsInteraction (ZoomRect (relativeRect, zoom), isActive);
			}
		}


		/**
		 * <summary>Recalculates display for a particular inventory item.</summary>
		 * <param name = "item">The InvItem to recalculate the Menus's display for</param>
		 */
		public void MatchInteractions (InvItem item)
		{
			bool match = false;
			foreach (InvInteraction interaction in item.interactions)
			{
				if (interaction.icon.id == iconID)
				{
					match = true;
					break;
				}
			}
			
			isVisible = match;
		}
		

		/**
		 * <summary>Recalculates display for a particular set of Hotspot Buttons.</summary>
		 * <param name = "buttons">A List of Button classes to recalculate the Menus's display for</param>
		 */
		public void MatchInteractions (List<AC.Button> buttons)
		{
			bool match = false;
			
			foreach (AC.Button button in buttons)
			{
				if (button.iconID == iconID && !button.isDisabled)
				{
					match = true;
					break;
				}
			}
			isVisible = match;
		}


		/**
		 * <summary>Recalculates display for an "Use" Hotspot Button.</summary>
		 * <param name = "button">A Button class to recalculate the Menus's display for</param>
		 */
		public void MatchUseInteraction (AC.Button button)
		{
			if (button.iconID == iconID && !button.isDisabled)
			{
				isVisible = true;
			}
		}


		/**
		 * <summary>Recalculates display for an "Examine" Hotspot Button.</summary>
		 * <param name = "lookIconID">The ID number of the CursorIcon in CursorManager that is reserved for Examine interactions</param>
		 */
		public void MatchLookInteraction (int lookIconID)
		{
			if (lookIconID == iconID)
			{
				isVisible = true;
			}
		}


		/**
		 * <summary>Gets the display text of the element</summary>
		 * <param name = "slot">Ignored by this subclass</param>
		 * <param name = "languageNumber">The index number of the language number to get the text in</param>
		 * <returns>The display text of the element's slot, or the whole element if it only has one slot</returns>
		 */
		public override string GetLabel (int slot, int languageNumber)
		{
			return label;
		}


		/**
		 * <summary>Recalculates the element's size.
		 * This should be called whenever a Menu's shape is changed.</summary>
		 * <param name = "source">How the parent Menu is displayed (AdventureCreator, UnityUiPrefab, UnityUiInScene)</param>
		 */
		public override void RecalculateSize (MenuSource source)
		{
			if (uiButton == null)
			{
				if (AdvGame.GetReferences ().cursorManager)
				{
					CursorIcon _icon = AdvGame.GetReferences ().cursorManager.GetCursorIconFromID (iconID);
					if (_icon != null)
					{
						icon = _icon;
						label = _icon.label;
						icon.Reset ();
					}
				}
			
				base.RecalculateSize (source);
			}
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

			if (_mouseState == MouseState.RightClick)
			{
				return;
			}

			base.ProcessClick (_menu, _slot, _mouseState);
			KickStarter.playerInteraction.ClickInteractionIcon (_menu, iconID);
		}
		
		
		protected override void AutoSize ()
		{
			if (displayType == AC_DisplayType.IconOnly && icon != null && icon.texture != null)
			{
				GUIContent content = new GUIContent (icon.texture);
				AutoSize (content);
			}
			else
			{
				GUIContent content = new GUIContent (TranslateLabel  (label, Options.GetLanguage ()));
				AutoSize (content);
			}
		}

	}

}