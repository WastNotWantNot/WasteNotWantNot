/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"RememberNPC.cs"
 * 
 *	This script is attached to NPCs in the scene
 *	with path and transform data we wish to save.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	/**
	 * Attach this script to NPCs in the scene whose state you wish to save.
	 */
	public class RememberNPC : Remember
	{

		/** Determines whether the object is on or off when the game starts */
		public AC_OnOff startState = AC_OnOff.On;

		
		private void Awake ()
		{
			if (KickStarter.settingsManager && GetComponent <RememberHotspot>() == null && GameIsPlaying ())
			{
				if (startState == AC_OnOff.On)
				{
					this.gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.hotspotLayer);
				}
				else
				{
					this.gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.deactivatedLayer);
				}
			}
		}


		/**
		 * <summary>Serialises appropriate GameObject values into a string.</summary>
		 * <returns>The data, serialised as a string</returns>
		 */
		public override string SaveData ()
		{
			NPCData npcData = new NPCData();

			npcData.objectID = constantID;
			
			if (gameObject.layer == LayerMask.NameToLayer (KickStarter.settingsManager.hotspotLayer))
			{
				npcData.isOn = true;
			}
			else
			{
				npcData.isOn = false;
			}
			
			npcData.LocX = transform.position.x;
			npcData.LocY = transform.position.y;
			npcData.LocZ = transform.position.z;
			
			npcData.RotX = transform.eulerAngles.x;
			npcData.RotY = transform.eulerAngles.y;
			npcData.RotZ = transform.eulerAngles.z;
			
			npcData.ScaleX = transform.localScale.x;
			npcData.ScaleY = transform.localScale.y;
			npcData.ScaleZ = transform.localScale.z;
			
			if (GetComponent <NPC>())
			{
				NPC npc = GetComponent <NPC>();
				npcData = npc.SaveData (npcData);
			}
			
			return Serializer.SaveScriptData <NPCData> (npcData);
		}
		

		/**
		 * <summary>Deserialises a string of data, and restores the GameObject to it's previous state.</summary>
		 * <param name = "stringData">The data, serialised as a string</param>
		 */
		public override void LoadData (string stringData)
		{
			NPCData data = Serializer.LoadScriptData <NPCData> (stringData);
			if (data == null) return;

			if (data.isOn)
			{
				gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.hotspotLayer);
			}
			else
			{
				gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.deactivatedLayer);
			}
			transform.position = new Vector3 (data.LocX, data.LocY, data.LocZ);
			transform.eulerAngles = new Vector3 (data.RotX, data.RotY, data.RotZ);
			transform.localScale = new Vector3 (data.ScaleX, data.ScaleY, data.ScaleZ);
			
			if (GetComponent <NPC>())
			{
				NPC npc = GetComponent <NPC>();
				npc.SetRotation (transform.rotation);
				npc.LoadData (data);
			}
		}

	}


	/**
	 * A data container used by the RememberNPC script.
	 */
	[System.Serializable]
	public class NPCData : RememberData
	{

		/** True if the NPC is enabled */
		public bool isOn;

		/** The X position */
		public float LocX;
		/** The Y position */
		public float LocY;
		/** The Z position */
		public float LocZ;

		/** The X rotation */
		public float RotX;
		/** The Y rotation */
		public float RotY;
		/** The Z rotation */
		public float RotZ;

		/** The X scale */
		public float ScaleX;
		/** The Y scale */
		public float ScaleY;
		/** The Z scale */
		public float ScaleZ;

		/** The NPC's idle animation */
		public string idleAnim;
		/** The NPC's walk animation */
		public string walkAnim;
		/** The NPC's talk animation */
		public string talkAnim;
		/** The NPC's run animation */
		public string runAnim;

		/** A unique identifier for the NPC's walk sound AudioClip */
		public string walkSound;
		/** A unique identifier for the NPC's run sound AudioClip */
		public string runSound;
		/** A unique identifier for the NPC's portrait graphic */
		public string portraitGraphic;

		/** The NPC's walk speed */
		public float walkSpeed;
		/** The NPC's run speed */
		public float runSpeed;

		/** True if a sprite-based NPC is locked to face a particular direction */
		public bool lockDirection;
		/** The direction that a sprite-based NPC is facing */
		public string spriteDirection;
		/** True if a sprite-based NPC has it's scale locked */
		public bool lockScale;
		/** The scale of a sprite-based NPC */
		public float spriteScale;
		/** True if a sprite-based NPC has it's sorting locked */
		public bool lockSorting;
		/** The sorting order of a sprite-based NPC */
		public int sortingOrder;
		/** The sorting layer of a sprite-based NPC */
		public string sortingLayer;

		/** The Constant ID number of the NPC's current Path */
		public int pathID;
		/** The target node number of the NPC's current Path */
		public int targetNode;
		/** The previous node number of the NPC's current Path */
		public int prevNode;
		/** The positions of each node in a pathfinding-generated Path */
		public string pathData;
		/** True if the NPC is running */
		public bool isRunning;
		/** True if the NPC's current Path affects the Y position */
		public bool pathAffectY;

		/** The Constant ID number of the NPC's last-used Path */
		public int lastPathID;
		/** The target node number of the NPC's last-used Path */
		public int lastTargetNode;
		/** The previous node number of the NPC's last-used Path */
		public int lastPrevNode;

		/** The Constant ID number of the NPC's follow target */
		public int followTargetID = 0;
		/** True if the NPC is following the player */
		public bool followTargetIsPlayer = false;
		/** The frequency with which the NPC follows it's target */
		public float followFrequency = 0f;
		/** The distance that the NPC keeps with when following it's target */
		public float followDistance = 0f;
		/** The maximum distance that the NPC keeps when following it's target */
		public float followDistanceMax = 0f;

		/** True if the NPC's head is pointed towards a target */
		public bool isHeadTurning = false;
		/** The NPC's head target's X position */
		public float headTargetX = 0f;
		/** The NPC's head target's Y position */
		public float headTargetY = 0f;
		/** The NPC's head target's Z position */
		public float headTargetZ = 0f;

		/** The NPC's display name */
		public string speechLabel;

		/**
		 * The default Constructor.
		 */
		public NPCData () { }

	}

}