namespace Experilous
{
	public interface IRandomEngine
	{
		void Seed();
		void Seed(int seed);
		void Seed(params int[] seed);
		void Seed(string seed);
		void Seed(IRandomEngine seeder);

		void MergeSeed();
		void MergeSeed(int seed);
		void MergeSeed(params int[] seed);
		void MergeSeed(string seed);
		void MergeSeed(IRandomEngine seeder);

		uint Next32();
		uint Next32(int bitCount);

		ulong Next64();
		ulong Next64(int bitCount);

		uint NextLessThan(uint upperBound);
		uint NextLessThanOrEqual(uint upperBound);

		ulong NextLessThan(ulong upperBound);
		ulong NextLessThanOrEqual(ulong upperBound);

		System.Random AsSystemRandom();
	}
}
