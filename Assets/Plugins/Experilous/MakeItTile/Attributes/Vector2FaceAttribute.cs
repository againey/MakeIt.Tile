/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;

namespace Experilous.MakeItTile
{
	public class Vector2FaceAttribute : FaceArrayAttribute<Vector2>
	{
		public static Vector2FaceAttribute Create(Vector2[] array) { return CreateDerived<Vector2FaceAttribute>(array); }
		public static Vector2FaceAttribute Create(int faceCount) { return CreateDerived<Vector2FaceAttribute>(faceCount); }
	}
}
