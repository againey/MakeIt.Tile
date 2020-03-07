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
using System.Collections.Generic;
using System;
using System.Collections;

namespace MakeIt.Tile
{
	/// <summary>
	/// A collection of face groups that also derives from <see cref="ScriptableObject"/>
	/// and can therefore be serialized, referenced, and turned into an asset by Unity.
	/// </summary>
	public class FaceGroupCollection : ScriptableObject, IEnumerable<IEnumerable<Topology.Face>>
	{
		/// <summary>
		/// The face groups contained within this collection.
		/// </summary>
		public FaceGroup[] faceGroups;

		/// <summary>
		/// Creates a face group collection with room for the specified number of face groups.
		/// </summary>
		/// <param name="faceGroupCount">The number of face groups to store in this collection.</param>
		/// <returns>A face group collectio with room for the specified number of face groups.</returns>
		public static FaceGroupCollection Create(int faceGroupCount)
		{
			var instance = CreateInstance<FaceGroupCollection>();
			instance.faceGroups = new FaceGroup[faceGroupCount];
			return instance;
		}

		/// <summary>
		/// Gets an enumerator over all the face groups in the current face group collection.
		/// </summary>
		/// <returns>An enumerator over all the face groups in the face group collection.</returns>
		public IEnumerator<IEnumerable<Topology.Face>> GetEnumerator()
		{
			foreach (var faceGroup in faceGroups)
			{
				yield return faceGroup;
			}
		}

		/// <summary>
		/// Gets an enumerator over all the face groups in the current face group collection.
		/// </summary>
		/// <returns>An enumerator over all the face groups in the face group collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
