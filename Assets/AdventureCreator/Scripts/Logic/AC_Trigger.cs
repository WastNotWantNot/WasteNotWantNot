/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Trigger.cs"
 * 
 *	This ActionList runs when the Player enters it.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	/**
	 * An ActionList that is run when the Player, or another object, comes into contact with it.
	 */
	[System.Serializable]
	public class AC_Trigger : ActionList
	{

		/** What the Trigger will react to (Player, SetObject, AnyObject, AnyObjectWithComponent) */
		public TriggerDetects detects = TriggerDetects.Player;
		/** The GameObject that the Trigger reacts to, if detects = TriggerDetects.SetObject */
		public GameObject obToDetect;
		/** The component that must be attached to an object for the Trigger to react to, if detects = TriggerDetects.AnyObjectWithComponent */
		public string detectComponent = "";

		/** What kind of contact the Trigger reacts to (0 = "On enter", 1 = "Continuous", 2 = "On exit") */
		public int triggerType;
		/** If True, then a Gizmo will be drawn in the Scene window at the Trigger's position */
		public bool showInEditor = false;
		/** If True, and the Player sets off the Trigger while walking towards a Hotspot Interaction, then the Player will stop, and the Interaction will be cancelled */
		public bool cancelInteractions = false;
		

		private void Interact (GameObject collisionOb)
		{
			if (cancelInteractions)
			{
				KickStarter.playerInteraction.StopMovingToHotspot ();
			}
			
			if (actionListType == ActionListType.PauseGameplay)
			{
				KickStarter.playerInteraction.DeselectHotspot (false);
			}
			
			// Set correct parameter
			if (useParameters && parameters.Count == 1)
			{
				parameters[0].gameObject = collisionOb;
			}
			
			Interact ();
		}
		
		
		private void OnTriggerEnter (Collider other)
		{
			if (triggerType == 0 && IsObjectCorrect (other.gameObject))
			{
				Interact (other.gameObject);
			}
		}
		
		
		private void OnTriggerEnter2D (Collider2D other)
		{
			if (triggerType == 0 && IsObjectCorrect (other.gameObject))
			{
				Interact (other.gameObject);
			}
		}
		
		
		private void OnTriggerStay (Collider other)
		{
			if (triggerType == 1 && IsObjectCorrect (other.gameObject))
			{
				Interact (other.gameObject);
			}
		}
		
		
		private void OnTriggerStay2D (Collider2D other)
		{
			if (triggerType == 1 && IsObjectCorrect (other.gameObject))
			{
				Interact (other.gameObject);
			}
		}
		
		
		private void OnTriggerExit (Collider other)
		{
			if (triggerType == 2 && IsObjectCorrect (other.gameObject))
			{
				Interact (other.gameObject);
			}
		}
		
		
		private void OnTriggerExit2D (Collider2D other)
		{
			if (triggerType == 2 && IsObjectCorrect (other.gameObject))
			{
				Interact (other.gameObject);
			}
		}


		/**
		 * <summary>Checks if the Trigger is enabled.</summary>
		 * <returns>True if the Trigger is enabled.</summary>
		 */
		public bool IsOn ()
		{
			if (GetComponent <Collider>())
			{
				return GetComponent <Collider>().enabled;
			}
			else if (GetComponent <Collider2D>())
			{
				return GetComponent <Collider2D>().enabled;
			}
			return false;
		}
		

		/**
		 * <summary>Enables the Trigger.</summary>
		 */
		public void TurnOn ()
		{
			if (GetComponent <Collider>())
			{
				GetComponent <Collider>().enabled = true;
			}
			else if (GetComponent <Collider2D>())
			{
				GetComponent <Collider2D>().enabled = true;
			}
			else
			{
				ACDebug.LogWarning ("Cannot turn " + this.name + " on because it has no Collider component.");
			}
		}
		

		/**
		 * <summary>Disables the Trigger.</summary>
		 */
		public void TurnOff ()
		{
			if (GetComponent <Collider>())
			{
				GetComponent <Collider>().enabled = false;
			}
			else if (GetComponent <Collider2D>())
			{
				GetComponent <Collider2D>().enabled = false;
			}
			else
			{
				ACDebug.LogWarning ("Cannot turn " + this.name + " off because it has no Collider component.");
			}
		}
		
		
		private bool IsObjectCorrect (GameObject obToCheck)
		{
			if (KickStarter.stateHandler.gameState != GameState.Normal || obToCheck == null)
			{
				return false;
			}

			if (KickStarter.stateHandler.AreTriggersDisabled ())
			{
				return false;
			}
			
			if (detects == TriggerDetects.Player)
			{
				if (obToCheck.CompareTag (Tags.player))
				{
					return true;
				}
			}
			else if (detects == TriggerDetects.SetObject)
			{
				if (obToDetect != null && obToCheck == obToDetect)
				{
					return true;
				}
			}
			else if (detects == TriggerDetects.AnyObjectWithComponent)
			{
				if (detectComponent != "" && obToCheck.GetComponent (detectComponent))
				{
					return true;
				}
			}
			else if (detects == TriggerDetects.AnyObjectWithTag)
			{
				if (detectComponent != "" && obToCheck.tag == detectComponent)
				{
					return true;
				}
			}
			else if (detects == TriggerDetects.AnyObject)
			{
				return true;
			}
			
			return false;
		}
		
		
		private void OnDrawGizmos ()
		{
			if (showInEditor)
			{
				DrawGizmos ();
			}
		}
		
		
		private void OnDrawGizmosSelected ()
		{
			DrawGizmos ();
		}
		
		
		private void DrawGizmos ()
		{
			if (GetComponent <PolygonCollider2D>())
			{
				AdvGame.DrawPolygonCollider (transform, GetComponent <PolygonCollider2D>(), new Color (1f, 0.3f, 0f, 0.8f));
			}
			else if (GetComponent <BoxCollider2D>() != null || GetComponent <BoxCollider>() != null)
			{
				AdvGame.DrawCubeCollider (transform, new Color (1f, 0.3f, 0f, 0.8f));
			}
		}

	}
	
}
