using UnityEngine;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Experilous.Topological;

public class CylindricalManifoldGenerationTests
{
#if false
	[Test]
	public void MinimalPointyTopTriGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifoldUtility.CreatePointyTopTriGridSphericalCylinder(2, 1, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold);
	}

	[Test]
	public void MinimalFlatTopTriGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifoldUtility.CreateFlatTopTriGridSphericalCylinder(3, 1, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold);
	}

	[Test]
	public void MinimalQuadGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifoldUtility.CreateQuadGridSphericalCylinder(3, 1, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold);
	}

	[Test]
	public void MinimalPointyTopHexGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifoldUtility.CreatePointyTopHexGridSphericalCylinder(3, 1, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold);
	}

	[Test]
	public void MinimalFlatTopHexGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifoldUtility.CreateFlatTopHexGridSphericalCylinder(4, 2, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold);
	}

	[Test]
	public void SmallPointyTopTriGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifoldUtility.CreatePointyTopTriGridSphericalCylinder(3, 2, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold);
	}

	[Test]
	public void SmallFlatTopTriGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifoldUtility.CreateFlatTopTriGridSphericalCylinder(4, 2, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold);
	}

	[Test]
	public void SmallQuadGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifoldUtility.CreateQuadGridSphericalCylinder(4, 2, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold);
	}

	[Test]
	public void SmallPointyTopHexGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifoldUtility.CreatePointyTopHexGridSphericalCylinder(3, 2, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold);
	}

	[Test]
	public void SmallFlatTopHexGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifoldUtility.CreateFlatTopHexGridSphericalCylinder(4, 4, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold);
	}

	[Test]
	public void LargePointyTopTriGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifoldUtility.CreatePointyTopTriGridSphericalCylinder(48, 24, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold);
	}

	[Test]
	public void LargeFlatTopTriGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifoldUtility.CreateFlatTopTriGridSphericalCylinder(48, 24, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold);
	}

	[Test]
	public void LargeQuadGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifoldUtility.CreateQuadGridSphericalCylinder(48, 24, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold);
	}

	[Test]
	public void LargePointyTopHexGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifoldUtility.CreatePointyTopHexGridSphericalCylinder(48, 24, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold);
	}

	[Test]
	public void LargeFlatTopHexGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifoldUtility.CreateFlatTopHexGridSphericalCylinder(48, 24, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold);
	}
#endif
}
