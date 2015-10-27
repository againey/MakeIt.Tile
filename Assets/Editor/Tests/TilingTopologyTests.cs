using UnityEngine;
using NUnit.Framework;
using System;
using Tiling;

public class MeshTopologyTests
{
	private void Print(Tile tile)
	{
		var sb = new System.Text.StringBuilder();
		sb.AppendFormat("Tile Index: {0}\nNeighbors: {1}\n", tile.Index, tile.NeighborCount);
		for (int i = 0; i < tile.NeighborCount; ++i)
		{
			var neighborTile = tile.Tiles[i];
			var neighborEdge = tile.Edges[i];
			var neighborCorner = tile.Corners[i];
			sb.AppendFormat("Neighbor {0}:\n", i);
			sb.AppendFormat("\tTile (Index: {0})\n", neighborTile.Index);
			sb.Append("\t\tTiles:");
			foreach (var neighborNeighborTile in neighborTile.Tiles) sb.AppendFormat(" {0},", neighborNeighborTile.Index);
			sb.Append("\n");
			sb.Append("\t\tEdges:");
			foreach (var neighborNeighborEdge in neighborTile.Edges) sb.AppendFormat(" {0},", neighborNeighborEdge.Index);
			sb.Append("\n");
			sb.Append("\t\tCorners:");
			foreach (var neighborNeighborCorner in neighborTile.Corners) sb.AppendFormat(" {0},", neighborNeighborCorner.Index);
			sb.Append("\n");
			sb.AppendFormat("\tEdge (Index: {0})\n", neighborEdge.Index);
			sb.Append("\t\tTiles:");
			foreach (var neighborNeighborTile in neighborEdge.Tiles) sb.AppendFormat(" {0},", neighborNeighborTile.Index);
			sb.Append("\n");
			sb.Append("\t\tCorners:");
			foreach (var neighborNeighborCorner in neighborEdge.Corners) sb.AppendFormat(" {0},", neighborNeighborCorner.Index);
			sb.Append("\n");
			sb.AppendFormat("\tCorner (Index: {0})\n", neighborCorner.Index);
			sb.Append("\t\tTiles:");
			foreach (var neighborNeighborTile in neighborCorner.Tiles) sb.AppendFormat(" {0},", neighborNeighborTile.Index);
			sb.Append("\n");
			sb.Append("\t\tEdges:");
			foreach (var neighborNeighborEdge in neighborCorner.Edges) sb.AppendFormat(" {0},", neighborNeighborEdge.Index);
			sb.Append("\n");
			sb.Append("\t\tCorners:");
			foreach (var neighborNeighborCorner in neighborCorner.Corners) sb.AppendFormat(" {0},", neighborNeighborCorner.Index);
			sb.Append("\n");
		}
		Debug.Log(sb.ToString());
	}

	private void Print(Edge edge)
	{
		var sb = new System.Text.StringBuilder();
		sb.AppendFormat("Edge Index: {0}\nNeighbors: {1}\n", edge.Index, edge.NeighborCount);
		for (int i = 0; i < edge.NeighborCount; ++i)
		{
			var neighborTile = edge.Tiles[i];
			var neighborCorner = edge.Corners[i];
			sb.AppendFormat("Neighbor {0}:\n", i);
			sb.AppendFormat("\tTile (Index: {0})\n", neighborTile.Index);
			sb.Append("\t\tTiles:");
			foreach (var neighborNeighborTile in neighborTile.Tiles) sb.AppendFormat(" {0},", neighborNeighborTile.Index);
			sb.Append("\n");
			sb.Append("\t\tEdges:");
			foreach (var neighborNeighborEdge in neighborTile.Edges) sb.AppendFormat(" {0},", neighborNeighborEdge.Index);
			sb.Append("\n");
			sb.Append("\t\tCorners:");
			foreach (var neighborNeighborCorner in neighborTile.Corners) sb.AppendFormat(" {0},", neighborNeighborCorner.Index);
			sb.Append("\n");
			sb.AppendFormat("\tCorner (Index: {0})\n", neighborCorner.Index);
			sb.Append("\t\tTiles:");
			foreach (var neighborNeighborTile in neighborCorner.Tiles) sb.AppendFormat(" {0},", neighborNeighborTile.Index);
			sb.Append("\n");
			sb.Append("\t\tEdges:");
			foreach (var neighborNeighborEdge in neighborCorner.Edges) sb.AppendFormat(" {0},", neighborNeighborEdge.Index);
			sb.Append("\n");
			sb.Append("\t\tCorners:");
			foreach (var neighborNeighborCorner in neighborCorner.Corners) sb.AppendFormat(" {0},", neighborNeighborCorner.Index);
			sb.Append("\n");
		}
		Debug.Log(sb.ToString());
	}

	private void Print(Topology topology)
	{
		var sb = new System.Text.StringBuilder();
		sb.Append("Tile Offsets:");
		foreach (var offset in topology._tileNeighborOffsets) sb.AppendFormat(" {0},", offset);
		sb.Append("\n");
		sb.Append("Tile Tiles:");
		for (var i = 0; i < topology._tileNeighborOffsets.Length - 1; ++i)
		{
			sb.Append(" [");
			for (var j = topology._tileNeighborOffsets[i]; j < topology._tileNeighborOffsets[i + 1]; ++j)
				sb.AppendFormat(" {0},", topology._tileTiles[j]);
			sb.Append(" ],");
		}
		sb.Append("\n");
		sb.Append("Tile Edges:");
		for (var i = 0; i < topology._tileNeighborOffsets.Length - 1; ++i)
		{
			sb.Append(" [");
			for (var j = topology._tileNeighborOffsets[i]; j < topology._tileNeighborOffsets[i + 1]; ++j)
				sb.AppendFormat(" {0},", topology._tileEdges[j]);
			sb.Append(" ],");
		}
		sb.Append("\n");
		sb.Append("Tile Corners:");
		for (var i = 0; i < topology._tileNeighborOffsets.Length - 1; ++i)
		{
			sb.Append(" [");
			for (var j = topology._tileNeighborOffsets[i]; j < topology._tileNeighborOffsets[i + 1]; ++j)
				sb.AppendFormat(" {0},", topology._tileCorners[j]);
			sb.Append(" ],");
		}
		Debug.Log(sb.ToString());
	}

	private void ValidateInvariants(Tile tile)
	{
		for (int i = 0; i < tile.NeighborCount; ++i)
		{
			var neighborTile = tile.Tiles[i];
			var neighborEdge = tile.Edges[i];
			var neighborCorner = tile.Corners[i];

			// The tile neighbors are findable as neighbors.
			Assert.DoesNotThrow(() => { tile.NeighborIndexOf(neighborTile); }, string.Format("Tile invariant was violated.  (Neighbor {0})  The search for the neighbor tile failed.", i));
			Assert.DoesNotThrow(() => { tile.NeighborIndexOf(neighborEdge); }, string.Format("Tile invariant was violated.  (Neighbor {0})  The search for the neighbor edge failed.", i));
			Assert.DoesNotThrow(() => { tile.NeighborIndexOf(neighborCorner); }, string.Format("Tile invariant was violated.  (Neighbor {0})  The search for the neighbor corner failed.", i));

			// The tile neighbors have the expected neighbor index.
			Assert.That(tile.NeighborIndexOf(neighborTile) == i, string.Format("Tile invariant was violated.  (Neighbor {0})  The search for the neighbor tile did not produce the expected index.", i));
			Assert.That(tile.NeighborIndexOf(neighborEdge) == i, string.Format("Tile invariant was violated.  (Neighbor {0})  The search for the neighbor tile did not produce the expected index.", i));
			Assert.That(tile.NeighborIndexOf(neighborCorner) == i, string.Format("Tile invariant was violated.  (Neighbor {0})  The search for the neighbor tile did not produce the expected index.", i));

			// The tile is a neighbor of its neighbor objects.
			Assert.DoesNotThrow(() => { neighborTile.NeighborIndexOf(tile); }, string.Format("Tile invariant was violated.  (Neighbor {0})  The search for the original tile within the neighbor tile failed.", i));
			Assert.DoesNotThrow(() => { neighborEdge.NeighborIndexOf(tile); }, string.Format("Tile invariant was violated.  (Neighbor {0})  The search for the original tile within the neighbor edge failed.", i));
			Assert.DoesNotThrow(() => { neighborCorner.NeighborIndexOf(tile); }, string.Format("Tile invariant was violated.  (Neighbor {0})  The search for the original tile within the neighbor corner failed.", i));

			// The previous and next neighbor tiles are neighbors on the neighbor tile.
			Assert.DoesNotThrow(() => { neighborTile.NeighborIndexOf(tile.PrevTile(neighborTile)); }, string.Format("Tile invariant was violated.  (Neighbor {0})  The search for the previous neighbor tile within the neighbor tile failed.", i));
			Assert.DoesNotThrow(() => { neighborTile.NeighborIndexOf(tile.NextTile(neighborTile)); }, string.Format("Tile invariant was violated.  (Neighbor {0})  The search for the next neighbor tile within the neighbor tile failed.", i));

			// The previous and next neighbor corners are neighbors on the neighbor corner.
			Assert.DoesNotThrow(() => { neighborCorner.NeighborIndexOf(tile.PrevCorner(neighborCorner)); }, string.Format("Tile invariant was violated.  (Neighbor {0})  The search for the previous neighbor corner within the neighbor corner failed.", i));
			Assert.DoesNotThrow(() => { neighborCorner.NeighborIndexOf(tile.NextCorner(neighborCorner)); }, string.Format("Tile invariant was violated.  (Neighbor {0})  The search for the next neighbor corner within the neighbor corner failed.", i));

			Assert.That(neighborTile.NextCorner(tile.NextTile(neighborTile)) == neighborCorner, string.Format("Tile invariant was violated.  (Neighbor {0})  The neighbor corners of the neighbor tile were not aligned with the neighbor corners of this tile.", i));
		}
	}

	private void ValidateInvariants(Edge edge)
	{
		for (int i = 0; i < edge.NeighborCount; ++i)
		{
			var neighborTile = edge.Tiles[i];
			var neighborCorner = edge.Corners[i];

			// The edge neighbors are findable as neighbors.
			Assert.DoesNotThrow(() => { edge.NeighborIndexOf(neighborTile); }, "Edge invariant was violated.");
			Assert.DoesNotThrow(() => { edge.NeighborIndexOf(neighborCorner); }, "Edge invariant was violated.");

			// The edge neighbors have the expected neighbor index.
			Assert.That(edge.NeighborIndexOf(neighborTile) == i, "Edge invariant was violated.");
			Assert.That(edge.NeighborIndexOf(neighborCorner) == i, "Edge invariant was violated.");

			// The edge is a neighbor of its neighbor objects.
			Assert.DoesNotThrow(() => { neighborTile.NeighborIndexOf(edge); }, "Edge invariant was violated.");
			Assert.DoesNotThrow(() => { neighborCorner.NeighborIndexOf(edge); }, "Edge invariant was violated.");

			Assert.That(edge.NextCorner(neighborTile) == neighborCorner, "Edge invariant was violated.");
			Assert.That(edge.PrevCorner(neighborTile) == edge.OppositeCorner(neighborCorner), "Edge invariant was violated.");

			Assert.That(edge.NextTile(neighborCorner) == edge.OppositeTile(neighborTile), "Edge invariant was violated.");
			Assert.That(edge.PrevTile(neighborCorner) == neighborTile, "Edge invariant was violated.");

			Assert.That(edge.OppositeTile(neighborTile).AssociatedTile(edge) == neighborTile, "Edge invariant was violated.");
			Assert.That(edge.OppositeCorner(neighborCorner).AssociatedCorner(edge) == neighborCorner, "Edge invariant was violated.");
		}
	}

	private void ValidateInvariants(Corner corner)
	{
		for (int i = 0; i < corner.NeighborCount; ++i)
		{
			var neighborTile = corner.Tiles[i];
			var neighborEdge = corner.Edges[i];
			var neighborCorner = corner.Corners[i];

			// The corner neighbors are findable as neighbors.
			Assert.DoesNotThrow(() => { corner.NeighborIndexOf(neighborTile); }, "Corner invariant was violated.");
			Assert.DoesNotThrow(() => { corner.NeighborIndexOf(neighborEdge); }, "Corner invariant was violated.");
			Assert.DoesNotThrow(() => { corner.NeighborIndexOf(neighborCorner); }, "Corner invariant was violated.");

			// The corner neighbors have the expected neighbor index.
			Assert.That(corner.NeighborIndexOf(neighborTile) == i, "Corner invariant was violated.");
			Assert.That(corner.NeighborIndexOf(neighborEdge) == i, "Corner invariant was violated.");
			Assert.That(corner.NeighborIndexOf(neighborCorner) == i, "Corner invariant was violated.");

			// The corner is a neighbor of its neighbor objects.
			Assert.DoesNotThrow(() => { neighborTile.NeighborIndexOf(corner); }, "Corner invariant was violated.");
			Assert.DoesNotThrow(() => { neighborEdge.NeighborIndexOf(corner); }, "Corner invariant was violated.");
			Assert.DoesNotThrow(() => { neighborCorner.NeighborIndexOf(corner); }, "Corner invariant was violated.");

			// The neighbor tile relates properly to the neighbor edge and corner.
			Assert.That(corner.NextEdge(neighborTile) == neighborEdge, "Corner invariant was violated.");
			Assert.That(corner.NextCorner(neighborTile) == neighborCorner, "Corner invariant was violated.");
			Assert.That(corner.PrevTile(neighborEdge) == neighborTile, "Corner invariant was violated.");
			Assert.That(corner.PrevTile(neighborCorner) == neighborTile, "Corner invariant was violated.");

			// The neighbor edge and corner relate properly to each other.
			Assert.That(corner.AssociatedEdge(neighborCorner) == neighborEdge, "Corner invariant was violated.");
			Assert.That(corner.AssociatedCorner(neighborEdge) == neighborCorner, "Corner invariant was violated.");

			Assert.That(neighborTile.AssociatedTile(neighborEdge) == corner.NextTile(neighborTile), "Corner Invariant was violated.");
			Assert.That(neighborTile.AssociatedTile(corner.PrevEdge(neighborEdge)) == corner.PrevTile(neighborTile), "Corner Invariant was violated.");

			Assert.That(neighborTile.NextEdge(neighborEdge) == corner.PrevEdge(neighborEdge), "Corner Invariant was violated.");
			Assert.That(neighborTile.PrevEdge(corner.PrevEdge(neighborEdge)) == neighborEdge, "Corner Invariant was violated.");

			Assert.That(neighborTile.NextCorner(neighborCorner) == corner, "Corner Invariant was violated.");
			Assert.That(neighborTile.NextCorner(corner) == corner.PrevCorner(neighborCorner), "Corner Invariant was violated.");
			Assert.That(neighborTile.PrevCorner(corner.PrevCorner(neighborCorner)) == corner, "Corner Invariant was violated.");
			Assert.That(neighborTile.PrevCorner(corner) == neighborCorner, "Corner Invariant was violated.");
		}
	}

	private void ValidateInvariants(Topology topology)
	{
		foreach (var tile in topology.Tiles) try { ValidateInvariants(tile); } catch (Exception) { Print(tile); throw; }
		foreach (var edge in topology.Edges) try { ValidateInvariants(edge); } catch (Exception) { Print(edge); throw; }
		foreach (var corner in topology.Corners) ValidateInvariants(corner);
	}

	[Test]
	public void IcosahedronValidation()
	{
		MinimalTopology basicTopology;
		TileAttribute<Vector3> tilePositions;
		SphereTopology.Icosahedron(out basicTopology, out tilePositions);
		var topology = new Topology(basicTopology);
		ValidateInvariants(topology);
	}

	[Test]
	public void SubdividedIcosahedronValidation()
	{
		MinimalTopology basicTopology;
		TileAttribute<Vector3> tilePositions;
		SphereTopology.Icosahedron(out basicTopology, out tilePositions);
		var topology = new Topology(basicTopology);
		ValidateInvariants(new Topology(topology.Subdivide(1, (int count) => { }, (int lhs, int rhs) => { }, (int i, int j, int k, float t) => { })));
		ValidateInvariants(new Topology(topology.Subdivide(2, (int count) => { }, (int lhs, int rhs) => { }, (int i, int j, int k, float t) => { })));
		ValidateInvariants(new Topology(topology.Subdivide(5, (int count) => { }, (int lhs, int rhs) => { }, (int i, int j, int k, float t) => { })));
	}

	[Test]
	public void IcosahedronAlterTopologyValidation_SingleEdgeRotation()
	{
		MinimalTopology basicTopology;
		TileAttribute<Vector3> tilePositions;
		SphereTopology.Icosahedron(out basicTopology, out tilePositions);
		var topology = new Topology(basicTopology);
		ValidateInvariants(topology
			.AlterTopology(1,
			(Topology altered, Edge edge) =>
				{
					return edge.Index == 0;
				},
				(Topology altered) =>
				{
				}));
	}

	[Test]
	public void SubdividedIcosahedronAlterTopologyValidation_SingleEdgeRotation()
	{
		MinimalTopology basicTopology;
		TileAttribute<Vector3> tilePositions;
		SphereTopology.Icosahedron(out basicTopology, out tilePositions);
		var topology = new Topology(basicTopology);
		ValidateInvariants(new Topology(topology.Subdivide(1, (int count) => { }, (int lhs, int rhs) => { }, (int i, int j, int k, float t) => { }))
			.AlterTopology(1,
				(Topology altered, Edge edge) =>
				{
					return edge.Index == 5;
				},
				(Topology altered) =>
				{
				}));
	}

	[Test]
	public void SubdividedIcosahedronAlterTopologyValidation_MultipleEdgeRotations()
	{
		MinimalTopology basicTopology;
		TileAttribute<Vector3> tilePositions;
		SphereTopology.Icosahedron(out basicTopology, out tilePositions);
		var topology = new Topology(basicTopology);
		ValidateInvariants(new Topology(topology.Subdivide(1, (int count) => { }, (int lhs, int rhs) => { }, (int i, int j, int k, float t) => { }))
			.AlterTopology(1,
				(Topology altered, Edge edge) =>
				{
					return (edge.Index % 41 == 0 || edge.Index % 43 == 0);
				},
				(Topology altered) =>
				{
				}));
	}

	[Test]
	public void IcosahedronAlterTopologyValidation_MultipleEdgeRotations_MultiplePasses()
	{
		MinimalTopology basicTopology;
		TileAttribute<Vector3> tilePositions;
		SphereTopology.Icosahedron(out basicTopology, out tilePositions);
		var topology = new Topology(basicTopology);
		topology = topology
			.AlterTopology(3,
				(Topology altered, Edge edge) =>
				{
					return edge.Index == 0;
				},
				(Topology altered) =>
				{
					ValidateInvariants(altered);
				});
	}

	[Test]
	public void SubdividedIcosahedronAlterTopologyValidation_MultipleEdgeRotations_MultiplePasses()
	{
		MinimalTopology basicTopology;
		TileAttribute<Vector3> tilePositions;
		SphereTopology.Icosahedron(out basicTopology, out tilePositions);
		var topology = new Topology(basicTopology);
		topology = topology
			.AlterTopology(3,
				(Topology altered, Edge edge) =>
				{
					return edge.Index == 0;
				},
				(Topology altered) =>
				{
					ValidateInvariants(altered);
				});
	}
}
