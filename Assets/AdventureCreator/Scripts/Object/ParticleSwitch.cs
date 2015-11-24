/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ParticleSwitch.cs"
 * 
 *	This can be used, via the Object: Send Message Action,
 *	to turn it's attached particle systems on and off.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	/**
	 * This script provides functions to enable and disable the ParticleSystem component on the GameObject it is attached to.
	 * These functions can be called either through script, or with the "Object: Send message" Action.
	 */
	public class ParticleSwitch : MonoBehaviour
	{

		/** If True, then the Light component will be enabled when the game begins. */
		public bool enableOnStart = false;
		
		
		private void Awake ()
		{
			Switch (enableOnStart);
		}
		

		/**
		 * Enables the ParticleSystem component on the GameObject this script is attached to.
		 */
		public void TurnOn ()
		{
			Switch (true);
		}
		

		/**
		 * Disables the ParticleSystem component on the GameObject this script is attached to.
		 */
		public void TurnOff ()
		{
			Switch (false);
		}


		/**
		 * Causes the ParticleSystem component on the GameObject to emit it's maximum number of particles in one go.
		 */
		public void Interact ()
		{
			if (GetComponent <ParticleSystem>())
			{
				GetComponent <ParticleSystem>().Emit (GetComponent <ParticleSystem>().maxParticles);
			}
		}
		
		
		private void Switch (bool turnOn)
		{
			if (GetComponent <ParticleSystem>())
			{
				if (turnOn)
				{
					GetComponent <ParticleSystem>().Play ();
				}
				else
				{
					GetComponent <ParticleSystem>().Stop ();
				}
			}
		}
		
	}

}