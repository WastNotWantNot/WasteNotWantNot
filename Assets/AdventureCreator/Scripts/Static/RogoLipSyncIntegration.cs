using UnityEngine;
using System.Collections;

namespace AC
{

	/**
	 * A class the contains a number of static functions to assist with Rogo Digital LipSync integration.
	 * To use Rogo Digital LipSync with Adventure Creator, the 'RogoLipSyncIsPresent' preprocessor must be defined.
	 */
	public class RogoLipSyncIntegration : ScriptableObject
	{
		
		/**
		 * <summary>Checks if the 'RogoLipSyncIsPresent' preprocessor has been defined.</summary>
		 * <returns>True if the 'RogoLipSyncIsPresent' preprocessor has been defined</returns>
		 */
		public static bool IsDefinePresent ()
		{
			#if RogoLipSyncIsPresent
			return true;
			#else
			return false;
			#endif
		}


		public static void Play (Char speaker, string speakerName, int lineNumber, string language)
		{
			if (speaker == null)
			{
				return;
			}

			#if RogoLipSyncIsPresent
			if (lineNumber > -1 && speakerName != "" && KickStarter.speechManager.searchAudioFiles)
			{
				string filename = "Lipsync/";
				if (KickStarter.speechManager.placeAudioInSubfolders)
				{
					filename += speakerName + "/";
				}
				if (language != "" && KickStarter.speechManager.translateAudio)
				{
					// Not in original language
					filename += language + "/";
				}
				filename += speakerName + lineNumber;

				RogoDigital.Lipsync.LipSyncData lipSyncData = Resources.Load (filename) as RogoDigital.Lipsync.LipSyncData;
				if (lipSyncData != null)
				{
					if (speaker.GetComponent <RogoDigital.Lipsync.LipSync>() != null)
					{
						speaker.GetComponent <RogoDigital.Lipsync.LipSync>().Play (lipSyncData);
					}
					else
					{
						ACDebug.LogWarning ("No LipSync component found on " + speaker.gameObject.name + " gameobject.");
					}
				}
				else
				{
					ACDebug.LogWarning ("No lipsync file found.  Looking for 'Resources/" + filename + "'");
				}
			}
			#else
			ACDebug.LogError ("The 'RogoLipSyncIsPresent' preprocessor define must be declared in the Player Settings.");
			#endif
		}


		public static void Stop (Char speaker)
		{
			if (speaker == null)
			{
				return;
			}
			
			#if RogoLipSyncIsPresent
			if (speaker.GetComponent <RogoDigital.Lipsync.LipSync>() != null)
			{
				speaker.GetComponent <RogoDigital.Lipsync.LipSync>().Stop (true);
			}
			#endif
		}
		
	}

}