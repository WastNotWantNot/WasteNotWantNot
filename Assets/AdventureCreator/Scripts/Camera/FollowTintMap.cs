/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"FollowTintMap.cs"
 * 
 *	This script causes any attached Sprite Renderer
 *	to change colour according to a TintMap.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	/**
	 * Attach this script to a GameObject to affect the colour values of it's SpriteRenderer component, according to a TintMap.
	 * This is intended for 2D character sprites, to provide lighting effects when moving around a scene.
	 */
	[RequireComponent (typeof (SpriteRenderer))]
	public class FollowTintMap : MonoBehaviour
	{

		/** If True, then the tintMap defined in SceneSettings will be used as this sprite's colour tinter. */
		public bool useDefaultTintMap = true;
		/** The TintMap to make use of, if useDefaultTintMap = False */
		public TintMap tintMap;
		/** How intense the colour-tiniting effect is. 0 = no effect, 1 = fully tinted */
		public float intensity = 1f;
		/** If True, then SpriteRenderer components found elsewhere in the object's hierarchy will also be affected */
		public bool affectChildren = false;

		private TintMap actualTintMap;
		private SpriteRenderer _spriteRenderer;
		private SpriteRenderer[] _spriteRenderers;

		private float targetIntensity;
		private float initialIntensity;
		private float fadeStartTime;
		private float fadeTime;


		private void Awake ()
		{
			if (KickStarter.settingsManager && KickStarter.settingsManager.IsInLoadingScene ())
			{
				return;
			}

			_spriteRenderer = GetComponent<SpriteRenderer>();
			_spriteRenderers = GetComponentsInChildren <SpriteRenderer>();
			targetIntensity = initialIntensity = intensity;

			ResetTintMap ();
		}


		private void OnLevelWasLoaded ()
		{
			ResetTintMap ();
		}


		/**
		 * Assigns the internal TintMap to follow based on the chosen public variables.
		 */
		public void ResetTintMap ()
		{
			actualTintMap = tintMap;

			if (useDefaultTintMap && KickStarter.sceneSettings)
			{
				if (KickStarter.sceneSettings.tintMap)
				{
					actualTintMap = KickStarter.sceneSettings.tintMap;
				}
				else
				{
					ACDebug.Log (this.gameObject.name + " cannot find Tint Map to follow!");
				}
			}
		}


		/**
		 * <summary>Changes the intensity of a TintMap's effect.</summary>
		 * <param name = "_targetIntensity">The new intensity value</param>
		 * <param name = "_fadeTime">The duration, in seconds, to change the intensity over. If = 0, the change will be instantaneous.</param>
		 */
		public void SetIntensity (float _targetIntensity, float _fadeTime = 0f)
		{
			targetIntensity = _targetIntensity;
			initialIntensity = intensity;

			if (_fadeTime <= 0)
			{
				intensity = _targetIntensity;
				fadeStartTime = 0f;
				fadeTime = _fadeTime;
			}
			else
			{
				fadeStartTime = Time.time;
				fadeTime = _fadeTime;
			}
		}


		private void Update ()
		{
			if (actualTintMap)
			{
				if (fadeTime > 0f)
				{
					intensity = Mathf.Lerp (initialIntensity, targetIntensity, AdvGame.Interpolate (fadeStartTime, fadeTime, MoveMethod.Linear, null));
					if (Time.time > (fadeStartTime + fadeTime))
					{
						intensity = targetIntensity;
						fadeTime = fadeStartTime = 0f;
					}
				}

				if (affectChildren)
				{
					foreach (SpriteRenderer spriteRenderer in _spriteRenderers)
					{
						spriteRenderer.color = actualTintMap.GetColorData (transform.position, intensity);
					}
				}
				else
				{
					_spriteRenderer.color = actualTintMap.GetColorData (transform.position, intensity);
				}
			}
		}


		/**
		 * <summary>Updates a VisibilityData class with it's own variables that need saving.</summary>
		 * <param name = "visibilityData">The original VisibilityData class</param>
		 * <returns>The updated VisibilityData class</returns>
		 */
		public VisibilityData SaveData (VisibilityData visibilityData)
		{
			visibilityData.useDefaultTintMap = useDefaultTintMap;
			visibilityData.tintIntensity = targetIntensity;

			visibilityData.tintMapID = 0;
			if (!useDefaultTintMap && tintMap != null && tintMap.gameObject != null)
			{
				visibilityData.tintMapID = Serializer.GetConstantID (tintMap.gameObject);
			}

			return visibilityData;
		}


		/**
		 * <summary>Updates it's own variables from a VisibilityData class.</summary>
		 * <param name = "data">The VisibilityData class to load from</param>
		 */
		public void LoadData (VisibilityData data)
		{
			useDefaultTintMap = data.useDefaultTintMap;
			SetIntensity (data.tintIntensity, 0f);

			if (!useDefaultTintMap && data.tintMapID != 0)
			{
				tintMap = Serializer.returnComponent <TintMap> (data.tintMapID);
			}

			ResetTintMap ();
		}

	}

}