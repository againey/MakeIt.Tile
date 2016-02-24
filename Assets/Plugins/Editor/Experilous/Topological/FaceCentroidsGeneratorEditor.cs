/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

using UnityEditor;
using Experilous.Generation;

namespace Experilous.Topological
{
	[CustomEditor(typeof(FaceCentroidsGenerator))]
	public class FaceCentroidsGeneratorEditor : GeneratorEditor
	{
		protected override void OnPropertiesGUI()
		{
			var generator = (FaceCentroidsGenerator)target;

			InputSlotEditor.OnInspectorGUI(_serializedGenerator.FindProperty("surfaceInputSlot"));
			InputSlotEditor.OnInspectorGUI(_serializedGenerator.FindProperty("topologyInputSlot"));
			InputSlotEditor.OnInspectorGUI(_serializedGenerator.FindProperty("vertexPositionsInputSlot"));

			var surface = generator.surfaceInputSlot.GetAsset<Surface>();
			if (surface is SphericalSurface)
			{
				generator.flatten = EditorGUILayout.Toggle("Flatten", generator.flatten);
			}
		}
	}
}
