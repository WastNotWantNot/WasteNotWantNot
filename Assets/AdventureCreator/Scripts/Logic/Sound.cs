/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Sound.cs"
 * 
 *	This script allows for easy playback of audio sources from within the ActionList system.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	/**
	 * This component controls the volume of the AudioSource component it is attached beside, according to the volume levels set within OptionsData by the player.
	 * It also allows for AudioSources to be controlled using Actions.
	 */
	[RequireComponent (typeof (AudioSource))]
	public class Sound : MonoBehaviour
	{

		/** The type of sound, so far as volume levels go (SFX, Music, Other) */
		public SoundType soundType;
		/** If True, then the sound can play when the game is paused */
		public bool playWhilePaused = false;
		/** The volume of the sound, relative to it's categoriy's "global" volume set within OptionsData */
		public float relativeVolume = 1f;
		/** If True, then the GameObject this is attached to will not be destroyed when changing scene */
		public bool surviveSceneChange = false;

		private float maxVolume = 1f;
		private float smoothVolume = 1f;
		private float smoothUpdateSpeed = 20f;
		private float fadeStartTime;
		private float fadeEndTime;
		private FadeType fadeType;
		private bool isFading = false;

		private Options options;
		private AudioSource audioSource;

		
		private void Awake ()
		{
			if (surviveSceneChange)
			{
				if (transform.root != null && transform.root != gameObject.transform)
				{
					transform.SetParent (null);
				}
				DontDestroyOnLoad (this);
			}
			
			if (GetComponent <AudioSource>())
			{
				audioSource = GetComponent <AudioSource>();

				if (audioSource.playOnAwake)
				{
					audioSource.playOnAwake = false;
				}
			}

			audioSource.ignoreListenerPause = playWhilePaused;
			AdvGame.AssignMixerGroup (audioSource, soundType);
		}


		private void OnLevelWasLoaded ()
		{
			// Search for duplicates carried over from scene change
			if (GetComponent <ConstantID>())
			{
				int ownID = GetComponent <ConstantID>().constantID;
				Sound[] sounds = FindObjectsOfType (typeof (Sound)) as Sound[];
				foreach (Sound sound in sounds)
				{
					if (sound != this && sound.GetComponent <ConstantID>() && sound.GetComponent <ConstantID>().constantID == ownID)
					{
						DestroyImmediate (sound.gameObject);
						return;
					}
				}
			}
		}


		/**
		 * Initialises the AudioSource's volume, when the scene begins.
		 */
		public void AfterLoading ()
		{
			if (audioSource == null && GetComponent <AudioSource>())
			{
				audioSource = GetComponent <AudioSource>();
			}

			if (audioSource)
			{
				audioSource.ignoreListenerPause = playWhilePaused;
				
				if (audioSource.playOnAwake && audioSource.clip)
				{
					FadeIn (0.5f, audioSource.loop);
				}
				else
				{
					SetMaxVolume ();
				}
			}
			else
			{
				ACDebug.LogWarning ("Sound object " + this.name + " has no AudioSource component.");
			}
		}
		

		/**
		 * Updates the AudioSource's volume.
		 * This is called every frame by StateHandler.
		 */
		public void _Update ()
		{
			if (isFading && audioSource.isPlaying)
			{
				smoothVolume = maxVolume;
				float progress = (Time.time - fadeStartTime) / (fadeEndTime - fadeStartTime);
				
				if (fadeType == FadeType.fadeIn)
				{
					if (progress > 1f)
					{
						audioSource.volume = smoothVolume;
						isFading = false;
					}
					else
					{
						audioSource.volume = progress * smoothVolume;
					}
				}
				else if (fadeType == FadeType.fadeOut)
				{
					if (progress > 1f)
					{
						audioSource.volume = 0f;
						Stop ();
					}
					else
					{
						audioSource.volume = (1 - progress) * smoothVolume;
					}
				}
				SetSmoothVolume ();
			}
			else
			{
				SetSmoothVolume ();
				if (audioSource)
				{
					audioSource.volume = smoothVolume;
				}
			}
		}


		private void SetSmoothVolume ()
		{
			if (smoothVolume != maxVolume)
			{
				if (smoothUpdateSpeed > 0)
				{
					smoothVolume = Mathf.Lerp (smoothVolume, maxVolume, Time.deltaTime * smoothUpdateSpeed);
				}
				else
				{
					smoothVolume = maxVolume;
				}
			}
		}
		

		/**
		 * Plays the AudioSource's current AudioClip.
		 */
		public void Interact ()
		{
			isFading = false;
			SetMaxVolume ();
			Play (audioSource.loop);
		}
		

		/**
		 * <summary>Fades in the AudioSource's current AudioClip, after which it continues to play.</summary>
		 * <param name = "fadeTime">The fade duration, in seconds</param>
		 * <param name = "loop">If True, then the AudioClip will loop</param>
		 */
		public void FadeIn (float fadeTime, bool loop)
		{
			if (audioSource.clip == null)
			{
				return;
			}

			audioSource.loop = loop;
			
			fadeStartTime = Time.time;
			fadeEndTime = Time.time + fadeTime;
			fadeType = FadeType.fadeIn;
			
			SetMaxVolume ();
			isFading = true;
			audioSource.volume = 0f;
			audioSource.timeSamples = 0;
			audioSource.Play ();
		}
		

		/**
		 * <summary>Fades out the AudioSource's current AudioClip, after which it stops.</summary>
		 * <param name = "fadeTime">The fade duration, in seconds</param>
		 */
		public void FadeOut (float fadeTime)
		{
			if (audioSource.isPlaying)
			{
				fadeStartTime = Time.time;
				fadeEndTime = Time.time + fadeTime;
				fadeType = FadeType.fadeOut;
				
				SetMaxVolume ();
				isFading = true;
			}
		}


		/**
		 * <summary>Checks if the AudioSource's AudioClip is being faded out.</summary>
		 * <returns>True if the AudioSource's AudioClip is being faded out</returns>
		 */
		public bool IsFadingOut ()
		{
			if (isFading && fadeType == FadeType.fadeOut)
			{
				return true;
			}
			return false;
		}


		#if !UNITY_5
		/**
		 * <summary>Fixes a Unity 4 issue whereby an AudioSource does not play while paused unless it is re-played from the current point.</summary>
		 */
		public void ContinueFix ()
		{
			float startPoint = audioSource.time;
			Play ();
			audioSource.time = startPoint;
		}
		#endif


		/**
		 * <summary>Plays the AudioSource's current AudioClip, without starting over if it was paused or changing it's "loop" variable.</summary>
		 */
		public void Play ()
		{
			if (audioSource == null)
			{
				return;
			}
			isFading = false;
			SetMaxVolume ();
			audioSource.Play ();
		}
		

		/**
		 * <summary>Plays the AudioSource's current AudioClip.</summary>
		 * <param name = "loop">If true, the AudioClip will be looped</param>
		 */
		public void Play (bool loop)
		{
			audioSource.loop = loop;
			audioSource.timeSamples = 0;
			Play ();
		}


		/**
		 * <summary>Plays an AudioClip.</summary>
		 * <param name = "clip">The AudioClip to play</param>
		 * <param name = "loop">If true, the AudioClip will be looped</param>
		 */
		public void Play (AudioClip clip, bool loop)
		{
			audioSource.clip = clip;
			audioSource.loop = loop;
			audioSource.timeSamples = 0;
			Play ();
		}


		/**
		 * <summary>Plays the AudioSource's current AudioClip from a set point.</summary>
		 * <param name = "loop">If true, the AudioClip will be looped</param>
		 * <param name = "samplePoint">The playback position in PCM samples</param>
		 */
		public void PlayAtPoint (bool loop, int samplePoint)
		{
			audioSource.loop = loop;
			audioSource.timeSamples = samplePoint;
			Play ();
		}
		

		/**
		 * Calculates the maximum volume that the AudioSource can have.
		 * This should be called whenever the volume in OptionsData is changed.
		 */
		public void SetMaxVolume ()
		{
			maxVolume = relativeVolume;
			
			if (Options.optionsData != null && soundType != SoundType.Other)
			{
				if (soundType == SoundType.Music)
				{
					maxVolume *= Options.optionsData.musicVolume;
				}
				else if (soundType == SoundType.SFX)
				{
					maxVolume *= Options.optionsData.sfxVolume;
				}
			}

			if (KickStarter.dialog.AudioIsPlaying ())
			{
				if (soundType == SoundType.SFX)
				{
					maxVolume *= 1f - KickStarter.speechManager.sfxDucking;
				}
				else if (soundType == SoundType.Music)
				{
					maxVolume *= 1f - KickStarter.speechManager.musicDucking;
				}
			}

			if (audioSource.isPlaying && playWhilePaused && KickStarter.stateHandler && KickStarter.stateHandler.gameState == GameState.Paused)
			{
				smoothVolume = maxVolume;
			}
		}


		/**
		 * Abrubtply stops the currently-playing sound.
		 */
		public void Stop ()
		{
			isFading = false;
			audioSource.Stop ();
		}


		/**
		 * <summary>Checks if the sound is fading in or out.</summary>
		 * <returns>True if the sound is fading in or out</summary>
		 */
		public bool IsFading ()
		{
			return isFading;
		}


		/**
		 * <summary>Checks if sound is playing.</summary>
		 * <returns>True if sound is playing</summary>
		 */
		public bool IsPlaying ()
		{
			return audioSource.isPlaying;
		}


		/**
		 * <summary>Checks if a particular AudioClip is playing.</summary>
		 * <param name = "clip">The AudioClip to check for</param>
		 * <returns>True if the AudioClip is playing</returns>
		 */
		public bool IsPlaying (AudioClip clip)
		{
			if (audioSource != null && clip != null && audioSource.clip != null && audioSource.clip == clip && audioSource.isPlaying)
			{
				return true;
			}
			return false;
		}
		

		/**
		 * Destroys itself, if it should do.
		 */
		public void TryDestroy ()
		{
			if (surviveSceneChange && !audioSource.isPlaying)
			{
				if (GetComponent <RememberSound>())
				{
					DestroyImmediate (GetComponent <RememberSound>());
				}
				DestroyImmediate (this);
			}
		}


		/**
		 * <summary>Fades out any music being played.</summary>
		 * <param name = "newSound">The Sound object to not affect</param>
		 */
		public void EndOldMusic (Sound newSound)
		{
			if (soundType == SoundType.Music && audioSource.isPlaying && this != newSound)
			{
				if (!isFading || fadeType == FadeType.fadeIn)
				{
					FadeOut (0.1f);
				}
			}
		}


		private void TurnOn ()
		{
			audioSource.timeSamples = 0;
			Play ();
		}


		private void TurnOff ()
		{
			FadeOut (0.2f);
		}


		private void Kill ()
		{
			Stop ();
		}

	}
	
}
