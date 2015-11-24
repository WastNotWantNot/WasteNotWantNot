/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberTrigger.cs"
 * 
 *	This script is attached to Trigger objects in the scene
 *	whose on/off state we wish to save. 
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	/**
	 * Attach this script to Trigger objects in the scene whose on/off state you wish to save.
	 */
	public class RememberTrigger : Remember
	{

		/** Whether the Trigger should be enabled or not when the game begins */
		public AC_OnOff startState = AC_OnOff.On;
		
		
		private void Awake ()
		{
			if (GameIsPlaying () && GetComponent <AC_Trigger>())
			{
				if (startState == AC_OnOff.On)
				{
					GetComponent <AC_Trigger>().TurnOn ();
				}
				else
				{
					GetComponent <AC_Trigger>().TurnOff ();
				}
			}
		}
		

		/**
		 * <summary>Serialises appropriate GameObject values into a string.</summary>
		 * <returns>The data, serialised as a string</returns>
		 */
		public override string SaveData ()
		{
			TriggerData triggerData = new TriggerData ();
			triggerData.objectID = constantID;

			if (GetComponent <Collider>())
			{
				triggerData.isOn = GetComponent <Collider>().enabled;
			}
			else if (GetComponent <Collider2D>())
			{
				triggerData.isOn = GetComponent <Collider2D>().enabled;
			}
			else
			{
				triggerData.isOn = false;
			}

			return Serializer.SaveScriptData <TriggerData> (triggerData);
		}
		

		/**
		 * <summary>Deserialises a string of data, and restores the GameObject to it's previous state.</summary>
		 * <param name = "stringData">The data, serialised as a string</param>
		 */
		public override void LoadData (string stringData)
		{
			TriggerData data = Serializer.LoadScriptData <TriggerData> (stringData);
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
	 * A data container used by the RememberTrigger script.
	 */
	[System.Serializable]
	public class TriggerData : RememberData
	{

		/** True if the Trigger is enabled */
		public bool isOn;


		/**
		 * The default Constructor.
		 */
		public TriggerData () { }

	}

}