using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Experilous
{
	public abstract class AssetGenerator : ScriptableObject
	{
		public AssetGeneratorBundle bundle;
		public abstract IEnumerable<GeneratedAsset> dependencies { get; }
		public abstract IEnumerable<GeneratedAsset> outputs { get; }
		public abstract void ResetDependency(GeneratedAsset dependency);
		public abstract void Generate(string location, string name);

		public virtual bool CanGenerate()
		{
			return true;
		}

		public virtual void Reset()
		{
			foreach (var dependency in dependencies)
			{
				ResetDependency(dependency);
			}
		}

		protected bool ResetMemberDependency<TAsset>(GeneratedAsset dependency, ref TAsset member) where TAsset : GeneratedAsset
		{
			if (ReferenceEquals(dependency, member))
			{
				member = null;
				return true;
			}
			else
			{
				return false;
			}
		}

		protected bool ResetMemberDependency<TAsset0, TAsset1>(GeneratedAsset dependency, ref TAsset0 member0, ref TAsset1 member1)
			where TAsset0 : GeneratedAsset where TAsset1 : GeneratedAsset
		{
			return
				ResetMemberDependency(dependency, ref member0) ||
				ResetMemberDependency(dependency, ref member1);
		}

		protected bool ResetMemberDependency<TAsset0, TAsset1, TAsset2>(GeneratedAsset dependency, ref TAsset0 member0, ref TAsset1 member1, ref TAsset2 member2)
			where TAsset0 : GeneratedAsset where TAsset1 : GeneratedAsset where TAsset2 : GeneratedAsset
		{
			return
				ResetMemberDependency(dependency, ref member0) ||
				ResetMemberDependency(dependency, ref member1) ||
				ResetMemberDependency(dependency, ref member2);
		}

		protected bool ResetMemberDependency<TAsset0, TAsset1, TAsset2, TAsset3>(GeneratedAsset dependency, ref TAsset0 member0, ref TAsset1 member1, ref TAsset2 member2, ref TAsset3 member3)
			where TAsset0 : GeneratedAsset where TAsset1 : GeneratedAsset where TAsset2 : GeneratedAsset where TAsset3 : GeneratedAsset
		{
			return
				ResetMemberDependency(dependency, ref member0) ||
				ResetMemberDependency(dependency, ref member1) ||
				ResetMemberDependency(dependency, ref member2) ||
				ResetMemberDependency(dependency, ref member3);
		}

		public void EditScript()
		{
			AssetDatabase.OpenAsset(MonoScript.FromScriptableObject(this));
		}

		[MenuItem("CONTEXT/AssetGenerator/Reset", priority = 0)]
		public static void ResetFromMenu(MenuCommand menuCommand)
		{
			var generator = (AssetGenerator)menuCommand.context;
			generator.Reset();
		}

		[MenuItem("CONTEXT/AssetGenerator/Remove Generator", priority = 20)]
		public static void RemoveGenerator(MenuCommand menuCommand)
		{
			var generator = (AssetGenerator)menuCommand.context;
			generator.bundle.Remove(generator);
		}

		[MenuItem("CONTEXT/AssetGenerator/Remove Generator", validate = true)]
		public static bool CanRemoveGenerator(MenuCommand menuCommand)
		{
			var generator = (AssetGenerator)menuCommand.context;
			return generator.bundle.CanRemove(generator);
		}

		[MenuItem("CONTEXT/AssetGenerator/Move Up", priority = 21)]
		public static void MoveUp(MenuCommand menuCommand)
		{
			var generator = (AssetGenerator)menuCommand.context;
			generator.bundle.MoveUp(generator);
		}

		[MenuItem("CONTEXT/AssetGenerator/Move Up", validate = true)]
		public static bool CanMoveUp(MenuCommand menuCommand)
		{
			var generator = (AssetGenerator)menuCommand.context;
			return generator.bundle.CanMoveUp(generator);
		}

		[MenuItem("CONTEXT/AssetGenerator/Move Down", priority = 22)]
		public static void MoveDown(MenuCommand menuCommand)
		{
			var generator = (AssetGenerator)menuCommand.context;
			generator.bundle.MoveDown(generator);
		}

		[MenuItem("CONTEXT/AssetGenerator/Move Down", validate = true)]
		public static bool CanMoveDown(MenuCommand menuCommand)
		{
			var generator = (AssetGenerator)menuCommand.context;
			return generator.bundle.CanMoveDown(generator);
		}

		[MenuItem("CONTEXT/AssetGenerator/Edit Script", priority = 40)]
		public static void EditScript(MenuCommand menuCommand)
		{
			var generator = (AssetGenerator)menuCommand.context;
			generator.EditScript();
		}
	}
}
