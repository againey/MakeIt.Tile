namespace Experilous.Topological
{
	public interface IFaceAttributeProvider<T> : IRefreshable where T : new()
	{
		FaceAttribute<T> attribute { get; }
	}
}
