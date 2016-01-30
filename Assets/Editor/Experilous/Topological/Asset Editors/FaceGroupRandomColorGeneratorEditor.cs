using UnityEditor;

namespace Experilous.Topological
{
	[CustomEditor(typeof(FaceGroupRandomColorGenerator))]
	public class FaceGroupRandomColorGeneratorEditor : AssetGeneratorEditor
	{
		protected override void OnPropertiesGUI()
		{
			var generator = (FaceGroupRandomColorGenerator)target;

			OnDependencyGUI("Face Groups", generator.faceGroupCollectionInputSlot);
			OnDependencyGUI("Face Group Indices", generator.faceGroupIndicesInputSlot);
		}
	}
}
