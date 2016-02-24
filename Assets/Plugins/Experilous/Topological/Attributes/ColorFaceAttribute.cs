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
	public class ColorFaceAttribute : FaceArrayAttribute<Color>
	{
		public static ColorFaceAttribute Create(Color[] array) { return CreateDerived<ColorFaceAttribute>(array); }
		public static ColorFaceAttribute Create(int faceCount) { return CreateDerived<ColorFaceAttribute>(faceCount); }
	}
}
