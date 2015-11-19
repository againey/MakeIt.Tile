using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[ExecuteInEditMode]
	public class CylinderGenerator : ManifoldGenerator
	{
		public CylinderStyle Style = CylinderStyle.QuadGrid;
		public int Columns = 24;
		public int Rows = 4;

		[Header("Spherical Projection")]
		public bool ProjectOntoSphere = true;
		public float MaxLatitude = 60;
		public float Regularity = 0.5f;

		public enum CylinderStyle
		{
			PointyTopTriGrid,
			FlatTopTriGrid,
			QuadGrid,
			PointyTopHexGrid,
			FlatTopHexGrid,
		}

		protected override Manifold RebuildManifold()
		{
			Manifold cylinder = null;

			if (ProjectOntoSphere)
			{
				switch (Style)
				{
					case CylinderStyle.PointyTopTriGrid: cylinder = CylindricalManifold.CreatePointyTopTriGridCylinder(Columns, Rows, MaxLatitude * Mathf.Deg2Rad, Regularity); break;
					case CylinderStyle.FlatTopTriGrid: cylinder = CylindricalManifold.CreateFlatTopTriGridCylinder(Columns, Rows, MaxLatitude * Mathf.Deg2Rad, Regularity); break;
					case CylinderStyle.QuadGrid: cylinder = CylindricalManifold.CreateQuadGridCylinder(Columns, Rows, MaxLatitude * Mathf.Deg2Rad, Regularity); break;
					case CylinderStyle.PointyTopHexGrid: cylinder = CylindricalManifold.CreatePointyTopHexGridCylinder(Columns, Rows, MaxLatitude * Mathf.Deg2Rad, Regularity); break;
					case CylinderStyle.FlatTopHexGrid: cylinder = CylindricalManifold.CreateFlatTopHexGridCylinder(Columns, Rows, MaxLatitude * Mathf.Deg2Rad, Regularity); break;
					default: throw new System.ArgumentException("A valid cylinder style must be selected.");
				}
			}

			return cylinder;
		}
	}
}
