﻿/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

using UnityEngine;

namespace Experilous.Topological
{
	public class ConstantColorEdgeAttribute : EdgeConstantAttribute<Color>
	{
		public static ConstantColorEdgeAttribute Create(Color constant) { return CreateDerived<ConstantColorEdgeAttribute>(constant); }
	}
}