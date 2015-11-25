namespace Experilous
{
	public class NativeRandomEngine : BufferedRandomEngine
	{
		private System.Random _random;

		public NativeRandomEngine()
		{
			_random = new System.Random();
		}

		public NativeRandomEngine(int seed)
		{
			_random = new System.Random(seed);
		}

		public override uint Next()
		{
			byte[] buffer = new byte[4];
			_random.NextBytes(buffer);
			return System.BitConverter.ToUInt32(buffer, 0);
		}
	}
}
