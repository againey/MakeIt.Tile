namespace Experilous
{
	public interface IRandomEngine
	{
		uint Next();
		uint NextBits(int bitCount);
		uint NextLessThan(uint upperBound);
		uint NextLessThanOrEqual(uint upperBound);
	}
}
