/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

namespace Experilous.Topological
{
	public class IntFaceAttribute : FaceArrayAttribute<int>
	{
		public static IntFaceAttribute Create(int[] array) { return CreateDerived<IntFaceAttribute>(array); }
		public static IntFaceAttribute Create(int faceCount) { return CreateDerived<IntFaceAttribute>(faceCount); }
	}
}
