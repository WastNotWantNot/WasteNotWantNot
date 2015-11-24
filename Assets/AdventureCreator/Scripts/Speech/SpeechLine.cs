/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"SpeechLine.cs"
 * 
 *	This script is a data container for speech lines found by Speech Manager.
 *	Such data is used to provide translation support, as well as auto-numbering
 *	of speech lines for sound files.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/**
	 * A container class for text gathered by the Speech Manager.
	 * It is not limited to just speech, as all text displayed in a game will be gathered.
	 */
	[System.Serializable]
	public class SpeechLine
	{

		/** True if this is a speech line spoken by the Player */
		public bool isPlayer;
		/** A unique ID number to identify the instance by */
		public int lineID;
		/** The name of the scene that the text was found in */
		public string scene;
		/** If not the player, who the text is owned by */
		public string owner;
		/** The display text itself */
		public string text;
		/** A user-generated description of the text */
		public string description;
		/** The type of text this is (Speech, Hotspot, DialogueOption, InventoryItem, CursorIcon, MenuElement, HotspotPrefix, JournalEntry) */
		public AC_TextType textType;
		/** An array of translations for the display text */
		public List<string> translationText = new List<string>();


		/**
		 * A constructor for non-speech text in which the ID number is explicitly defined.
		 */
		public SpeechLine (int _id, string _scene, string _text, int _languagues, AC_TextType _textType)
		{
			lineID = _id;
			scene = _scene;
			owner = "";
			text = _text;
			textType = _textType;
			description = "";
			isPlayer = false;
			
			translationText = new List<string>();
			for (int i=0; i<_languagues; i++)
			{
				translationText.Add (_text);
			}
		}


		/**
		 * A Constructor that copies all values from another SpeechLine.
		 * This way ensures that no connection remains to the original class.
		 */
		public SpeechLine (SpeechLine _speechLine)
		{
			isPlayer = _speechLine.isPlayer;
			lineID = _speechLine.lineID;
			scene = _speechLine.scene;
			owner = _speechLine.owner;
			text = _speechLine.text;
			description = _speechLine.description;
			textType = _speechLine.textType;
			translationText = _speechLine.translationText;
		}
		

		/**
		 * A constructor for non-speech text in which a unique ID number is assigned based on an array of already-used values.
		 */
		public SpeechLine (int[] idArray, string _scene, string _text, int _languagues, AC_TextType _textType)
		{
			// Update id based on array
			lineID = 0;

			foreach (int _id in idArray)
			{
				if (lineID == _id)
					lineID ++;
			}

			scene = _scene;
			owner = "";
			text = _text;
			textType = _textType;
			description = "";
			isPlayer = false;
			
			translationText = new List<string>();
			for (int i=0; i<_languagues; i++)
			{
				translationText.Add (_text);
			}
		}
		

		/**
		 * A constructor for speech text in which the ID number is explicitly defined.
		 */
		public SpeechLine (int _id, string _scene, string _owner, string _text, int _languagues, AC_TextType _textType, bool _isPlayer = false)
		{
			lineID = _id;
			scene = _scene;
			owner = _owner;
			text = _text;
			textType = _textType;
			description = "";
			isPlayer = _isPlayer;
			
			translationText = new List<string>();
			for (int i=0; i<_languagues; i++)
			{
				translationText.Add (_text);
			}
		}
		
		
		/**
		 * A constructor for speech text in which a unique ID number is assigned based on an array of already-used values.
		 */
		public SpeechLine (int[] idArray, string _scene, string _owner, string _text, int _languagues, AC_TextType _textType,  bool _isPlayer = false)
		{
			// Update id based on array
			lineID = 0;
			foreach (int _id in idArray)
			{
				if (lineID == _id)
					lineID ++;
			}
			
			scene = _scene;
			owner = _owner;
			text = _text;
			description = "";
			textType = _textType;
			isPlayer = _isPlayer;

			translationText = new List<string>();
			for (int i=0; i<_languagues; i++)
			{
				translationText.Add (_text);
			}
		}


		/**
		 * <summary>Checks if the class matches another, in terms of line ID, text, type and owner.
		 * Used to determine if a speech line is a duplicate of another.</summary>
		 * <param name = "newLine">The SpeechLine class to check against</param>
		 * <returns>True if the two classes have the same line ID, text, type and owner</returns>
		 */
		public bool IsMatch (SpeechLine newLine)
		{
			if (lineID == newLine.lineID && text == newLine.text && textType == newLine.textType && owner == newLine.owner)
			{
				return true;
			}
			return false;
		}


		#if UNITY_EDITOR

		/**
		 * Displays the GUI of the class's entry within the Speech Manager.
		 */
		public void ShowGUI ()
		{
			SpeechManager speechManager = AdvGame.GetReferences ().speechManager;

			if (this == speechManager.activeLine)
			{
				EditorGUILayout.BeginVertical ("Button");
				ShowField ("ID #:", lineID.ToString (), false);
				ShowField ("Type:", textType.ToString (), false);

				string sceneName = scene.Replace ("Assets/", "");
				sceneName = sceneName.Replace (".unity", "");
				ShowField ("Scene:", sceneName, true);

				if (textType == AC_TextType.Speech)
				{
					ShowField ("Speaker:", GetSpeakerName (), false);
				}

				if (speechManager.languages != null && speechManager.languages.Count > 1)
				{
					for (int i=0; i<speechManager.languages.Count; i++)
					{
						if (i==0)
						{
							ShowField ("Original:", "'" + text + "'", true);
						}
						else if (translationText.Count > (i-1))
						{
							translationText [i-1] = EditField (speechManager.languages[i] + ":", translationText [i-1], true);
						}
						else
						{
							ShowField (speechManager.languages[i] + ":", "(Not defined)", false);
						}

						if (speechManager.translateAudio && textType == AC_TextType.Speech)
						{
							string language = "";
							if (i > 0)
							{
								language = speechManager.languages[i];
							}

							if (speechManager.UseFileBasedLipSyncing ())
							{
								ShowField (" (Lipsync path):", GetFolderName (language, true), false);

							}
							ShowField (" (Audio path):", GetFolderName (language), false);

							EditorGUILayout.Space ();
						}
					}

					if (!speechManager.translateAudio && textType == AC_TextType.Speech)
					{
						if (speechManager.UseFileBasedLipSyncing ())
						{
							ShowField ("Lipsync path:", GetFolderName ("", true), false);
						}
						ShowField ("Audio path:", GetFolderName (""), false);
					}
				}
				else if (textType == AC_TextType.Speech)
				{
					ShowField ("Text:", "'" + text + "'", true);
					if (speechManager.UseFileBasedLipSyncing ())
					{
						ShowField ("Lipsync path:", GetFolderName ("", true), false);
					}
					ShowField ("Audio Path:", GetFolderName (""), false);
				}

				if (textType == AC_TextType.Speech)
				{
					ShowField ("Filename:", GetFilename () + lineID.ToString (), false);
					description = EditField ("Description:", description, true);
				}

				EditorGUILayout.EndVertical ();
			}
			else
			{
				if (GUILayout.Button (lineID.ToString () + ": '" + text + "'", EditorStyles.label, GUILayout.MaxWidth (300)))
				{
					speechManager.activeLine = this;
				}
				GUILayout.Box ("", GUILayout.ExpandWidth (true), GUILayout.Height(1));
			}
		}


		/**
		 * <summary>Displays a GUI of a field within the class.</summary>
		 * <param name = "label">The label in front of the field</param>
		 * <param name = "field">The field to display</param>
		 * <param name = "multiLine">True if the field should be word-wrapped</param>
		 */
		public static void ShowField (string label, string field, bool multiLine)
		{
			if (field == "") return;

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField (label, GUILayout.Width (85f));

			if (multiLine)
			{
				GUIStyle style = new GUIStyle ();
				#if UNITY_PRO_LICENSE
				style.normal.textColor = Color.white;
				#endif
				style.wordWrap = true;
				style.alignment = TextAnchor.MiddleLeft;
				EditorGUILayout.LabelField (field, style, GUILayout.MaxWidth (270f));
			}
			else
			{
				EditorGUILayout.LabelField (field);
			}
			EditorGUILayout.EndHorizontal ();
		}


		private string EditField (string label, string field, bool multiLine)
		{
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField (label, GUILayout.Width (85f));
			if (multiLine)
			{
				field = EditorGUILayout.TextArea (field, EditorStyles.textArea);
			}
			else
			{
				field = EditorGUILayout.TextField (field);
			}
			EditorGUILayout.EndHorizontal ();
			return field;
		}


		/**
		 * <summary>Gets the folder name for a speech line's audio or lipsync file.</summary>
		 * <param name = "language">The language of the audio</param>
		 * <param name = "forLipSync">True if this is for a lipsync file</param>
		 * <returns>A string of the folder name that the audio or lipsync file should be placed in</returns>
		 */
		public string GetFolderName (string language, bool forLipsync = false)
		{
			string folderName = "Resources/";

			if (forLipsync)
			{
				folderName += "Lipsync/";
			}
			else
			{
				folderName += "Speech/";
			}

			if (language != "" && KickStarter.speechManager.translateAudio)
			{
				folderName += language + "/";
			}
			if (KickStarter.speechManager.placeAudioInSubfolders)
			{
				folderName += GetFilename ();
			}
			return folderName;
		}


		/**
		 * <summary>Checks to see if the class matches a filter set in the Speech Manager.</summary>
		 * <param name = "filter The filter text</param>
		 * <param name = "filterSpeechLine The type of filtering selected (Type, Text, Scene, Speaker, Description, All)</param>
		 * <returns>True if the class matches the criteria of the filter, and should be listed</returns>
		 */
		public bool Matches (string filter, FilterSpeechLine filterSpeechLine)
		{
			if (filter == null || filter == "")
			{
				return true;
			}
			filter = filter.ToLower ();
			if (filterSpeechLine == FilterSpeechLine.All)
			{
				if (description.ToLower ().Contains (filter)
				    || scene.ToLower ().Contains (filter)
				    || owner.ToLower ().Contains (filter)
				    || text.ToLower ().Contains (filter)
				    || textType.ToString ().ToLower ().Contains (filter))
				{
					return true;
				}
			}
			else if (filterSpeechLine == FilterSpeechLine.Description)
			{
				return description.ToLower ().Contains (filter);
			}
			else if (filterSpeechLine == FilterSpeechLine.Scene)
			{
				return scene.ToLower ().Contains (filter);
			}
			else if (filterSpeechLine == FilterSpeechLine.Speaker)
			{
				return owner.ToLower ().Contains (filter);
			}
			else if (filterSpeechLine == FilterSpeechLine.Text)
			{
				return text.ToLower ().Contains (filter);
			}
			else if (filterSpeechLine == FilterSpeechLine.Type)
			{
				return textType.ToString ().ToLower ().Contains (filter);
			}
			return false;
		}


		/**
		 * <summary>Combines the type and owner into a single string, for display in exported game text.</summary>
		 * <returns>A string of the type, and the owner if there is one</returns>
		 */
		public string GetInfo ()
		{
			string info = textType.ToString ();
			if (owner != "")
			{
				info += " (" + owner + ")";
			}
			return info;
		}


		/**
		 * <summary>Combines the class's various fields into a formatted HTML string, for display in exported game text.</summary>
		 * <param name = "languageIndex">The index number of the language to display fields for, where 0 = the game's original language</param>
		 * <returns>A string of the owner, filename, text and description</returns>
		 */
		public string Print (int languageIndex = 0)
		{
			int i = languageIndex;

			string result = "<table>\n";
			result += "<tr><td width=150><b>Character:</b></td><td>" + GetSpeakerName () + "</td></tr>\n";

			string lineText = text;
			if (i > 0 && translationText.Count > (i-1))
			{
				lineText = translationText [i-1];
			}
			result += "<tr><td><b>Line text:</b></td><td>" + lineText + "</td></tr>\n";

			if (description != null && description.Length > 0)
			{
				result += "<tr><td><b>Description:</b></td><td>" + description + "</td></tr>\n";
			}

			string language = "";
			if (i > 0 && AdvGame.GetReferences().speechManager.translateAudio)
			{
				language = AdvGame.GetReferences ().speechManager.languages[i];
			}

			if (AdvGame.GetReferences ().speechManager.UseFileBasedLipSyncing ())
			{
				result += "<td><b>Lipsync file:</b></td><td>" + GetFolderName (language, true) + GetFilename () + lineID.ToString () + "</td></tr>\n";
			}
			result += "<tr><td><b>Audio file:</b></td><td>" + GetFolderName (language, false) + GetFilename () + lineID.ToString () + "</td></tr>\n";

			result += "</table>\n\n";
			result += "<br/>\n";
			return result;
		}
		
		#endif


		/**
		 * <summary>Gets the clean-formatted filename for a speech line's audio file.</summary>
		 * <returns>The filename</returns>
		 */
		public string GetFilename ()
		{
			string filename = "";
			if (owner != "")
			{
				filename = owner;
				
				if (isPlayer && (KickStarter.speechManager == null || !KickStarter.speechManager.usePlayerRealName))
				{
					filename = "Player";
				}
				
				string badChars = "/`'!@£$%^&*(){}:;.|<,>?#-=+-";
				for (int i=0; i<badChars.Length; i++)
				{
					filename = filename.Replace(badChars[i].ToString (), "_");
				}
				filename = filename.Replace ('"'.ToString (), "_");
			}
			else
			{
				filename = "Narrator";
			}
			return filename;
		}


		private string GetSpeakerName ()
		{
			if (isPlayer && (AdvGame.GetReferences ().speechManager == null || !AdvGame.GetReferences ().speechManager.usePlayerRealName))
			{
				return "Player";
			}
			return owner;
		}
		
	}
	
}