using UnityEngine;

namespace Experilous.Topological
{
	public class ConstantColorFaceAttribute : FaceConstantAttribute<Color>
	{
		public static ConstantColorFaceAttribute Create(Color constant) { return CreateDerived<ConstantColorFaceAttribute>(constant); }
	}
}
