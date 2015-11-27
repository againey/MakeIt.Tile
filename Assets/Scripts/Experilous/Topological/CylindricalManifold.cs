using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public static class CylindricalManifold
	{
		public static Manifold CreatePointyTopTriGridCylinder(int columnCount, int rowCount, float maxLatitude, float regularity)
		{
			columnCount += columnCount % 2;
			var builder = new Topology.FaceVerticesBuilder(columnCount * rowCount, columnCount * rowCount * 3 + columnCount * 2);

			var columnPairCount = columnCount / 2;

			var vertexPositions = new Vector3[columnPairCount * (rowCount + 2)];

			for (int y = 0; y < rowCount; ++y)
			{
				var v0 = y * columnPairCount;
				var v1 = v0 + columnPairCount;
				var v2 = v1 + columnPairCount;
				if (y % 2 == 0)
				{
					for (int x = 0; x < columnPairCount; ++x)
					{
						builder.AddFace(v1 + x, v0 + x, v2 + x);
						builder.AddFace(v1 + (x + 1) % columnPairCount, v2 + x, v0 + x);
					}
				}
				else
				{
					for (int x = 0; x < columnPairCount; ++x)
					{
						builder.AddFace(v1 + x, v2 + x, v0 + x);
						builder.AddFace(v1 + x, v0 + (x + 1) % columnPairCount, v2 + (x + 1) % columnPairCount);
					}
				}
			}

			var longitudeMultiplier = Mathf.PI * -2f / columnPairCount;

			for (int y = 0; y <= rowCount + 1; ++y)
			{
				var relativeHeight = (y / (float)(rowCount + 1)) * 2f - 1f;
				var latitude = relativeHeight * maxLatitude; //for regularity == 0.5
				var absoluteHeight = Mathf.Sin(latitude);
				var xzRadius = Mathf.Cos(latitude);

				var v = y * columnPairCount;
				if (y % 2 == 0)
				{
					for (int x = 0; x < columnPairCount; ++x)
					{
						var longitude = (x + 0.5f) * longitudeMultiplier;
						vertexPositions[v + x] = new Vector3(Mathf.Cos(longitude) * xzRadius, absoluteHeight, Mathf.Sin(longitude) * xzRadius);
					}
				}
				else
				{
					for (int x = 0; x < columnPairCount; ++x)
					{
						var longitude = x * longitudeMultiplier;
						vertexPositions[v + x] = new Vector3(Mathf.Cos(longitude) * xzRadius, absoluteHeight, Mathf.Sin(longitude) * xzRadius);
					}
				}
			}

			return new Manifold(builder.BuildTopology(), vertexPositions);
		}

		public static Manifold CreateFlatTopTriGridCylinder(int columnCount, int rowCount, float maxLatitude, float regularity)
		{
			var builder = new Topology.FaceVerticesBuilder(columnCount * rowCount * 2, columnCount * rowCount * 6 + columnCount * 2);

			var vertexPositions = new Vector3[columnCount * rowCount + columnCount];

			for (int y = 0; y < rowCount; ++y)
			{
				var v0 = y * columnCount;
				var v1 = v0 + columnCount;
				for (int x = 1; x < columnCount; ++x)
				{
					var vShared0 = v0 + x;
					var vShared1 = v1 + x;
					builder.AddFace(vShared0 - 1, vShared0, vShared1 - 1);
					builder.AddFace(vShared1, vShared1 - 1, vShared0);
				}
				builder.AddFace(v0 + columnCount - 1, v0, v1 + columnCount - 1);
				builder.AddFace(v1, v1 + columnCount - 1, v0);
			}

			var longitudeMultiplier = Mathf.PI * -2f / columnCount;

			for (int y = 0; y <= rowCount; ++y)
			{
				var relativeHeight = (y / (float)rowCount) * 2f - 1f;
				var latitude = relativeHeight * maxLatitude; //for regularity == 0.5
				var absoluteHeight = Mathf.Sin(latitude);
				var xzRadius = Mathf.Cos(latitude);

				var v = y * columnCount;
				for (int x = 0; x < columnCount; ++x)
				{
					var longitude = (x + y * 0.5f) * longitudeMultiplier;
					vertexPositions[v + x] = new Vector3(Mathf.Cos(longitude) * xzRadius, absoluteHeight, Mathf.Sin(longitude) * xzRadius);
				}
			}

			return new Manifold(builder.BuildTopology(), vertexPositions);
		}

		public static Manifold CreateQuadGridCylinder(int columnCount, int rowCount, float maxLatitude, float regularity)
		{
			var builder = new Topology.FaceVerticesBuilder(columnCount * rowCount, columnCount * rowCount * 4 + columnCount * 2);

			var vertexPositions = new Vector3[columnCount * rowCount + columnCount];

			for (int y = 0; y < rowCount; ++y)
			{
				var v0 = y * columnCount;
				var v1 = v0 + columnCount;
				for (int x = 1; x < columnCount; ++x)
				{
					builder.AddFace(v0 + x - 1, v0 + x, v1 + x, v1 + x - 1);
				}
				builder.AddFace(v0 + columnCount - 1, v0, v1, v1 + columnCount - 1);
			}

			var longitudeMultiplier = Mathf.PI * -2f / columnCount;

			for (int y = 0; y <= rowCount; ++y)
			{
				var relativeHeight = (y / (float)rowCount) * 2f - 1f;
				var latitude = relativeHeight * maxLatitude; //for regularity == 0.5
				var absoluteHeight = Mathf.Sin(latitude);
				var xzRadius = Mathf.Cos(latitude);

				var v = y * columnCount;
				for (int x = 0; x < columnCount; ++x)
				{
					var longitude = x * longitudeMultiplier;
					vertexPositions[v + x] = new Vector3(Mathf.Cos(longitude) * xzRadius, absoluteHeight, Mathf.Sin(longitude) * xzRadius);
				}
			}

			return new Manifold(builder.BuildTopology(), vertexPositions);
		}

		public static Manifold CreatePointyTopHexGridCylinder(int columnCount, int rowCount, float maxLatitude, float regularity)
		{
			var builder = new Topology.FaceVerticesBuilder(columnCount * rowCount, columnCount * rowCount * 6 + columnCount * 4);
			var vertexPositions = new Vector3[columnCount * 2 * (rowCount + 1)];

			for (int y = 0; y < rowCount; ++y)
			{
				var v0 = y * columnCount * 2;
				var v1 = v0 + columnCount;
				var v2 = v1 + columnCount;
				var v3 = v2 + columnCount;
				builder.AddFace(v0, v1, v2, v3 + columnCount - 1, v2 + columnCount - 1, v1 + columnCount - 1);
				for (int x = 1; x < columnCount; ++x)
				{
					builder.AddFace(v0 + x, v1 + x, v2 + x, v3 + x - 1, v2 + x - 1, v1 + x - 1);
				}
			}

			var heightMultiplier = 3f / (rowCount * 3 + 1);
			var heightOffset = 1f / (rowCount * 3 + 1);
			var longitudeMultiplier = Mathf.PI * -2f / columnCount;

			for (int y = 0; y <= rowCount; ++y)
			{
				var relativeHeight = y * heightMultiplier;
				var latitude0 = (relativeHeight * 2f - 1f) * maxLatitude; //for regularity == 0.5
				var absoluteHeight0 = Mathf.Sin(latitude0);
				var xzRadius0 = Mathf.Cos(latitude0);
				var latitude1 = ((relativeHeight + heightOffset) * 2f - 1f) * maxLatitude; //for regularity == 0.5
				var absoluteHeight1 = Mathf.Sin(latitude1);
				var xzRadius1 = Mathf.Cos(latitude1);

				var v = y * columnCount * 2;
				for (int x = 0; x < columnCount; ++x)
				{
					var longitude = (x + y * 0.5f) * longitudeMultiplier;
					vertexPositions[v + x] = new Vector3(Mathf.Cos(longitude) * xzRadius0, absoluteHeight0, Mathf.Sin(longitude) * xzRadius0);
				}

				v += columnCount;
				for (int x = 0; x < columnCount; ++x)
				{
					var longitude = (x + 0.5f + y * 0.5f) * longitudeMultiplier;
					vertexPositions[v + x] = new Vector3(Mathf.Cos(longitude) * xzRadius1, absoluteHeight1, Mathf.Sin(longitude) * xzRadius1);
				}
			}

			return new Manifold(builder.BuildTopology(), vertexPositions);
		}

		public static Manifold CreateFlatTopHexGridCylinder(int columnCount, int rowCount, float maxLatitude, float regularity)
		{
			columnCount += columnCount % 2;
			var columnPairCount = columnCount / 2;

			var builder = new Topology.FaceVerticesBuilder(columnPairCount * rowCount, columnPairCount * rowCount * 6 + columnCount * 4);
			var vertexPositions = new Vector3[columnCount * (rowCount + 2)];

			for (int y = 0; y < rowCount; ++y)
			{
				var v0 = y * columnCount;
				var v1 = v0 + columnCount;
				var v2 = v1 + columnCount;
				if (y % 2 == 0)
				{
					for (int x = 0; x < columnCount; x += 2)
					{
						builder.AddFace(v1 + x, v0 + x, v0 + x + 1, v1 + x + 1, v2 + x + 1, v2 + x);
					}
				}
				else
				{
					for (int x = 2; x < columnCount; x += 2)
					{
						builder.AddFace(v1 + x - 1, v0 + x - 1, v0 + x, v1 + x, v2 + x, v2 + x - 1);
					}
					builder.AddFace(v1 + columnCount - 1, v0 + columnCount - 1, v0, v1, v2, v2 + columnCount - 1);
				}
			}

			var longitudeMultiplier = Mathf.PI * -2f / columnCount;
			var longitudeOffset = longitudeMultiplier / 3f;

			for (int y = 0; y <= rowCount + 1; ++y)
			{
				var relativeHeight = (y / (float)(rowCount + 1)) * 2f - 1f;
				var latitude = relativeHeight * maxLatitude; //for regularity == 0.5
				var absoluteHeight = Mathf.Sin(latitude);
				var xzRadius = Mathf.Cos(latitude);

				var v = y * columnCount;
				if (y % 2 == 0)
				{
					for (int x = 0; x < columnCount; x += 2)
					{
						var v0 = v + x;
						var longitude0 = x * longitudeMultiplier + longitudeOffset;
						vertexPositions[v0] = new Vector3(Mathf.Cos(longitude0) * xzRadius, absoluteHeight, Mathf.Sin(longitude0) * xzRadius);
						var longitude1 = (x + 1) * longitudeMultiplier;
						vertexPositions[v0 + 1] = new Vector3(Mathf.Cos(longitude1) * xzRadius, absoluteHeight, Mathf.Sin(longitude1) * xzRadius);
					}
				}
				else
				{
					for (int x = 0; x < columnCount; x += 2)
					{
						var v0 = v + x;
						var longitude0 = x * longitudeMultiplier;
						vertexPositions[v0] = new Vector3(Mathf.Cos(longitude0) * xzRadius, absoluteHeight, Mathf.Sin(longitude0) * xzRadius);
						var longitude1 = (x + 1) * longitudeMultiplier + longitudeOffset;
						vertexPositions[v0 + 1] = new Vector3(Mathf.Cos(longitude1) * xzRadius, absoluteHeight, Mathf.Sin(longitude1) * xzRadius);
					}
				}
			}

			return new Manifold(builder.BuildTopology(), vertexPositions);
		}
	}
}
