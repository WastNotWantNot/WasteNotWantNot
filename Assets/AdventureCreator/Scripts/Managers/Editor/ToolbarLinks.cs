using UnityEngine;
using UnityEditor;
using System.Collections;

namespace AC
{

	public class ToolbarLinks : EditorWindow
	{

		[MenuItem ("Adventure Creator/Online resources/Website", false, 0)]
		static void Website ()
		{
			Application.OpenURL ("http://www.adventurecreator.org/");
		}


		[MenuItem ("Adventure Creator/Online resources/Tutorials", false, 1)]
		static void Tutorials ()
		{
			Application.OpenURL ("http://www.adventurecreator.org/tutorials/");
		}


		[MenuItem ("Adventure Creator/Online resources/Downloads", false, 2)]
		static void Downloads ()
		{
			Application.OpenURL ("http://www.adventurecreator.org/downloads/");
		}


		[MenuItem ("Adventure Creator/Online resources/Forum", false, 3)]
		static void Forum ()
		{
			Application.OpenURL ("http://www.adventurecreator.org/forum/");
		}


		[MenuItem ("Adventure Creator/Online resources/Scripting guide", false, 4)]
		static void ScriptingGuide ()
		{
			Application.OpenURL ("http://www.adventurecreator.org/scripting-guide/");
		}

	}

}