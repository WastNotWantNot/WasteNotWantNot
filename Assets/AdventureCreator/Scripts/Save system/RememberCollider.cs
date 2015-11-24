/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberCollider.cs"
 * 
 *	This script is attached to Colliders in the scene
 *	whose on/off state we wish to save. 
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	/**
	 * This script is attached to Colliders in the scene whose on/off state you wish to save.
	 */
	public class RememberCollider : Remember
	{

		/** Determines whether the Collider is on or off when the game begins */
		public AC_OnOff startState = AC_OnOff.On;
		
		
		private void Awake ()
		{
			if (KickStarter.settingsManager && GameIsPlaying ())
			{
				bool isOn = false;
				if (startState == AC_OnOff.On)
				{
					isOn = true;
				}

				if (GetComponent <Collider>())
				{
					GetComponent <Collider>().enabled = isOn;
				}

				else if (GetComponent <Collider2D>())
				{
					GetComponent <Collider2D>().enabled = isOn;
				}
			}
		}
		

		/**
		 * <summary>Serialises appropriate GameObject values into a string.</summary>
		 * <returns>The data, serialised as a string</returns>
		 */
		public override string SaveData ()
		{
			ColliderData colliderData = new ColliderData ();

			colliderData.objectID = constantID;
			colliderData.isOn = false;

			if (GetComponent <Collider>())
			{
				colliderData.isOn = GetComponent <Collider>().enabled;
			}
			else if (GetComponent <Collider2D>())
			{
				colliderData.isOn = GetComponent <Collider2D>().enabled;
			}

			return Serializer.SaveScriptData <ColliderData> (colliderData);
		}
		

		/**
		 * <summary>Deserialises a string of data, and restores the GameObject to it's previous state.</summary>
		 * <param name = "stringData">The data, serialised as a string</param>
		 */
		public override void LoadData (string stringData)
		{
			ColliderData data = Serializer.LoadScriptData <ColliderData> (stringData);
			if (data == null) return;

			if (GetComponent <Collider>())
			{
				GetComponent <Collider>().enabled = data.isOn;
			}
			else if (GetComponent <Collider2D>())
			{
				GetComponent <Collider2D>().enabled = data.isOn;
			}
		}

	}


	/**
	 * A data container used by the RememberCollider script.
	 */
	[System.Serializable]
	public class ColliderData : RememberData
	{

		/** True if the Collider is enabled */
		public bool isOn;

		/**
		 * The default Constructor.
		 */
		public ColliderData () { }
	}

}