/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberConversation.cs"
 * 
 *	This script is attached to conversation objects in the scene
 *	with DialogOption states we wish to save.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	/**
	 * Attach this script to Conversation objects in the scene with DialogOption states you wish to save.
	 */
	public class RememberConversation : Remember
	{

		/**
		 * <summary>Serialises appropriate GameObject values into a string.</summary>
		 * <returns>The data, serialised as a string</returns>
		 */
		public override string SaveData ()
		{
			ConversationData conversationData = new ConversationData();
			conversationData.objectID = constantID;

			if (GetComponent <Conversation>())
			{
				bool[] optionStates = GetComponent <Conversation>().GetOptionStates ();
				conversationData._optionStates = ArrayToString <bool> (optionStates);

				bool[] optionLocks = GetComponent <Conversation>().GetOptionLocks ();
				conversationData._optionLocks = ArrayToString <bool> (optionLocks);

				bool[] optionChosens = GetComponent <Conversation>().GetOptionChosens ();
				conversationData._optionChosens = ArrayToString <bool> (optionChosens);
			}

			return Serializer.SaveScriptData <ConversationData> (conversationData);
		}


		/**
		 * <summary>Deserialises a string of data, and restores the GameObject to it's previous state.</summary>
		 * <param name = "stringData">The data, serialised as a string</param>
		 */
		public override void LoadData (string stringData)
		{
			ConversationData data = Serializer.LoadScriptData <ConversationData> (stringData);
			if (data == null) return;

			if (GetComponent <Conversation>())
			{
				bool[] optionStates = StringToBoolArray (data._optionStates);
				GetComponent <Conversation>().SetOptionStates (optionStates);

				bool[] optionLocks = StringToBoolArray (data._optionLocks);
				GetComponent <Conversation>().SetOptionLocks (optionLocks);

				bool[] optionChosens = StringToBoolArray (data._optionChosens);
				GetComponent <Conversation>().SetOptionChosens (optionChosens);
			}
		}

	}


	/**
	 * A data container used by the RememberConversation script.
	 */
	[System.Serializable]
	public class ConversationData : RememberData
	{

		/** The enabled state of each DialogOption */
		public string _optionStates;
		/** The locked state of each DialogOption */
		public string _optionLocks;
		/** The 'already chosen' state of each DialogOption */
		public string _optionChosens;

		/**
		 * The default Constructor.
		 */
		public ConversationData () { }
	}

}