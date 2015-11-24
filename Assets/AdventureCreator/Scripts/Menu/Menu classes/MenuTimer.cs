/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuTimer.cs"
 * 
 *	This MenuElement can be used in conjunction with MenuDialogList to create
 *	timed conversations, "Walking Dead"-style.
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
	 * A MenuElement that provides a "countdown" timer that can either show the time remaining to choose a Conversation's dialogue option or complete a QTE, or the progress made in the current QTE.
	 */
	public class MenuTimer : MenuElement
	{

		/** The Unity UI Slider this is linked to (Unity UI Menus only) */
		public Slider uiSlider;
		/** If True, then the value will be inverted, and the timer will move in the opposite direction */
		public bool doInvert;
		/** The texture of the slider bar (OnGUI Menus only) */
		public Texture2D timerTexture;
		/** What the value of the timer represents (Conversation, QuickTimeEventProgress, QuickTimeEventRemaining) */
		public AC_TimerType timerType = AC_TimerType.Conversation;

		private Rect timerRect;


		/**
		 * Initialises the MenuElement when it is created within MenuManager.
		 */
		public override void Declare ()
		{
			uiSlider = null;
			doInvert = false;
			isVisible = true;
			isClickable = false;
			timerType = AC_TimerType.Conversation;
			numSlots = 1;
			SetSize (new Vector2 (20f, 5f));
			
			base.Declare ();
		}


		/**
		 * <summary>Creates and returns a new MenuTimer that has the same values as itself.</summary>
		 * <returns>A new MenuTimer with the same values as itself</returns>
		 */
		public override MenuElement DuplicateSelf ()
		{
			MenuTimer newElement = CreateInstance <MenuTimer>();
			newElement.Declare ();
			newElement.CopyTimer (this);
			return newElement;
		}
		
		
		private void CopyTimer (MenuTimer _element)
		{
			uiSlider = _element.uiSlider;
			doInvert = _element.doInvert;
			timerTexture = _element.timerTexture;
			timerType = _element.timerType;
			
			base.Copy (_element);
		}


		/**
		 * <summary>Initialises the linked Unity UI GameObject.</summary>
		 * <param name = "_menu">The element's parent Menu</param>
		 */
		public override void LoadUnityUI (AC.Menu _menu)
		{
			uiSlider = LinkUIElement <Slider>();

			if (uiSlider)
			{
				uiSlider.minValue = 0f;
				uiSlider.maxValue = 1f;
				uiSlider.wholeNumbers = false;
				uiSlider.value = 1f;
				uiSlider.interactable = false;
			}
		}


		/**
		 * <summary>Gets the boundary of the element.</summary>
		 * <param name = "_slot">Ignored by this subclass</param>
		 * <returns>The boundary Rect of the element</returns>
		 */
		public override RectTransform GetRectTransform (int _slot)
		{
			if (uiSlider)
			{
				return uiSlider.GetComponent <RectTransform>();
			}
			return null;
		}


		#if UNITY_EDITOR
		
		public override void ShowGUI (MenuSource source)
		{
			EditorGUILayout.BeginVertical ("Button");
			timerType = (AC_TimerType) EditorGUILayout.EnumPopup ("Timer type:", timerType);
			if (timerType == AC_TimerType.LoadingProgress && AdvGame.GetReferences ().settingsManager != null && !AdvGame.GetReferences ().settingsManager.useAsyncLoading)
			{
				EditorGUILayout.HelpBox ("Loading progress cannot be displayed unless asynchonised loading is enabled within the Settings Manager.", MessageType.Warning);
			}
			doInvert = EditorGUILayout.Toggle ("Invert value?", doInvert);

			if (source == MenuSource.AdventureCreator)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Timer texture:", GUILayout.Width (145f));
				timerTexture = (Texture2D) EditorGUILayout.ObjectField (timerTexture, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
				EditorGUILayout.EndHorizontal ();
			}
			else
			{
				uiSlider = LinkedUiGUI <Slider> (uiSlider, "Linked Slider:", source);
			}
			EditorGUILayout.EndVertical ();

			if (source == MenuSource.AdventureCreator)
			{
				EndGUI ();
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
			if (Application.isPlaying)
			{
				float progress = 0f;

				if (timerType == AC_TimerType.Conversation)
				{
					if (KickStarter.playerInput.activeConversation && KickStarter.playerInput.activeConversation.isTimed)
					{
						progress = KickStarter.playerInput.activeConversation.GetTimeRemaining ();
					}
				}
				else if (timerType == AC_TimerType.QuickTimeEventProgress)
				{
					progress = KickStarter.playerQTE.GetProgress ();
				}
				else if (timerType == AC_TimerType.QuickTimeEventRemaining)
				{
					progress = KickStarter.playerQTE.GetRemainingTimeFactor ();
				}
				else if (timerType == AC_TimerType.LoadingProgress)
				{
					progress = KickStarter.sceneChanger.GetLoadingProgress ();
				}

				if (doInvert)
				{
					progress = 1f - progress;
				}

				if (uiSlider)
				{
					uiSlider.value = progress;
					UpdateUIElement (uiSlider);
				}
				else
				{
					timerRect = relativeRect;
					timerRect.width *= progress;
				}
			}
			else
			{
				timerRect = relativeRect;
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
			if (timerTexture)
			{
				GUI.DrawTexture (ZoomRect (timerRect, zoom), timerTexture, ScaleMode.StretchToFill, true, 0f);
			}
			
			base.Display (_style, _slot, zoom, isActive);
		}

	}

}