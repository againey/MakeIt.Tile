using UnityEngine;
using System;

namespace Experilous.Topological
{
	public class ManifoldMeshGenerator : RefreshableCompoundMesh, ISerializationCallbackReceiver
	{
		[SerializeField]
		private Component _serializableManifoldGenerator;
		private IManifoldProvider _manifoldGenerator;

		public IManifoldProvider ManifoldGenerator
		{
			get { return _manifoldGenerator; }
			set { Refreshable.Rechain(ref _manifoldGenerator, value, this); }
		}

		[SerializeField]
		private Component _serializableCentroidGenerator;
		private IFaceAttributeProvider<Vector3> _centroidGenerator;

		public IFaceAttributeProvider<Vector3> CentroidGenerator
		{
			get { return _centroidGenerator; }
			set { Refreshable.Rechain(ref _centroidGenerator, value, this); }
		}

		[SerializeField]
		private Component _serializableFaceColors;
		private IFaceAttribute<Color> _faceColors;

		public IFaceAttribute<Color> FaceColors
		{
			get { return _faceColors; }
			set { _faceColors = Refreshable.Rechain(_faceColors, value, this); }
		}

		private void BuildSubmeshes(Topology.FacesIndexer faces, VertexAttribute<Vector3> vertexPositions)
		{
		}

		private void BuildSubmeshes(Topology.FacesIndexer faces, VertexAttribute<Vector3> vertexPositions, FaceAttribute<Vector3> centroidPositions)
		{
			var faceIndex = 0;
			var faceCount = faces.Count;
			while (faceIndex < faceCount)
			{
				var endFaceIndex = faceIndex;
				var meshVertexCount = 0;
				var meshTriangleCount = 0;
				while (endFaceIndex < faceCount)
				{
					var face = faces[endFaceIndex];
					var neighborCount = face.neighborCount;
					var faceVertexCount = neighborCount + 1;
					if (meshVertexCount + faceVertexCount > 65534) break;
					++endFaceIndex;
					meshVertexCount += faceVertexCount;
					meshTriangleCount += neighborCount;
				}

				Vector3[] vertices = new Vector3[meshVertexCount];
				Color[] colors = new Color[meshVertexCount];
				int[] triangles = new int[meshTriangleCount * 3];

				int meshVertex = 0;
				int meshTriangle = 0;

				while (faceIndex < endFaceIndex)
				{
					var face = faces[faceIndex];
					var edge = face.firstEdge;
					var neighborCount = face.neighborCount;
					vertices[meshVertex] = centroidPositions[faceIndex];
					colors[meshVertex] = (FaceColors != null) ? FaceColors[face] : new Color(1, 1, 1);
					for (int j = 0; j < neighborCount; ++j, edge = edge.next)
					{
						vertices[meshVertex + j + 1] = vertexPositions[edge.nextVertex];
						colors[meshVertex + j + 1] = new Color(0, 0, 0);
						triangles[meshTriangle + j * 3 + 0] = meshVertex;
						triangles[meshTriangle + j * 3 + 1] = meshVertex + 1 + j;
						triangles[meshTriangle + j * 3 + 2] = meshVertex + 1 + (j + 1) % neighborCount;

						if (meshVertex + 1 + j >= meshVertexCount || meshVertex + 1 + (j + 1) % neighborCount > meshVertexCount) throw new InvalidOperationException("Oops");
					}
					meshVertex += neighborCount + 1;
					meshTriangle += neighborCount * 3;
					++faceIndex;
				}

				PushSubmesh(vertices, colors, triangles);
			}
		}

		protected override void RefreshContent()
		{
			ClearSubmeshes();

			bool hasFaces = ManifoldGenerator != null && ManifoldGenerator.manifold != null && ManifoldGenerator.manifold.topology.faces.Count > 0;
			bool hasCentroids = CentroidGenerator != null && !CentroidGenerator.attribute.isEmpty;

			if (hasFaces && SubmeshPrefab != null)
			{
				if (hasCentroids)
				{
					BuildSubmeshes(ManifoldGenerator.manifold.topology.faces, ManifoldGenerator.manifold.vertexPositions, CentroidGenerator.attribute);
				}
				else
				{
					BuildSubmeshes(ManifoldGenerator.manifold.topology.faces, ManifoldGenerator.manifold.vertexPositions);
				}
			}

			ClearSubmeshCache();
		}

		private void OnEnable()
		{
			if (_manifoldGenerator != null) _manifoldGenerator.Refreshed += PropagateRefresh;
			if (_centroidGenerator != null) _centroidGenerator.Refreshed += PropagateRefresh;
			if (FaceColors != null) ((IRefreshable)FaceColors).Refreshed += PropagateRefresh;
		}

		private void OnDisable()
		{
			if (_manifoldGenerator != null) _manifoldGenerator.Refreshed -= PropagateRefresh;
			if (_centroidGenerator != null) _centroidGenerator.Refreshed -= PropagateRefresh;
			if (_faceColors != null) ((IRefreshable)_faceColors).Refreshed -= PropagateRefresh;
		}

		public void OnBeforeSerialize()
		{
			_serializableManifoldGenerator = (Component)_manifoldGenerator;
			_serializableCentroidGenerator = (Component)_centroidGenerator;
			_serializableFaceColors = (Component)_faceColors;
		}

		public void OnAfterDeserialize()
		{
			_manifoldGenerator = (_serializableManifoldGenerator != null ? (IManifoldProvider)_serializableManifoldGenerator : null);
			_centroidGenerator = (_serializableCentroidGenerator != null ? (IFaceAttributeProvider<Vector3>)_serializableCentroidGenerator : null);
			_faceColors = (_serializableFaceColors != null ? (IFaceAttribute<Color>)_serializableFaceColors : null);
		}
	}
}
