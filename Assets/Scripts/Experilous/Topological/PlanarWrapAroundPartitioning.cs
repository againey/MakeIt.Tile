using UnityEngine;
using System;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[Serializable]
	public class PlanarWrapAroundPartitioning : PlanarPartitioning
	{
		[SerializeField] private Vector2 _origin;

		protected PlanarWrapAroundPartitioning Initialize(Topology topology, Vector3[] vertexPositions, Vector2 origin)
		{
			Initialize(topology, vertexPositions);
			_origin = origin;
			return this;
		}

		protected PlanarWrapAroundPartitioning Initialize(Topology topology, Vector3[] vertexPositions, Vector2 origin, string name)
		{
			Initialize(topology, vertexPositions, name);
			_origin = origin;
			return this;
		}

		public static new PlanarWrapAroundPartitioning CreateInstance(Topology topology, Vector3[] vertexPositions)
		{
			if (!(topology is PlanarWrapAroundManifold)) throw new ArgumentException("The provided topology was not an instance of PlanarWrapAroundManifold.", "manifold");
			return (PlanarWrapAroundPartitioning)CreateInstance<PlanarWrapAroundPartitioning>().Initialize(topology, vertexPositions);
		}

		public static new PlanarWrapAroundPartitioning CreateInstance(Topology topology, Vector3[] vertexPositions, string name)
		{
			if (!(topology is PlanarWrapAroundManifold)) throw new ArgumentException("The provided topology was not an instance of PlanarWrapAroundManifold.", "manifold");
			return (PlanarWrapAroundPartitioning)CreateInstance<PlanarWrapAroundPartitioning>().Initialize(topology, vertexPositions, name);
		}

		public static PlanarWrapAroundPartitioning CreateInstance(Topology topology, Vector3[] vertexPositions, Vector2 origin)
		{
			if (!(topology is PlanarWrapAroundManifold)) throw new ArgumentException("The provided topology was not an instance of PlanarWrapAroundManifold.", "manifold");
			return CreateInstance<PlanarWrapAroundPartitioning>().Initialize(topology, vertexPositions, origin);
		}

		public static PlanarWrapAroundPartitioning CreateInstance(Topology topology, Vector3[] vertexPositions, Vector2 origin, string name)
		{
			if (!(topology is PlanarWrapAroundManifold)) throw new ArgumentException("The provided topology was not an instance of PlanarWrapAroundManifold.", "manifold");
			return CreateInstance<PlanarWrapAroundPartitioning>().Initialize(topology, vertexPositions, origin, name);
		}

		protected override Vector2 GetVertexPosition(Topology.FaceEdge edge, Vector3[] vertexPositions)
		{
			return IgnoreZ((_topology as PlanarWrapAroundManifold).GetFaceVertexPosition(edge));
		}

		private Topology.Face Intersect(Vector2 point, Vector2[] repetitionAxes)
		{
			var offset = _origin + _origin - point;

			var nearestDistanceSquared = float.PositiveInfinity;
			var nearestRepetitionAxis = -1;
			var secondNearestDistanceSquared = float.PositiveInfinity;
			var secondNearestRepetitionAxis = -1;

			for (int i = 0; i < repetitionAxes.Length; ++i)
			{
				var distanceSquared = Vector2.SqrMagnitude(offset + repetitionAxes[i]);

				if (distanceSquared <= nearestDistanceSquared)
				{
					secondNearestDistanceSquared = nearestDistanceSquared;
					secondNearestRepetitionAxis = nearestRepetitionAxis;
					nearestDistanceSquared = distanceSquared;
					nearestRepetitionAxis = i;
				}
				else if (distanceSquared <= secondNearestDistanceSquared)
				{
					secondNearestDistanceSquared = distanceSquared;
					secondNearestRepetitionAxis = i;
				}
			}

			if (nearestRepetitionAxis != 0)
			{
				var face = Intersect(point - repetitionAxes[nearestRepetitionAxis], 0);
				if (face.isInternal) return face;
			}

			if (secondNearestRepetitionAxis != 0)
			{
				var face = Intersect(point - repetitionAxes[secondNearestRepetitionAxis], 0);
				if (face.isInternal) return face;
			}

			if (nearestRepetitionAxis != 0)
			{
				//Recurse with the point offset by the nearest repetition axis.
				return Intersect(point - repetitionAxes[nearestRepetitionAxis], repetitionAxes);
			}
			else if (secondNearestRepetitionAxis != 0)
			{
				//Recurse with the point offset by the nearest non-null repetition axis.
				return Intersect(point - repetitionAxes[secondNearestRepetitionAxis], repetitionAxes);
			}
			else
			{
				return new Topology.Face();
			}
		}

		public override Topology.Face Intersect(Vector2 point)
		{
			var face = Intersect(point, 0);
			if (face.isInternal)
			{
				return face;
			}
			else
			{
				return Intersect(point, (_topology as PlanarWrapAroundManifold).repetitionAxes);
			}
		}
	}
}
