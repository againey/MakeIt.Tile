namespace Experilous
{
	public interface IRandomEngine
	{
		uint Next32();
		uint Next32(int bitCount);

		ulong Next64();
		ulong Next64(int bitCount);

		uint NextLessThan(uint upperBound);
		uint NextLessThanOrEqual(uint upperBound);

		ulong NextLessThan(ulong upperBound);
		ulong NextLessThanOrEqual(ulong upperBound);
	}
}
