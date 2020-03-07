# MakeIt.Tile

MakeIt.Tile is a framework for Unity to aid in the creation and usage of game
worlds which are divided up into discrete tiles. It is designed to be maximally
flexible, applying just as easily to the simple 8x8 game world of chess as to a
large hex grid in a strategy game or even an irregular arrangment of tiles on
the surface of a sphere. It includes support for:

* square, hex, and irregular grids
* planar and spherical surfaces
* world edge wrap-around
* mouse picking
* A\* path finding
* dynamic mesh generation
* neighboring tile visitation

The framework is very extensible, to support whatever tiling patterns, tile
attributes, and algorithms you might need.

## Getting Started

### Prerequisites

MakeIt.Tile depends on the following Unity packags:

* [MakeIt.Core](https://github.com/againey)
* [MakeIt.Random](https://github.com/againey)
* [Unity Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@latest/)

Optional dependencies will unlock additional features:

* [MakeIt.Colorful](https://github.com/againey) for random tile color generation

### Installing

This repository is arranged as a valid Unity package that can be imported into a
Unity project using the instructions in [the Unity Package Manager Manual](https://docs.unity3d.com/Manual/upm-ui-giturl.html).

### How MakeIt.Tile Is Organized

The core of the MakeIt.Tile framework is a Topology, a data structure for
representing the relations between vertices, edges, and faces. In terms of the
primary use case for this framework, faces are typically synonymous with tiles,
while vertices are the corners of each tile. Edges are the boundaries between
two adjacent faces/tiles, and they can simultaneously be treated as the
connections between two adjacent vertices/corners.

On top of this foundational data structure, Topology Attributes enable any kind
of data to be associated with the individual vertices, edges, and faces of a
topology. There are many different kinds of information that could be associated
with a topology, depending on the needs of your game or application. Elevation,
terrain type, a list of units, movement costs, surface normals, and more.

Surface Manifolds are an extension of the topology concept, embedding a topology
onto a two-dimensional surface within three-dimensional space. MakeIt.Tile
supports spherical and planar surfaces, constructible with a variety of tiling
patterns, including subdivisions of the five Platonic solids for spherical
surfaces, and highly flexible quadrilateral and hexagonal tiles for planar
surfaces. Surfaces are typically the first thing you construct, and which is
then used to build the full topology data structure and additional attributes
such as vertex positions.

Further utilities are provided such as the Dynamic Mesh for generating
renderable meshes of the tiled surfaces, Spatial Partitioning data structures
for quickly looking up elements of a topology nearest a point or intersected by
a ray, a Topology Traversal utilizing the A* path finding algorithm, a variety
of Topology Traversal for iterating over the elements of a topology in
particular orders, utilities for calculating additional common Topology
Attributes, and tools to Topology Mutation the shapes and arrangements of tiles
in a topology.

#### Note

The MakeIt.Tile library is located within the `MakeIt.Tile` namespace, with
supporting libraries and supplemental utilities in `MakeIt.Core`,
`MakeIt.Numerics`, `MakeIt.Containers`, `MakeIt.Random`, `MakeIt.Colorful`. Be
sure to put the appropriate using directives at the top of your scripts, or
explicitly qualify type names within your code. For namespaces that contain
types which might cause name ambiguities with types in Unity or third party
assets, consider using using aliases for just the types you need, rather than
pulling in an entire namespace, as in the following example:
```
using IntVector2 = Experilous.Numerics.IntVector2;
```

### Using MakeIt.Tile

MakeIt.Tile is a complex library with a lot of potential to be improved given
recent updates made by Unity. In the meantime, please refer to the online
[Make It Tile User's Manual](https://experilous.com/1/product/make-it-tile/2.0/documentation/)
for v2.0 from November 2016.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/your/project/tags). 

MakeIt.Tile was originally part of a collection of private projects in a
single repository, so some of that shared history still exists within this
repository. Releases for other projects before the split are also tagged, such
as `makeit-random-v1.0`.

## Authors

* **Andy Gainey**

## License

This project is licensed under the Apache License, Version 2.0 - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* Random engine state initialization uses the [FNV-1a](http://www.isthe.com/chongo/tech/comp/fnv/) hash function, developed by Glenn Fowler, Phong Vo, and Landon Curt Noll.
* The foundational work for multiple random engines is [Xorshift RNGs](https://www.jstatsoft.org/article/view/v008i14) by George Marsaglia.
* `XorShift128Plus` is an implementation of the XorShift modifications by Sebastiano Vigna in his paper [*Further scramblings of Marsaglia's xorshift generators*](http://vigna.di.unimi.it/ftp/papers/xorshiftplus.pdf).
* `XorShift1024Star` is an implementation of the XorShift modifications by Sebastiano Vigna, adapted from a [C code reference implementation](http://xoroshiro.di.unimi.it/xorshift1024star.c).
* `XorShiftAdd` is an implementation of the XorShift modifications by [Mutsuo Saito and Makoto Matsumoto](http://www.math.sci.hiroshima-u.ac.jp/~m-mat/MT/XSADD/).
* `XoroShiro128Plus` is an implementation of the XorShift modifications by David Blackman and Sebastiano Vigna, adapted from a [C code reference implementation](http://xorshift.di.unimi.it/xoroshiro128plus.c).
* `SplitMix64` is an implementation of the algorithm by Sebastiano Vigna, adapted from a [C code reference implementation](http://xorshift.di.unimi.it/splitmix64.c).
