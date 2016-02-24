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
	public class ColorFaceGroupAttribute : FaceGroupArrayAttribute<Color>
	{
		public static ColorFaceGroupAttribute Create(Color[] array) { return CreateDerived<ColorFaceGroupAttribute>(array); }
		public static ColorFaceGroupAttribute Create(int groupCount) { return CreateDerived<ColorFaceGroupAttribute>(groupCount); }
	}
}
