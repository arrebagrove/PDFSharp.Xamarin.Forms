﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using ImageIO;
using Foundation;

namespace PdfSharp.Xamarin.Sample.iOS
{
	public class IosImageSource : ImageSource
	{
		private enum Orientation
		{
			Normal = 1,
			Rotate90 = 6,
			Rotate180 = 3,
			Rotate270 = 8
		}

		protected override IImageSource FromBinaryImpl(string name, Func<byte[]> imageSource, int? quality = 75)
		{
			return new IosImageSourceImpl(name, () => { return new MemoryStream(imageSource.Invoke()); }, (int)quality);
		}

		protected override IImageSource FromFileImpl(string path, int? quality = 75)
		{
			return new IosImageSourceImpl(path, () => { return File.OpenRead(path); }, (int)quality);
		}

		protected override IImageSource FromStreamImpl(string name, Func<Stream> imageStream, int? quality = 75)
		{
			return new IosImageSourceImpl(name, imageStream, (int)quality);
		}

		private class IosImageSourceImpl : IImageSource
		{
			internal UIKit.UIImage image;

			private Orientation Orientation { get; }

			private readonly Func<Stream> _streamSource;
			private readonly int _quality;

			public int Width { get; }
			public int Height { get; }
			public string Name { get; }


			public IosImageSourceImpl(string name, Func<Stream> streamSource, int quality)
			{
				Name = name;
				_streamSource = streamSource;
				_quality = quality;
				using (var stream = streamSource.Invoke())
				{
					image = UIKit.UIImage.LoadFromData(NSData.FromStream(stream));
					var size = image?.Size ?? new CoreGraphics.CGSize(0, 0);

					Width = (int)size.Width;
					Height = (int)size.Height;
					Orientation = Orientation.Normal;
				}
			}

			public void SaveAsJpeg(MemoryStream ms, CancellationToken ct)
			{
				var jpg = image.AsJPEG();
				ms.Write(jpg.ToArray(), 0, (int)jpg.Length);
				ms.Seek(0, SeekOrigin.Begin);
			}
		}
	}
}