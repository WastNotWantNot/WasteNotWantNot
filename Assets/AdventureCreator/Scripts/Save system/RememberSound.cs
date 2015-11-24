/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"RememberSound.cs"
 * 
 *	This script is attached to Sound objects in the scene
 *	we wish to save.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	/**
	 * Attach this script to Sound objects you wish to save.
	 */
	[RequireComponent (typeof (AudioSource))]
	[RequireComponent (typeof (Sound))]
	public class RememberSound : Remember
	{

		/**
		 * <summary>Serialises appropriate GameObject values into a string.</summary>
		 * <returns>The data, serialised as a string</returns>
		 */
		public override string SaveData ()
		{
			Sound sound = GetComponent <Sound>();
			AudioSource audioSource = GetComponent <AudioSource>();

			SoundData soundData = new SoundData();
			soundData.objectID = constantID;
			if (sound.IsFadingOut ())
			{
				soundData.isPlaying = false;
			}
			else
			{
				soundData.isPlaying = sound.IsPlaying ();
			}
			soundData.isLooping = audioSource.loop;
			soundData.samplePoint = audioSource.timeSamples;
			soundData.relativeVolume = sound.relativeVolume;

			if (audioSource.clip != null)
			{
				soundData.clipID = AssetLoader.GetAssetInstanceID (audioSource.clip);
			}
			
			return Serializer.SaveScriptData <SoundData> (soundData);
		}
		

		/**
		 * <summary>Deserialises a string of data, and restores the GameObject to it's previous state.</summary>
		 * <param name = "stringData">The data, serialised as a string</param>
		 */
		public override void LoadData (string stringData, bool restoringSaveFile = false)
		{
			SoundData data = Serializer.LoadScriptData <SoundData> (stringData);
			if (data == null) return;
			
			Sound sound = GetComponent <Sound>();
			AudioSource audioSource = GetComponent <AudioSource>();

			sound.relativeVolume = data.relativeVolume;
			if (!restoringSaveFile && sound.surviveSceneChange)
			{
				return;
			}

			if (data.isPlaying)
			{
				audioSource.clip = AssetLoader.RetrieveAsset (audioSource.clip, data.clipID);
				sound.PlayAtPoint (data.isLooping, data.samplePoint);
			}
			else
			{
				sound.Stop ();
			}
		}
		
	}
	

	/**
	 * A data container used by the RememberSound script.
	 */
	[System.Serializable]
	public class SoundData : RememberData
	{

		/** True if a sound is playing */
		public bool isPlaying;
		/** True if a sound is looping */
		public bool isLooping;
		/** How far along the track a sound is */
		public int samplePoint;
		/** A unique identifier for the currently-playing AudioClip */
		public string clipID;
		/** The relative volume on the Sound component */
		public float relativeVolume;

		/**
		 * The default Constructor.
		 */
		public SoundData () { }

	}
	
}