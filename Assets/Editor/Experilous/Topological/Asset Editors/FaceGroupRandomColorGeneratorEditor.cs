using UnityEngine;
using UnityEditor;

namespace Experilous.Topological
{
	[CustomEditor(typeof(FaceGroupRandomColorGenerator))]
	public class FaceGroupRandomColorGeneratorEditor : AssetGeneratorEditor
	{
		protected override void OnPropertiesGUI()
		{
			var generator = (FaceGroupRandomColorGenerator)target;

			generator.faceGroupCollection = OnDependencyGUI("Face Groups", generator.faceGroupCollection, typeof(FaceGroupCollection), false);
			generator.faceGroupIndices = OnDependencyGUI("Face Group Indices", generator.faceGroupIndices, typeof(IFaceAttribute<int>), false);
		}
	}
}
