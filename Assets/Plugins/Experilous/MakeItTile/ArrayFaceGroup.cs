/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System;
using System.Collections.Generic;

namespace Experilous.MakeItTile
{
	public class ArrayFaceGroup : FaceGroup
	{
		[SerializeField] private Topology _topology;
		public int[] faceIndices;

		public static ArrayFaceGroup Create(Topology topology, int[] faceIndices)
		{
			var instance = CreateInstance<ArrayFaceGroup>();
			instance._topology = topology;
			instance.faceIndices = faceIndices;
			return instance;
		}

		public override int Count { get { return faceIndices.Length; } }

		protected override Topology topology { get { return _topology; } }

		public override Topology.Face this[int index]
		{
			get { return new Topology.Face(_topology, index); }
			set { throw new NotSupportedException(); }
		}

		public override bool Contains(int faceIndex)
		{
			foreach (var containedFaceIndex in faceIndices)
			{
				if (containedFaceIndex == faceIndex)
				{
					return true;
				}
			}
			return false;
		}

		public override IEnumerator<Topology.Face> GetEnumerator()
		{
			foreach (var faceIndex in faceIndices)
			{
				yield return new Topology.Face(_topology, faceIndex);
			}
		}
	}
}
