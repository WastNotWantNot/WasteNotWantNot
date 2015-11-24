/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"SkipList.cs"
 * 
 *	This is a container for ActionList objects and assets than can be skipped or resumed at a later time.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	/**
	 * A data container for ActionList and ActionListAsset objects that can be skipped or resumed at a later time.
	 */
	public class SkipList
	{

		/** The ActionList this references */
		public ActionList actionList;
		/** The ActionListAsset this references */
		public ActionListAsset actionListAsset;
		/** The index number of the Action to skip from */
		public int startIndex;
		

		/**
		 * The default Constructor.
		 */
		public SkipList ()
		{
			actionList = null;
			actionListAsset = null;
			startIndex = 0;
		}
		

		/**
		 * <summary>A Constructor that copies the values of another SkipList.</summary>
		 * <param name = "_skipList">The SkipList to copy</param>
		 */
		public SkipList (SkipList _skipList)
		{
			actionList = _skipList.actionList;
			actionListAsset = _skipList.actionListAsset;
			startIndex = _skipList.startIndex;
		}
		

		/**
		 * <summary>A Constructor that assigns the variables explicitly.</summary>
		 * <param name = "_actionList">The ActionList this references. If it is a RuntimeActionList, it's assetSource will be assigned to actionListAsset.</param>
		 * <param name = "_startIndex">The index number of the Action to skip from</param>
		 */
		public SkipList (ActionList _actionList, int _startIndex)
		{
			actionList = _actionList;
			startIndex = _startIndex;
			
			if (_actionList is RuntimeActionList)
			{
				RuntimeActionList runtimeActionList = (RuntimeActionList) _actionList;
				actionListAsset = runtimeActionList.assetSource;
			}
			else
			{
				actionListAsset = null;
			}
		}
		

		/**
		 * Resumes the ActionList / ActionListAsset from startIndex.
		 */
		public void Resume ()
		{
			if (actionListAsset != null)
			{
				// Destroy old list, but don't go through ActionListManager's Reset code, to bypass changing GameState etc
				KickStarter.actionListManager.DestroyAssetList (actionListAsset);
				actionList = AdvGame.RunActionListAsset (actionListAsset, startIndex, true);
			}
			else if (actionList != null)
			{
				actionList.Interact (startIndex, true);
			}
		}
		

		/**
		 * Skips the ActionList / ActionListAsset from startIndex.
		 */
		public void Skip ()
		{
			if (actionListAsset != null)
			{
				// Destroy old list, but don't go through ActionListManager's Reset code, to bypass changing GameState etc
				KickStarter.actionListManager.DestroyAssetList (actionListAsset);
				actionList = AdvGame.SkipActionListAsset (actionListAsset, startIndex);
			}
			else if (actionList != null)
			{
				actionList.Skip (startIndex);
			}
		}
		

		/**
		 * <summary>Gets the name of the referenced ActionList / ActionListAsset.</summary>
		 * <returns>The name of the referenced ActionList / ActionListAsset</returns>
		 */
		public string GetName ()
		{
			if (actionListAsset != null)
			{
				return actionListAsset.name;
			}
			if (actionList != null)
			{
				return actionList.gameObject.name;
			}
			return "";
		}
		
	}

}