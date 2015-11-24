/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Conversation.cs"
 * 
 *	This script acts just like an ActionList,
 *	only it is a subclass so that other base classes
 *	(such as Button, DialogOption) cannot be referenced 
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	/**
	 * An ActionList that can run when the scene begins, loads, or whenver it is called from another Action.
	 * A delay can be assigned to it, so that it won't run immediately when called.
	 */
	[System.Serializable]
	public class Cutscene : ActionList
	{ }

}