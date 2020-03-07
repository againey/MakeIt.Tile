/******************************************************************************\
* Copyright Andy Gainey                                                        *
*                                                                              *
* Licensed under the Apache License, Version 2.0 (the "License");              *
* you may not use this file except in compliance with the License.             *
* You may obtain a copy of the License at                                      *
*                                                                              *
*     http://www.apache.org/licenses/LICENSE-2.0                               *
*                                                                              *
* Unless required by applicable law or agreed to in writing, software          *
* distributed under the License is distributed on an "AS IS" BASIS,            *
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.     *
* See the License for the specific language governing permissions and          *
* limitations under the License.                                               *
\******************************************************************************/

using UnityEngine;
using System;
using System.Collections.Generic;

namespace MakeIt.Tile
{
	/// <summary>
	/// A face group that stores its collection of faces as an array of face indices.
	/// </summary>
	public class ArrayFaceGroup : FaceGroup
	{
		[SerializeField] private Topology _topology;

		/// <summary>
		/// The face indices of the faces within the current face group.
		/// </summary>
		public int[] faceIndices;

		/// <summary>
		/// Creates an array-based face group for faces in the specified topology, using the provided array of face indices.
		/// </summary>
		/// <param name="topology">The topology to which the faces of the face group belong.</param>
		/// <param name="faceIndices">The array of face indices of the faces that will be in the face group.</param>
		/// <returns>The face group containing the faces indicated.</returns>
		public static ArrayFaceGroup Create(Topology topology, int[] faceIndices)
		{
			var instance = CreateInstance<ArrayFaceGroup>();
			instance._topology = topology;
			instance.faceIndices = faceIndices;
			return instance;
		}

		/// <inheritdoc/>
		public override int Count { get { return faceIndices.Length; } }

		/// <inheritdoc/>
		protected override Topology topology { get { return _topology; } }

		/// <inheritdoc/>
		public override Topology.Face this[int index]
		{
			get { return new Topology.Face(_topology, faceIndices[index]); }
			set { throw new NotSupportedException(); }
		}

		/// <inheritdoc/>
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

		/// <inheritdoc/>
		public override IEnumerator<Topology.Face> GetEnumerator()
		{
			foreach (var faceIndex in faceIndices)
			{
				yield return new Topology.Face(_topology, faceIndex);
			}
		}
	}
}
