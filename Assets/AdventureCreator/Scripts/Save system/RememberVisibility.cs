/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberVisibility.cs"
 * 
 *	This script is attached to scene objects
 *	whose renderer.enabled state we wish to save.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	/**
	 * Attach this to GameObjects whose Renderer's enabled state you wish to save.
	 * Fading in and out, due to the SpriteFader component, is also saved.
	 */
	public class RememberVisibility : Remember
	{

		/** Whether the Renderer is enabled or not when the game begins */
		public AC_OnOff startState = AC_OnOff.On;
		/** True if child Renderers should be affected as well */
		public bool affectChildren = false;

		private LimitVisibility limitVisibility;

		
		private void Awake ()
		{
			if (GameIsPlaying ())
			{
				bool state = false;
				if (startState == AC_OnOff.On)
				{
					state = true;
				}

				if (GetComponent <LimitVisibility>())
				{
					limitVisibility = GetComponent <LimitVisibility>();
					limitVisibility.isLockedOff = !state;
				}
				else if (GetComponent <Renderer>())
				{
					GetComponent <Renderer>().enabled = state;
				}

				if (affectChildren)
				{
					foreach (Renderer _renderer in GetComponentsInChildren <Renderer>())
					{
						_renderer.enabled = state;
					}
				}
			}
		}


		/**
		 * <summary>Serialises appropriate GameObject values into a string.</summary>
		 * <returns>The data, serialised as a string</returns>
		 */
		public override string SaveData ()
		{
			VisibilityData visibilityData = new VisibilityData ();
			visibilityData.objectID = constantID;

			if (GetComponent <SpriteFader>())
			{
				SpriteFader spriteFader = GetComponent <SpriteFader>();
				visibilityData.isFading = spriteFader.isFading;
				if (spriteFader.isFading)
				{
					if (spriteFader.fadeType == FadeType.fadeIn)
					{
						visibilityData.isFadingIn = true;
					}
					else
					{
						visibilityData.isFadingIn = false;
					}

					visibilityData.fadeTime = spriteFader.fadeTime;
					visibilityData.fadeStartTime = spriteFader.fadeStartTime;
				}
				visibilityData.fadeAlpha = GetComponent <SpriteRenderer>().color.a;
			}

			if (GetComponent <FollowTintMap>())
			{
				visibilityData = GetComponent <FollowTintMap>().SaveData (visibilityData);
			}

			if (limitVisibility)
			{
				visibilityData.isOn = !limitVisibility.isLockedOff;
			}
			else if (GetComponent <Renderer>())
			{
				visibilityData.isOn = GetComponent <Renderer>().enabled;
			}
			else if (affectChildren)
			{
				foreach (Renderer _renderer in GetComponentsInChildren <Renderer>())
				{
					visibilityData.isOn = _renderer.enabled;
					break;
				}
			}

			return Serializer.SaveScriptData <VisibilityData> (visibilityData);
		}
		

		/**
		 * <summary>Deserialises a string of data, and restores the GameObject to it's previous state.</summary>
		 * <param name = "stringData">The data, serialised as a string</param>
		 */
		public override void LoadData (string stringData)
		{
			VisibilityData data = Serializer.LoadScriptData <VisibilityData> (stringData);
			if (data == null) return;

			if (GetComponent <SpriteFader>())
			{
				SpriteFader spriteFader = GetComponent <SpriteFader>();
				if (data.isFading)
				{
					if (data.isFadingIn)
					{
						spriteFader.Fade (FadeType.fadeIn, data.fadeTime, data.fadeAlpha);
					}
					else
					{
						spriteFader.Fade (FadeType.fadeOut, data.fadeTime, data.fadeAlpha);
					}
				}
				else
				{
					spriteFader.EndFade ();
					spriteFader.SetAlpha (data.fadeAlpha);
				}
			}

			if (GetComponent <FollowTintMap>())
			{
				GetComponent <FollowTintMap>().LoadData (data);
			}

			if (limitVisibility)
			{
				limitVisibility.isLockedOff = !data.isOn;
			}
			else if (GetComponent <Renderer>())
			{
				GetComponent <Renderer>().enabled = data.isOn;
			}

			if (affectChildren)
			{
				foreach (Renderer _renderer in GetComponentsInChildren <Renderer>())
				{
					_renderer.enabled = data.isOn;
				}
			}
		}
		
	}


	/**
	 * A data container used by the RememberVisibility script.
	 */
	[System.Serializable]
	public class VisibilityData : RememberData
	{

		/** True if the Renderer is enabled */
		public bool isOn;
		/** True if the Renderer is fading */
		public bool isFading;
		/** True if the Renderer is fading in */
		public bool isFadingIn;
		/** The fade duration, if the Renderer is fading */
		public float fadeTime;
		/** The fade start time, if the Renderer is fading */
		public float fadeStartTime;
		/** The current alpha, if the Renderer is fading */
		public float fadeAlpha;

		/** If True, then the attached FollowTintMap makes use of the default TintMap defined in SceneSettings */
		public bool useDefaultTintMap;
		/** The ConstantID number of the attached FollowTintMap's tintMap object */
		public int tintMapID;
		/** The intensity value of the attached FollowTintMap component */
		public float tintIntensity;


		/**
		 * The default Constructor.
		 */
		public VisibilityData () { }

	}

}