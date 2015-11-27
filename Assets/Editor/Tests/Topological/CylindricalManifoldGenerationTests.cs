using UnityEngine;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Experilous.Topological;

public class CylindricalManifoldGenerationTests
{
	[Test]
	public void MinimalPointyTopTriGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifold.CreatePointyTopTriGridSphericalCylinder(2, 1, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold.topology);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold.topology);
	}

	[Test]
	public void MinimalFlatTopTriGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifold.CreateFlatTopTriGridSphericalCylinder(3, 1, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold.topology);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold.topology);
	}

	[Test]
	public void MinimalQuadGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifold.CreateQuadGridSphericalCylinder(3, 1, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold.topology);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold.topology);
	}

	[Test]
	public void MinimalPointyTopHexGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifold.CreatePointyTopHexGridSphericalCylinder(3, 1, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold.topology);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold.topology);
	}

	[Test]
	public void MinimalFlatTopHexGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifold.CreateFlatTopHexGridSphericalCylinder(4, 2, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold.topology);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold.topology);
	}

	[Test]
	public void SmallPointyTopTriGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifold.CreatePointyTopTriGridSphericalCylinder(3, 2, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold.topology);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold.topology);
	}

	[Test]
	public void SmallFlatTopTriGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifold.CreateFlatTopTriGridSphericalCylinder(4, 2, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold.topology);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold.topology);
	}

	[Test]
	public void SmallQuadGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifold.CreateQuadGridSphericalCylinder(4, 2, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold.topology);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold.topology);
	}

	[Test]
	public void SmallPointyTopHexGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifold.CreatePointyTopHexGridSphericalCylinder(3, 2, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold.topology);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold.topology);
	}

	[Test]
	public void SmallFlatTopHexGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifold.CreateFlatTopHexGridSphericalCylinder(4, 4, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold.topology);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold.topology);
	}
}
