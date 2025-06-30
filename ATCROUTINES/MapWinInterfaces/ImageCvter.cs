using System;

namespace LegendControl
{
	using System.Drawing;
	using System.Runtime.InteropServices;

	public class ImageCvter : System.Windows.Forms.AxHost
	{

		public ImageCvter() : base("59EE46BA-677D-4d20-BF10-8D8067CB8B33")
		{
		}

		public System.Drawing.Image IPictureDispToImage(object image)
		{
			return System.Windows.Forms.AxHost.GetPictureFromIPicture(image);
		}
	}
}
