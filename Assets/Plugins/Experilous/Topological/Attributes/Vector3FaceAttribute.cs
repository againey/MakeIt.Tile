/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

using UnityEngine;

namespace Experilous.Topological
{
	public class Vector3FaceAttribute : FaceArrayAttribute<Vector3>
	{
		public static Vector3FaceAttribute Create(Vector3[] array) { return CreateDerived<Vector3FaceAttribute>(array); }
		public static Vector3FaceAttribute Create(int faceCount) { return CreateDerived<Vector3FaceAttribute>(faceCount); }
	}
}
