using UnityEngine;

namespace Experilous.Topological
{
	public class ConstantColorFaceAttribute : FaceConstantAttribute<Color>, System.ICloneable
	{
		public static ConstantColorFaceAttribute CreateInstance() { return CreateDerivedInstance<ConstantColorFaceAttribute>(); }
		public static ConstantColorFaceAttribute CreateInstance(Color constant) { return CreateDerivedInstance<ConstantColorFaceAttribute>(constant); }
		public static ConstantColorFaceAttribute CreateInstance(Color constant, string name) { return CreateDerivedInstance<ConstantColorFaceAttribute>(constant, name); }
		public static new ConstantColorFaceAttribute CreateInstance(string name) { return CreateDerivedInstance<ConstantColorFaceAttribute>(name); }
		object System.ICloneable.Clone() { return Clone(); }
		public ConstantColorFaceAttribute Clone() { return CloneDerived<ConstantColorFaceAttribute>(); }
	}
}
