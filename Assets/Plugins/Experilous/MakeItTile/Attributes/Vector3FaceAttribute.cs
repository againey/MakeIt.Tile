/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;

namespace Experilous.MakeItTile
{
	public class Vector3FaceAttribute : FaceArrayAttribute<Vector3>
	{
		public static Vector3FaceAttribute Create(Vector3[] array) { return CreateDerived<Vector3FaceAttribute>(array); }
		public static Vector3FaceAttribute Create(int faceCount) { return CreateDerived<Vector3FaceAttribute>(faceCount); }
	}
}
