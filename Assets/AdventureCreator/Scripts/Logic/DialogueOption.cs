/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"DialogueOption.cs"
 * 
 *	This ActionList is used by Conversations
 *	Each instance of the script handles a particular dialog option.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	/**
	 * An ActionList that is run when a Conversation's dialogue option is clicked on, unless the Conversation has been overridden with the "Dialogue: Start conversation" Action.
	 */
	[System.Serializable]
	public class DialogueOption : ActionList
	{ }
	
}