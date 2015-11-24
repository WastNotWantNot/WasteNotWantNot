/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuCycle.cs"
 * 
 *	This MenuElement is like a label, only it's text cycles through an array when clicked on.
 * 
 */

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/**
	 * A MenuElement that displays different text each time it is clicked on.
	 * The index number of the text array it displays can be linked to a Global Variable (GVar), a custom script, or the game's current language.
	 */
	public class MenuCycle : MenuElement
	{

		/** The Unity UI Button this is linked to (Unity UI Menus only) */
		public UnityEngine.UI.Button uiButton;
		/** The ActionListAsset to run when the element is clicked on */
		public ActionListAsset actionListOnClick = null;
		/** The text that's displayed on-screen, which prefixes the varying text */
		public string label = "Element";
		/** The special FX applied to the text (None, Outline, Shadow, OutlineAndShadow) */
		public TextEffects textEffects;
		/** The text alignment */
		public TextAnchor anchor;
		/** The index number of the currently-shown text in optionsArray */
		public int selected;
		/** An array of texts that the element can show one at a time */
		public List<string> optionsArray = new List<string>();
		/** What the text links to (CustomScript, GlobalVariable, Language) */
		public AC_CycleType cycleType;
		/** The ID number of the linked GlobalVariable, if cycleType = AC_CycleType.GlobalVariable */
		public int varID;

		private Text uiText;
		private string cycleText;


		/**
		 * Initialises the element when it is created within MenuManager.
		 */
		public override void Declare ()
		{
			uiButton = null;
			uiText = null;
			label = "Cycle";
			selected = 0;
			isVisible = true;
			isClickable = true;
			numSlots = 1;
			textEffects = TextEffects.None;
			SetSize (new Vector2 (15f, 5f));
			anchor = TextAnchor.MiddleLeft;
			cycleType = AC_CycleType.CustomScript;
			varID = 0;
			optionsArray = new List<string>();
			cycleText = "";
			actionListOnClick = null;
			
			base.Declare ();
		}


		/**
		 * <summary>Creates and returns a new MenuCycle that has the same values as itself.</summary>
		 * <returns>A new MenuCycle with the same values as itself</returns>
		 */
		public override MenuElement DuplicateSelf ()
		{
			MenuCycle newElement = CreateInstance <MenuCycle>();
			newElement.Declare ();
			newElement.CopyCycle (this);
			return newElement;
		}
		
		
		private void CopyCycle (MenuCycle _element)
		{
			uiButton = _element.uiButton;
			uiText = null;
			label = _element.label;
			textEffects = _element.textEffects;
			anchor = _element.anchor;
			selected = _element.selected;
			optionsArray = _element.optionsArray;
			cycleType = _element.cycleType;
			varID = _element.varID;
			cycleText = "";
			actionListOnClick = _element.actionListOnClick;

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

			if (source != AC.MenuSource.AdventureCreator)
			{
				uiButton = LinkedUiGUI <UnityEngine.UI.Button> (uiButton, "Linked Button:", source);
				EditorGUILayout.EndVertical ();
				EditorGUILayout.BeginVertical ("Button");
			}

			label = EditorGUILayout.TextField ("Label text:", label);

			if (source == MenuSource.AdventureCreator)
			{
				anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
				textEffects = (TextEffects) EditorGUILayout.EnumPopup ("Text effect:", textEffects);
			}

			cycleType = (AC_CycleType) EditorGUILayout.EnumPopup ("Cycle type:", cycleType);
			if (cycleType == AC_CycleType.CustomScript || cycleType == AC_CycleType.Variable)
			{
				int numOptions = optionsArray.Count;
				numOptions = EditorGUILayout.IntField ("Number of choices:", optionsArray.Count);
				if (numOptions < 0)
				{
					numOptions = 0;
				}
				
				if (numOptions < optionsArray.Count)
				{
					optionsArray.RemoveRange (numOptions, optionsArray.Count - numOptions);
				}
				else if (numOptions > optionsArray.Count)
				{
					if(numOptions > optionsArray.Capacity)
					{
						optionsArray.Capacity = numOptions;
					}
					for (int i=optionsArray.Count; i<numOptions; i++)
					{
						optionsArray.Add ("");
					}
				}
				
				for (int i=0; i<optionsArray.Count; i++)
				{
					optionsArray [i] = EditorGUILayout.TextField ("Choice #" + i.ToString () + ":", optionsArray [i]);
				}
				
				if (cycleType == AC_CycleType.CustomScript)
				{
					if (optionsArray.Count > 0)
					{
						selected = EditorGUILayout.IntField ("Default option #:", selected);
					}
					ShowClipHelp ();
				}
				else if (cycleType == AC_CycleType.Variable)
				{
					varID = EditorGUILayout.IntField ("Global Variable ID:", varID);
				}

				actionListOnClick = (ActionListAsset) EditorGUILayout.ObjectField ("ActionList on click:", actionListOnClick, typeof (ActionListAsset), false);
			}
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
			CalculateValue ();

			cycleText = TranslateLabel (label, languageNumber) + " : ";
			
			if (Application.isPlaying)
			{
				if (optionsArray.Count > selected && selected > -1)
				{
					cycleText += optionsArray [selected];
				}
				else
				{
					ACDebug.Log ("Could not gather options for MenuCycle " + label);
					selected = 0;
				}
			}
			else if (optionsArray.Count > 0)
			{
				if (selected >= 0 && selected < optionsArray.Count)
				{
					cycleText += optionsArray [selected];
				}
				else
				{
					cycleText += optionsArray [0];
				}
			}
			else
			{
				cycleText += "Default option";	
			}

			if (uiButton)
			{
				if (uiText)
				{
					uiText.text = cycleText;
				}
				UpdateUIElement (uiButton);
			}
		}
		

		/**
		 * <summary>Draws the element using OnGUI.</summary>
		 * <param name = "_style">The GUIStyle to draw with</param>
		 * <param name = "_slot">The index number of the slot to display</param>
		 * <param name = "zoom">The zoom factor</param>
		 * <param name = "isActive">If True, then the element will be drawn as though highlighted</param>
		 */
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display (_style, _slot, zoom, isActive);
			
			_style.alignment = anchor;
			if (zoom < 1f)
			{
				_style.fontSize = (int) ((float) _style.fontSize * zoom);
			}

			if (textEffects != TextEffects.None)
			{
				AdvGame.DrawTextEffect (ZoomRect (relativeRect, zoom), cycleText, _style, Color.black, _style.normal.textColor, 2, textEffects);
			}
			else
			{
				GUI.Label (ZoomRect (relativeRect, zoom), cycleText, _style);
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
			if (optionsArray.Count > selected && selected > -1)
			{
				return TranslateLabel (label, languageNumber) + " : " + optionsArray [selected];
			}
			
			return TranslateLabel (label, languageNumber);
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

			selected ++;
			if (selected > optionsArray.Count-1)
			{
				selected = 0;
			}
			
			if (cycleType == AC_CycleType.Language)
			{
				if (selected == 0 && KickStarter.speechManager.ignoreOriginalText && KickStarter.runtimeLanguages.Languages.Count > 1)
				{
					// Ignore original text by skipping to first language
					selected = 1;
				}
				Options.SetLanguage (selected);
			}
			else if (cycleType == AC_CycleType.Variable)
			{
				if (varID >= 0)
				{
					GVar var = GlobalVariables.GetVariable (varID);
					if (var.type == VariableType.Integer)
					{
						var.val = selected;
						var.Upload ();
					}
				}
			}

			if (cycleType == AC_CycleType.CustomScript)
			{
				MenuSystem.OnElementClick (_menu, this, _slot, (int) _mouseState);
			}

			if (actionListOnClick)
			{
				AdvGame.RunActionListAsset (actionListOnClick);
			}
		}


		private void CalculateValue ()
		{
			if (!Application.isPlaying)
			{
				return;
			}

			if (cycleType == AC_CycleType.Language)
			{
				if (Application.isPlaying)
				{
					optionsArray = KickStarter.runtimeLanguages.Languages;
				}
				else
				{
					optionsArray = AdvGame.GetReferences ().speechManager.languages;
				}

				if (Options.optionsData != null)
				{
					selected = Options.optionsData.language;
				}
			}
			else if (cycleType == AC_CycleType.Variable)
			{
				if (varID >= 0)
				{
					if (GlobalVariables.GetVariable (varID) == null || GlobalVariables.GetVariable (varID).type != VariableType.Integer)
					{
						ACDebug.LogWarning ("Cannot link MenuToggle " + title + " to Variable " + varID + " as it is not an Integer.");
					}
					else if (optionsArray.Count > 0)
					{
						selected = Mathf.Clamp (GlobalVariables.GetIntegerValue (varID), 0, optionsArray.Count - 1);
					}
					else
					{
						selected = 0;
					}
				}
			}
		}


		protected override void AutoSize ()
		{
			AutoSize (new GUIContent (TranslateLabel (label, Options.GetLanguage ()) + " : Default option"));
		}
		
	}
	
}