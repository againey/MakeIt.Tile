﻿namespace Experilous.Topological
{
	public class IntFaceAttribute : FaceArrayAttribute<int>, System.ICloneable
	{
		public static IntFaceAttribute CreateInstance() { return CreateDerivedInstance<IntFaceAttribute>(); }
		public static IntFaceAttribute CreateInstance(int[] array) { return CreateDerivedInstance<IntFaceAttribute>(array); }
		public static IntFaceAttribute CreateInstance(int[] array, string name) { return CreateDerivedInstance<IntFaceAttribute>(array, name); }
		public static new IntFaceAttribute CreateInstance(string name) { return CreateDerivedInstance<IntFaceAttribute>(name); }
		object System.ICloneable.Clone() { return Clone(); }
		public IntFaceAttribute Clone() { return CloneDerived<IntFaceAttribute>(); }
	}
}
