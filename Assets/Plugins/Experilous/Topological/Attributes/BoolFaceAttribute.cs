/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

namespace Experilous.Topological
{
	public class BoolFaceAttribute : FaceArrayAttribute<bool>
	{
		public static BoolFaceAttribute Create(bool[] array) { return CreateDerived<BoolFaceAttribute>(array); }
		public static BoolFaceAttribute Create(int faceCount) { return CreateDerived<BoolFaceAttribute>(faceCount); }
	}
}
