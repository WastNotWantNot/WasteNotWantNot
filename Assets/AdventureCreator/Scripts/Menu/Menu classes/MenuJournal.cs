/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuJournal.cs"
 * 
 *	This MenuElement provides an array of labels, used to make a book.
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
	 * A MenuElement that provides an array of labels, each one representing a page, that collectively form a bork.
	 * "Pages" can be added to the journal mid-game, and changes made to it will be saved in save games.
	 */
	public class MenuJournal : MenuElement
	{

		/** The Unity UI Text this is linked to (Unity UI Menus only) */
		public Text uiText;
		/** A List of JournalPage instances that make up the pages within */
		public List<JournalPage> pages = new List<JournalPage>();
		/** The initial number of pages when the game begins */
		public int numPages = 1;
		/** The index number of the current page being shown */
		public int showPage = 1;
		/** If True, then the "Preview page" set in the Editor will be the first page open when the game begins */
		public bool startFromPage = false;
		/** The text alignment */
		public TextAnchor anchor;
		/** The special FX applied to the text (None, Outline, Shadow, OutlineAndShadow) */
		public TextEffects textEffects;
		/** An ActionList to run whenever a new page is added */
		public ActionListAsset actionListOnAddPage;

		private string fullText;


		/**
		 * Initialises the element when it is created within MenuManager.
		 */
		public override void Declare ()
		{
			uiText = null;

			pages = new List<JournalPage>();
			pages.Add (new JournalPage ());
			numPages = 1;
			showPage = 1;
			isVisible = true;
			isClickable = false;
			numSlots = 1;
			anchor = TextAnchor.MiddleCenter;
			SetSize (new Vector2 (10f, 5f));
			textEffects = TextEffects.None;
			fullText = "";
			actionListOnAddPage = null;

			base.Declare ();
		}


		/**
		 * <summary>Creates and returns a new MenuJournal that has the same values as itself.</summary>
		 * <returns>A new MenuJournal with the same values as itself</returns>
		 */
		public override MenuElement DuplicateSelf ()
		{
			MenuJournal newElement = CreateInstance <MenuJournal>();
			newElement.Declare ();
			newElement.CopyJournal (this);
			return newElement;
		}
		
		
		private void CopyJournal (MenuJournal _element)
		{
			uiText = _element.uiText;
			pages = new List<JournalPage>();
			foreach (JournalPage page in _element.pages)
			{
				pages.Add (new JournalPage (page));
			}

			numPages = _element.numPages;
			startFromPage = _element.startFromPage;
			if (startFromPage)
			{
				showPage = _element.showPage;
			}
			else
			{
				showPage = 1;
			}
			anchor = _element.anchor;
			textEffects = _element.textEffects;
			fullText = "";
			actionListOnAddPage = _element.actionListOnAddPage;

			base.Copy (_element);
		}


		/**
		 * <summary>Initialises the linked Unity UI GameObject.</summary>
		 * <param name = "_menu">The element's parent Menu</param>
		 */
		public override void LoadUnityUI (AC.Menu _menu)
		{
			uiText = LinkUIElement <Text>();
		}
		

		/**
		 * <summary>Gets the boundary of the element</summary>
		 * <param name = "_slot">Ignored by this subclass</param>
		 * <returns>The boundary Rect of the element</returns>
		 */
		public override RectTransform GetRectTransform (int _slot)
		{
			if (uiText)
			{
				return uiText.rectTransform;
			}
			return null;
		}


		/**
		 * <summary>Gets the currently-viewed page number.</summary>
		 * <returns>The currently-viewed page number</returms>
		 */
		public int GetCurrentPageNumber ()
		{
			return showPage;
		}


		/**
		 * <summary>Gets the total number of pages.</summary>
		 * <returns>The total number of pages</returns>
		 */
		public int GetTotalNumberOfPages ()
		{
			if (pages != null)
			{
				return pages.Count;
			}
			return 0;
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI (MenuSource source)
		{
			EditorGUILayout.BeginVertical ("Button");

			if (pages == null || pages.Count == 0)
			{
				pages.Clear ();
				pages.Add (new JournalPage ());
			}
			numPages = pages.Count;

			for (int i=0; i<pages.Count; i++)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Page #" + (i+1).ToString () + ":");
				if (GUILayout.Button ("-", GUILayout.Width (20f)))
				{
					Undo.RecordObject (this, "Delete journal page");
					pages.RemoveAt (i);
					break;
				}
				EditorGUILayout.EndHorizontal ();

				pages[i].text = EditorGUILayout.TextArea (pages[i].text);
				GUILayout.Box ("", GUILayout.ExpandWidth (true), GUILayout.Height(1));
			}

			if (GUILayout.Button ("Create new page", EditorStyles.miniButton))
			{
				Undo.RecordObject (this, "Create journal page");
				pages.Add (new JournalPage ());
			}

			numPages = pages.Count;

			EditorGUILayout.EndVertical ();
			EditorGUILayout.BeginVertical ("Button");

			if (numPages > 1)
			{
				showPage = EditorGUILayout.IntSlider ("Preview page #:", showPage, 1, numPages);
				startFromPage = EditorGUILayout.Toggle ("Start from this page?", startFromPage);
			}
			else
			{
				showPage = 1;
			}

			if (source == MenuSource.AdventureCreator)
			{
				anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
				textEffects = (TextEffects) EditorGUILayout.EnumPopup ("Text effect:", textEffects);
			}
			else
			{
				EditorGUILayout.EndVertical ();
				EditorGUILayout.BeginVertical ("Button");
				uiText = LinkedUiGUI <Text> (uiText, "Linked Text:", source);
			}

			actionListOnAddPage = (ActionListAsset) EditorGUILayout.ObjectField ("ActionList on add page:", actionListOnAddPage, typeof (ActionListAsset), false);

			EditorGUILayout.EndVertical ();
			
			base.ShowGUI (source);
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
			if (pages.Count >= showPage)
			{
				fullText = TranslatePage (pages[showPage - 1], languageNumber);
				fullText = AdvGame.ConvertTokens (fullText);
			}

			if (uiText != null)
			{
				UpdateUIElement (uiText);
				uiText.text = fullText;
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
			base.Display (_style, _slot, zoom, isActive);
			
			_style.wordWrap = true;
			_style.alignment = anchor;
			if (zoom < 1f)
			{
				_style.fontSize = (int) ((float) _style.fontSize * zoom);
			}

			if (pages.Count >= showPage)
			{
				if (textEffects != TextEffects.None)
				{
					AdvGame.DrawTextEffect (ZoomRect (relativeRect, zoom), fullText, _style, Color.black, _style.normal.textColor, 2, textEffects);
				}
				else
				{
					GUI.Label (ZoomRect (relativeRect, zoom), fullText, _style);
				}
			}
		}


		/**
		 * <summary>Gets the display text of the current page</summary>
		 * <param name = "slot">Ignored by this subclass</param>
		 * <param name = "languageNumber">The index number of the language number to get the text in</param>
		 * <returns>The display text of the current page</returns>
		 */
		public override string GetLabel (int slot, int languageNumber)
		{
			return TranslatePage (pages [showPage-1], languageNumber);
		}


		/**
		 * <summary>Shifts which slots are on display, if the number of slots the element has exceeds the number of slots it can show at once.</summary>
		 * <param name = "shiftType">The direction to shift slots in (Left, Right)</param>
		 * <param name = "doLoop">If True, then shifting right beyond the last page will display the first page, and vice-versa</param>
		 */
		public void Shift (AC_ShiftInventory shiftType, bool doLoop)
		{
			if (shiftType == AC_ShiftInventory.ShiftRight)
			{
				if (pages.Count > showPage)
				{
					showPage ++;
				}
				else if (doLoop)
				{
					showPage = 1;
				}
			}
			else if (shiftType == AC_ShiftInventory.ShiftLeft)
			{
				if (showPage > 1)
				{
					showPage --;
				}
				else if (doLoop)
				{
					showPage = pages.Count;
				}
			}
		}


		private string TranslatePage (JournalPage page, int languageNumber)
		{
			if (languageNumber > 0)
			{
				return (KickStarter.runtimeLanguages.GetTranslation (page.text, page.lineID, languageNumber));
			}
			return page.text;
		}

		
		protected override void AutoSize ()
		{
			if (showPage > 0 && pages.Count >= showPage-1)
			{
				if (pages[showPage-1].text == "" && backgroundTexture != null)
				{
					GUIContent content = new GUIContent (backgroundTexture);
					AutoSize (content);
				}
				else
				{
					GUIContent content = new GUIContent (pages[showPage-1].text);
					AutoSize (content);
				}
			
			}
		}


		/**
		 * <summary>Adds a page to the journal.</summary>
		 * <param name = "newPage">The page to add</param>
		 * <param name = "onlyAddNew">If True, then the page will not be added if it's lineID number matches that of any page already in the journal</param>
		 */
		public void AddPage (JournalPage newPage, bool onlyAddNew)
		{
			if (onlyAddNew && newPage.lineID >= 0 && pages != null && pages.Count > 0)
			{
				// Check for existing to avoid duplicates
				foreach (JournalPage page in pages)
				{
					if (page.lineID == newPage.lineID)
					{
						return;
					}
				}
			}
			pages.Add (newPage);
			AdvGame.RunActionListAsset (actionListOnAddPage);
		}

	}


	/**
	 * A data container for the contents of each page in a MenuJournal.
	 */
	[System.Serializable]
	public class JournalPage
	{

		/** The translation ID, as set by SpeechManager */
		public int lineID = -1;
		/** The page text, in it's original language */
		public string text = "";


		/**
		 * The default Constructor.
		 */
		public JournalPage ()
		{ }


		public JournalPage (JournalPage journalPage)
		{
			lineID = -1;
			text = journalPage.text;
		}


		public JournalPage (int _lineID, string _text)
		{
			lineID = _lineID;
			text = _text;
		}

	}

}