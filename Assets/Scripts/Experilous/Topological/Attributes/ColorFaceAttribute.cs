using UnityEngine;

namespace Experilous.Topological
{
	public class ColorFaceAttribute : FaceArrayAttribute<Color>, System.ICloneable
	{
		public static ColorFaceAttribute CreateInstance() { return CreateDerivedInstance<ColorFaceAttribute>(); }
		public static ColorFaceAttribute CreateInstance(Color[] array) { return CreateDerivedInstance<ColorFaceAttribute>(array); }
		public static ColorFaceAttribute CreateInstance(Color[] array, string name) { return CreateDerivedInstance<ColorFaceAttribute>(array, name); }
		public static new ColorFaceAttribute CreateInstance(string name) { return CreateDerivedInstance<ColorFaceAttribute>(name); }
		object System.ICloneable.Clone() { return Clone(); }
		public ColorFaceAttribute Clone() { return CloneDerived<ColorFaceAttribute>(); }
	}
}
