using UnityEngine;

namespace Experilous.Topological
{
	public class ColorFaceGroupAttribute : FaceGroupArrayAttribute<Color>, System.ICloneable
	{
		public static ColorFaceGroupAttribute CreateInstance() { return CreateDerivedInstance<ColorFaceGroupAttribute>(); }
		public static ColorFaceGroupAttribute CreateInstance(Color[] array) { return CreateDerivedInstance<ColorFaceGroupAttribute>(array); }
		public static ColorFaceGroupAttribute CreateInstance(Color[] array, string name) { return CreateDerivedInstance<ColorFaceGroupAttribute>(array, name); }
		public static new ColorFaceGroupAttribute CreateInstance(string name) { return CreateDerivedInstance<ColorFaceGroupAttribute>(name); }
		object System.ICloneable.Clone() { return Clone(); }
		public ColorFaceGroupAttribute Clone() { return CloneDerived<ColorFaceGroupAttribute>(); }
	}
}
