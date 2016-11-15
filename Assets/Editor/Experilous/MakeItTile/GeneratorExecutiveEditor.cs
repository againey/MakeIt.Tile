/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using UnityEditor;

namespace Experilous.Examples.MakeItTile
{
	[CustomEditor(typeof(GeneratorExecutive))]
	public class GeneratorExecutiveEditor : Editor
	{
		private GeneratorExecutive _executive;

		protected void OnEnable()
		{
			if (_executive == null)
			{
				if (!(target is GeneratorExecutive)) return;
				_executive = (GeneratorExecutive)target;
			}
		}

		public override void OnInspectorGUI()
		{
			if (GUILayout.Button(new GUIContent("Generate", "Executes all of the generators on this game object.")))
			{
				_executive.Generate();
			}
		}
	}
}
