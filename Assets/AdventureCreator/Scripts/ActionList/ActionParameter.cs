/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionParameter.cs"
 * 
 *	This defines a parameter that can be used by ActionLists
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	/**
	 * A data container for an ActionList parameter. A parameter can change the value of an Action's public variables dynamically during gameplay, allowing the same Action to be repurposed for different tasks.
	 */
	[System.Serializable]
	public class ActionParameter
	{

		/** The display name in the Editor */
		public string label = "";
		/** A unique identifier */
		public int ID = 0;
		/** The type of variable it overrides (GameObject, InventoryItem, GlobalVariable, LocalVariable, String, Float, Integer, Boolean) */
		public ParameterType parameterType = ParameterType.GameObject;
		/** The new value or ID number, if parameterType = ParameterType.Integer / Boolean / LocalVariable / GlobalVariable / InventoryItem.  If parameterType = ParameterType.GameObject, it is the ConstantID number of the GameObject if it is not currently accessible */
		public int intValue = -1;
		/** The new value, if parameterType = ParameterType.Float */
		public float floatValue = 0f;
		/** The new value, if parameterType = ParameterType.String */
		public string stringValue = "";
		/** The new value, if parameterType = ParameterType.GameObject */
		public GameObject gameObject;


		/**
		 * <summary>A Constructor that generates a unique ID number.</summary>
		 * <param name = "idArray">An array of previously-used ID numbers, to ensure it's own ID is unique.</param>
		 */
		public ActionParameter (int[] idArray)
		{
			label = "";
			ID = 0;
			intValue = -1;
			floatValue = 0f;
			stringValue = "";
			gameObject = null;
			parameterType = ParameterType.GameObject;
			
			// Update id based on array
			foreach (int _id in idArray)
			{
				if (ID == _id)
					ID ++;
			}
			
			label = "Parameter " + (ID + 1).ToString ();
		}


		/**
		 * <summary>A Constructor that sets the ID number explicitly.</summary>
		 * <param name = "id">The unique identifier to assign</param>
		 */
		public ActionParameter (int id)
		{
			label = "";
			ID = id;
			intValue = -1;
			floatValue = 0f;
			stringValue = "";
			gameObject = null;
			parameterType = ParameterType.GameObject;
			
			label = "Parameter " + (ID + 1).ToString ();
		}


		/**
		 * <summary>Copies the "value" variables from another ActionParameter, without changing the type, ID, or label.</summary>
		 * <parameter name = "otherParameter">The ActionParameter to copy from</param>
		 */
		public void CopyValues (ActionParameter otherParameter)
		{
			intValue = otherParameter.intValue;
			floatValue = otherParameter.floatValue;
			stringValue = otherParameter.stringValue;
			gameObject = otherParameter.gameObject;
		}


		/**
		 * Resets the value that the parameter assigns.
		 */
		public void Reset ()
		{
			intValue = -1;
			floatValue = 0f;
			stringValue = "";
			gameObject = null;
		}


		/**
		 * <summary>Sets the intValue that the parameter assigns</summary>
		 * <param name = "_value">The new value or ID number, if parameterType = ParameterType.Integer / Boolean / LocalVariable / GlobalVariable / InventoryItem.  If parameterType = ParameterType.GameObject, it is the ConstantID number of the GameObject if it is not currently accessible</param>
		 */
		public void SetValue (int _value)
		{
			intValue = _value;
			floatValue = 0f;
			stringValue = "";
			gameObject = null;
		}


		/**
		 * <summary>Sets the floatValue that the parameter assigns</summary>
		 * <param name = "_value">The new value, if parameterType = ParameterType.Float</param>
		 */
		public void SetValue (float _value)
		{
			floatValue = _value;
			stringValue = "";
			intValue = -1;
			gameObject = null;
		}


		/**
		 * <summary>Sets the stringValue that the parameter assigns</summary>
		 * <param name = "_value">The new value, if parameterType = ParameterType.String</param>
		 */
		public void SetValue (string _value)
		{
			stringValue = _value;
			floatValue = 0f;
			intValue = -1;
			gameObject = null;
		}


		/**
		 * <summary>Sets the gameObject that the parameter assigns</summary>
		 * <param name = "_object">The new GameObject, if parameterType = ParameterType.GameObject</param>
		 */
		public void SetValue (GameObject _object)
		{
			gameObject = _object;
			floatValue = 0f;
			stringValue = "";
			intValue = -1;
		}


		/**
		 * <summary>Sets the gameObject that the parameter assigns</summary>
		 * <param name = "_object">The new GameObject, if parameterType = ParameterType.GameObject</param>
		 * <param name = "_value">The GameObject's ConstantID number, which is used to find the GameObject if it is not always in the same scene as the ActionParameter class</param>
		 */
		public void SetValue (GameObject _object, int _value)
		{
			gameObject = _object;
			floatValue = 0f;
			stringValue = "";
			intValue = _value;
		}

	}

}