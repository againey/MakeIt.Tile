/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

namespace Experilous.Topological
{
	public class FloatFaceAttribute : FaceArrayAttribute<float>
	{
		public static FloatFaceAttribute Create(float[] array) { return CreateDerived<FloatFaceAttribute>(array); }
		public static FloatFaceAttribute Create(int faceCount) { return CreateDerived<FloatFaceAttribute>(faceCount); }
	}
}
