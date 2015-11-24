/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"GameCamera25D.cs"
 * 
 *	This GameCamera is fixed, but allows for a background image to be displayed.
 * 
 */

using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/**
	 * A fixed camera that allows for a BackgroundImage to be displayed underneath all scene objects.
	 */
	public class GameCamera25D : _Camera
	{

		/** The BackgroundImage to display underneath all scene objects. */
		public BackgroundImage backgroundImage;
		/** If True, then the MainCamera will copy it's position when the Inspector is viewed */
		public bool isActiveEditor = false;


		/**
		 * Enables the assigned backgroundImage, disables all other BackgroundImage objects, and ensures MainCamera can view it.
		 */
		public void SetActiveBackground ()
		{
			if (backgroundImage)
			{
				// Move background images onto correct layer
				BackgroundImage[] backgroundImages = FindObjectsOfType (typeof (BackgroundImage)) as BackgroundImage[];
				foreach (BackgroundImage image in backgroundImages)
				{
					if (image == backgroundImage)
					{
						image.TurnOn ();
					}
					else
					{
						image.TurnOff ();
					}
				}
				
				// Set MainCamera's Clear Flags
				KickStarter.mainCamera.PrepareForBackground ();
			}
		}


		new public void ResetTarget ()
		{}

	}
		
}