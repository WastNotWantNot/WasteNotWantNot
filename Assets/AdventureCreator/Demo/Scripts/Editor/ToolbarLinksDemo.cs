using UnityEngine;
using UnityEditor;
using System.Collections;

namespace AC
{

	public class ToolbarLinksDemo : EditorWindow
	{

		[MenuItem ("Adventure Creator/Getting started/Load 3D Demo managers", false, 6)]
		static void Demo3D ()
		{
			ManagerPackage package = AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Demo/ManagerPackage.asset", typeof (ManagerPackage)) as ManagerPackage;
			if (package != null)
			{
				package.AssignManagers ();
				AdventureCreator.RefreshActions ();
			}
		}

	}

}