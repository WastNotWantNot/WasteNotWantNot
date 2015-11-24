/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RuntimeActionList.cs"
 * 
 *	This is a special derivative of ActionList.
 *	It is used to run ActionList assets, which are assets defined outside of the scene.
 *	This type of asset's actions are copied here and run locally.
 *	When a ActionList asset is copied is copied from a menu, the menu it is called from is recorded, so that the game returns
 *	to the appropriate state after running.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	/**
	 * An ActionList subclass used to run ActionListAssets, which exist in asset files outside of the scene.
	 * When an ActionListAsset is run, it's Actions are copied to a new RuntimeActionList and run locally.
	 */
	public class RuntimeActionList : ActionList
	{

		/** The ActionListAsset that this ActionList's Actions are copied from */
		public ActionListAsset assetSource;


		/**
		 * <summary>Downloads and runs the settings and Actions stored within an ActionListAsset.</summary>
		 * <param name = "actionListAsset">The ActionListAsset to copy Actions from and run</param>
		 * <param name = "endConversation">If set, the supplied Conversation will be run when the AcionList ends</param>
		 * <param name = "i">The index number of the first Action to run</param>
		 * <param name = "doSkip">If True, then the Actions will be skipped, instead of run normally</param>
		 * <param name = "addToSkipQueue">If True, the ActionList will be skippable when the user presses 'EndCutscene'</param>
		 */
		public void DownloadActions (ActionListAsset actionListAsset, Conversation endConversation, int i, bool doSkip, bool addToSkipQueue)
		{
			this.name = actionListAsset.name;
			assetSource = actionListAsset;

			useParameters = actionListAsset.useParameters;
			parameters = actionListAsset.parameters;
			unfreezePauseMenus = actionListAsset.unfreezePauseMenus;

			actionListType = actionListAsset.actionListType;
			if (actionListAsset.actionListType == ActionListType.PauseGameplay)
			{
				isSkippable = actionListAsset.isSkippable;
			}
			else
			{
				isSkippable = false;
			}

			conversation = endConversation;
			actions.Clear ();
			
			foreach (AC.Action action in actionListAsset.actions)
			{
				ActionEnd _lastResult = action.lastResult;
				
				actions.Add (action);
				
				if (doSkip && action != null)
				{
					actions[actions.Count-1].lastResult = _lastResult;
				}
			}

			if (!useParameters)
			{
				foreach (Action action in actions)
				{
					action.AssignValues (null);
				}
			}

			if (doSkip)
			{
				Skip (i);
			}
			else
			{
				Interact (i, addToSkipQueue);
			}
		}


		/**
		 * Destroys itself.
		 */
		public void DestroySelf ()
		{
			Destroy (this.gameObject);
		}


		protected new void ReturnLastResultToSource (ActionEnd _lastResult, int i)
		{
			assetSource.actions[i].lastResult = _lastResult;
		}
	
	}

}
