using UnityEngine;
using System;
using System.Collections.Generic;
using Tiling;
/*
public class SphereTilingPartition
{
	private struct PartitionedTile
	{
		public int _index;
		public Vector3 _position;
		public float _boundingRadius;

		public PartitionedTile(int index, Vector3 position, float boundingRadius)
		{
			_index = index;
			_position = position;
			_boundingRadius = boundingRadius;
		}
	}

	private class UpperComparer : IComparer<PartitionedTile>
	{
		public int Compare(PartitionedTile lhs, PartitionedTile rhs)
		{
			return Comparer<float>.Default.Compare(lhs._position.y, rhs._position.y);
		}
	}

	private class LowerComparer : IComparer<PartitionedTile>
	{
		public int Compare(PartitionedTile lhs, PartitionedTile rhs)
		{
			return Comparer<float>.Default.Compare(rhs._position.y, lhs._position.y);
		}
	}

	[Flags]
	private enum Octant
	{
		Right = 1,
		Left = 0,
		Upper = 2,
		Lower = 0,
		Far = 4,
		Near = 0,

		LeftLowerNear = Left | Lower | Near,
		RightLowerNear = Right | Lower | Near,
		LeftUpperNear = Left | Upper | Near,
		RightUpperNear = Right | Upper | Near,
		LeftLowerFar = Left | Lower | Far,
		RightLowerFar = Right | Lower | Far,
		LeftUpperFar = Left | Upper | Far,
		RightUpperFar = Right | Upper | Far,

		X = 1,
		Y = 2,
		Z = 4,

		XY = X | Y,
		XZ = X | Z,
		YZ = Y | Z,
	}

	private readonly Topology _topology;
	private readonly TileAttribute<Vector3> _tilePositions;
	private readonly CornerAttribute<Vector3> _cornerPositions;
	private readonly List<PartitionedTile>[] _octants = new List<PartitionedTile>[8];

	public SphereTilingPartition(Topology topology, TileAttribute<Vector3> tilePositions, CornerAttribute<Vector3> cornerPositions)
	{
		_topology = topology;
		_tilePositions = tilePositions;
		_cornerPositions = cornerPositions;

		for (int i = 0; i < 8; ++i)
		{
			_octants[i] = new List<PartitionedTile>();
		}

		foreach (var tile in topology.Tiles)
		{
			var tilePosition = tilePositions[tile];
			float boundingRadius = 0f;
			foreach (var corner in tile.Corners)
			{
				boundingRadius = Mathf.Max(boundingRadius, (tilePosition - cornerPositions[corner]).magnitude);
			}

			var partitionedTile = new PartitionedTile(tile.Index, tilePosition, boundingRadius);

			var octant = GetOctant(tilePosition);
			_octants[(int)octant].Add(partitionedTile);

			if ((octant & Octant.X) == Octant.Right && GetOctant(tilePosition + new Vector3(-boundingRadius, 0f, 0f)) != octant) _octants[(int)(octant ^ Octant.X)].Add(partitionedTile);
			if ((octant & Octant.X) == Octant.Left && GetOctant(tilePosition + new Vector3(+boundingRadius, 0f, 0f)) != octant) _octants[(int)(octant ^ Octant.X)].Add(partitionedTile);
			if ((octant & Octant.Y) == Octant.Upper && GetOctant(tilePosition + new Vector3(0f, -boundingRadius, 0f)) != octant) _octants[(int)(octant ^ Octant.Y)].Add(partitionedTile);
			if ((octant & Octant.Y) == Octant.Lower && GetOctant(tilePosition + new Vector3(0f, +boundingRadius, 0f)) != octant) _octants[(int)(octant ^ Octant.Y)].Add(partitionedTile);
			if ((octant & Octant.Z) == Octant.Far && GetOctant(tilePosition + new Vector3(0f, 0f, -boundingRadius)) != octant) _octants[(int)(octant ^ Octant.Z)].Add(partitionedTile);
			if ((octant & Octant.Z) == Octant.Near && GetOctant(tilePosition + new Vector3(0f, 0f, +boundingRadius)) != octant) _octants[(int)(octant ^ Octant.Z)].Add(partitionedTile);
			//TODO diagonals
		}

		var upperComparer = new UpperComparer();
		_octants[(int)Octant.RightUpperFar].Sort(upperComparer);
		_octants[(int)Octant.LeftUpperFar].Sort(upperComparer);
		_octants[(int)Octant.RightUpperNear].Sort(upperComparer);
		_octants[(int)Octant.LeftUpperNear].Sort(upperComparer);

		var lowerComparer = new LowerComparer();
		_octants[(int)Octant.RightLowerFar].Sort(lowerComparer);
		_octants[(int)Octant.LeftLowerFar].Sort(lowerComparer);
		_octants[(int)Octant.RightLowerNear].Sort(lowerComparer);
		_octants[(int)Octant.LeftLowerNear].Sort(lowerComparer);
	}

	private Octant GetOctant(Vector3 position)
	{
		return (position.x >= 0f ? Octant.Right : Octant.Left) | (position.y >= 0f ? Octant.Upper : Octant.Lower) | (position.z >= 0f ? Octant.Far : Octant.Near);
	}

	private Octant ReverseX(Octant octant)
	{
		return octant ^ Octant.X;
	}

	private Octant ReverseY(Octant octant)
	{
		return octant ^ Octant.Y;
	}

	private Octant ReverseZ(Octant octant)
	{
		return octant ^ Octant.Z;
	}

	public Tile Pick(Vector3 originRay)
	{
		return new Tile();
	}

	public Tile Pick(Ray ray)
	{
		return new Tile();
	}
}
*/