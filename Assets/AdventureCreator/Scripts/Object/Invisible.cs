/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"Invisible.cs"
 * 
 *	This script makes any gameObject it is attached to invisible.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	/**
	 * This script disables the Renderer component of any GameObject it is attached to, making it invisible.
	 */
	public class Invisible : MonoBehaviour
	{
		
		private void Awake ()
		{
			this.GetComponent <Renderer>().enabled = false;
		}

	}

}