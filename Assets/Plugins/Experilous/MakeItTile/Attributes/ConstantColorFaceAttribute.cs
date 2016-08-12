/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;

namespace Experilous.MakeIt.Tile
{
	public class ConstantColorFaceAttribute : FaceConstantAttribute<Color>
	{
		public static ConstantColorFaceAttribute Create(Color constant) { return CreateDerived<ConstantColorFaceAttribute>(constant); }
	}
}
