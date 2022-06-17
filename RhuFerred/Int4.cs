namespace RhuFerred
{
	public struct Int4
	{
		public int x;
		public int y;
		public int z;
		public int w;

		public static Int4 Zero => new(0);
		public static Int4 One => new(1);

		public Int4(int val) : this() {
			x = val;
			y = val;
			z = val;
			w = val;
		}
		public Int4(int x, int y, int z, int w) : this() {
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}


	}
}