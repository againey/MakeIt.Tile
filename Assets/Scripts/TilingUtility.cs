using UnityEngine;
/*
namespace Tiling
{
	public static class TilingUtility
	{
		public static void RelaxTilePositionsForRegularity(Topology topology, TileAttribute<Vector3> originalPositions, TileAttribute<Vector3> relaxedPositions)
		{
			System.Array.Clear(relaxedPositions._values, 0, relaxedPositions.Count);

			foreach (var tile in topology.Tiles)
			{
				foreach (var neighborTile in tile.Tiles)
				{
					relaxedPositions[tile] += originalPositions[neighborTile];
				}
				relaxedPositions[tile] = relaxedPositions[tile].normalized;
			}
		}

		public static void RelaxTilePositionsForEqualArea(Topology topology, TileAttribute<Vector3> originalPositions, TileAttribute<Vector3> relaxedPositions, float idealArea)
		{
			System.Array.Clear(relaxedPositions._values, 0, relaxedPositions.Count);

			foreach (var tile in topology.Tiles)
			{
				var centerPosition = originalPositions[tile];
				var prevNeighborPosition = originalPositions[tile.Tiles.Last];
				float surroundingArea = 0;
				foreach (var neighborTile in tile.Tiles)
				{
					var neighborPosition = originalPositions[neighborTile];
					surroundingArea += Vector3.Cross(neighborPosition - centerPosition, prevNeighborPosition - centerPosition).magnitude * 0.5f;
					prevNeighborPosition = neighborPosition;
				}
				surroundingArea /= 3f;
				var multiplier = idealArea / surroundingArea;
				foreach (var neighborTile in tile.Tiles)
				{
					var neighborPosition = originalPositions[neighborTile];
					relaxedPositions[neighborTile] += (neighborPosition - centerPosition) * multiplier + centerPosition;
				}
			}

			for (int i = 0; i < originalPositions.Count; ++i)
			{
				relaxedPositions[i] = relaxedPositions[i].normalized;
			}
		}

		public static bool ValidateAndRepairTilePositions(Topology topology, TileAttribute<Vector3> tilePositions, float adjustmentWeight)
		{
			bool repaired = false;
			float originalWeight = 1f - adjustmentWeight;
			foreach (var tile in topology.Tiles)
			{
				var centerPosition = tilePositions[tile];
				var neighborPosition0 = tilePositions[tile.PrevTile(tile.Tiles.Last)];
				var neighborPosition1 = tilePositions[tile.Tiles.Last];
				var centroid0 = (centerPosition + neighborPosition0 + neighborPosition1) / 3f;
				foreach (var neighborTile in tile.Tiles)
				{
					var neighborPosition2 = tilePositions[neighborTile];
					var centroid1 = (centerPosition + neighborPosition1 + neighborPosition2) / 3f;
					var normal = Vector3.Cross(centroid0 - centerPosition, centroid1 - centerPosition);
					if (Vector3.Dot(normal, centerPosition) < 0f) goto repair;
					neighborPosition0 = neighborPosition1;
					neighborPosition1 = neighborPosition2;
					centroid0 = centroid1;
				}
				continue;

				repair:
				repaired = true;
				var averageNeighborPosition = new Vector3(0f, 0f, 0f);
				foreach (var neighborTile in tile.Tiles)
				{
					averageNeighborPosition += tilePositions[neighborTile];
				}
				averageNeighborPosition /= tile.NeighborCount;
				tilePositions[tile] = (centerPosition * originalWeight + averageNeighborPosition * adjustmentWeight).normalized;
			}
			return !repaired;
		}
	}
}
*/