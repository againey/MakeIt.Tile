﻿/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

using UnityEngine;

namespace Experilous.Topological
{
	public class FaceGroupCollection : ScriptableObject
	{
		public FaceGroup[] faceGroups;

		public static FaceGroupCollection Create(int faceGroupCount)
		{
			var instance = CreateInstance<FaceGroupCollection>();
			instance.faceGroups = new FaceGroup[faceGroupCount];
			return instance;
		}

		public static FaceGroupCollection Create(int faceGroupCount, string name)
		{
			var instance = Create(faceGroupCount);
			instance.name = name;
			return instance;
		}
	}
}
