/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Experilous.Topologies;

namespace Experilous.MakeItTile
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
