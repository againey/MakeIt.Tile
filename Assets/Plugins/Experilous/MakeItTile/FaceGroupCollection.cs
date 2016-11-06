/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

namespace Experilous.MakeItTile
{
	public class FaceGroupCollection : ScriptableObject, IEnumerable<IEnumerable<Topology.Face>>
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

		public IEnumerator<IEnumerable<Topology.Face>> GetEnumerator()
		{
			foreach (var faceGroup in faceGroups)
			{
				yield return faceGroup;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
