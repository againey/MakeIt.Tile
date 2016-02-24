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
	public class ColorFaceGroupLookupFaceAttribute : FaceGroupLookupFaceAttribute<Color, ColorFaceGroupAttribute>
	{
		public static ColorFaceGroupLookupFaceAttribute Create(IntFaceAttribute faceGroupIndices, ColorFaceGroupAttribute faceGroupColors)
		{
			var instance = CreateInstance<ColorFaceGroupLookupFaceAttribute>();
			instance.faceGroupData = faceGroupColors;
			instance.faceGroupIndices = faceGroupIndices;
			return instance;
		}
	}
}
