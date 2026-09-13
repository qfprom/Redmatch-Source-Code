namespace Photon.Voice
{
	public class ImageBufferInfo
	{
		public int Width { get; private set; }

		public int Height { get; private set; }

		public int[] Stride { get; private set; }

		public ImageFormat Format { get; private set; }

		public Rotation Rotation { get; set; }

		public Flip Flip { get; set; }

		public ImageBufferInfo(int width, int height, int[] stride, ImageFormat format)
		{
			Width = width;
			Height = height;
			Stride = stride;
			Format = format;
		}
	}
}
