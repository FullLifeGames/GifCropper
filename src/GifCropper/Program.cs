using PhotoSauce.MagicScaler;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace GifCropper
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Execute this program with: 'GifCropper.exe {inPath} {outPath}'");
            }

            string inPath = args[0];
            string outPath = args[1];

            if (!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
            }

            var fileList = new string[] { inPath };
            if (File.GetAttributes(inPath).HasFlag(FileAttributes.Directory))
            {
                fileList = Directory.GetFiles(inPath);
            }

            foreach (var file in fileList)
            {
                var fileInfo = new FileInfo(file);

                if (fileInfo.Extension == ".gif")
                {
                    Console.WriteLine("Processing " + file);
                    var frameList = Gif.LoadAnimatedGif(file);

                    var leftMin = int.MaxValue;
                    var rightMax = 0;
                    var topMin = int.MaxValue;
                    var bottomMax = 0;

                    // Calculating the minimum left, top and max right, bottom, for knowing how to crop the gif
                    foreach (var frame in frameList)
                        SetDimensions(frame.Image, ref leftMin, ref rightMax, ref topMin, ref bottomMax);

                    var settings = new ProcessImageSettings { Crop = new Rectangle(leftMin, topMin, rightMax - leftMin, bottomMax - topMin), ResizeMode = CropScaleMode.Crop };

                    using var outStream = new FileStream(outPath + fileInfo.Name, FileMode.Create);
                    MagicImageProcessor.ProcessImage(file, outStream, settings);
                }
            }
        }

        // Adapted from: https://stackoverflow.com/a/10392379
        public static void SetDimensions(Bitmap bmp, ref int leftMin, ref int rightMax, ref int topMin, ref int bottomMax)
        {
            int w = bmp.Width;
            int h = bmp.Height;

            Func<int, bool> allWhiteRow = row =>
            {
                for (int i = 0; i < w; ++i)
                    if (bmp.GetPixel(i, row).A != 0)
                        return false;
                return true;
            };

            Func<int, bool> allWhiteColumn = col =>
            {
                for (int i = 0; i < h; ++i)
                    if (bmp.GetPixel(col, i).A != 0)
                        return false;
                return true;
            };

            int topmost = 0;
            for (int row = 0; row < h; ++row)
            {
                if (allWhiteRow(row))
                    topmost = row;
                else break;
            }

            int bottommost = 0;
            for (int row = h - 1; row >= 0; --row)
            {
                if (allWhiteRow(row))
                    bottommost = row;
                else break;
            }

            int leftmost = 0, rightmost = 0;
            for (int col = 0; col < w; ++col)
            {
                if (allWhiteColumn(col))
                    leftmost = col;
                else
                    break;
            }

            for (int col = w - 1; col >= 0; --col)
            {
                if (allWhiteColumn(col))
                    rightmost = col;
                else
                    break;
            }

            if (rightmost == 0) rightmost = w; // As reached left
            if (bottommost == 0) bottommost = h; // As reached top.

            int croppedWidth = rightmost - leftmost;
            int croppedHeight = bottommost - topmost;

            if (croppedWidth == 0) // No border on left or right
            {
                leftmost = 0;
                croppedWidth = w;
            }

            if (croppedHeight == 0) // No border on top or bottom
            {
                topmost = 0;
                croppedHeight = h;
            }

            if (leftmost < leftMin)
            {
                leftMin = leftmost;
            }
            if (rightmost > rightMax)
            {
                rightMax = rightmost;
            }
            if (topmost < topMin)
            {
                topMin = topmost;
            }
            if (bottommost > bottomMax)
            {
                bottomMax = bottommost;
            }
        }

        // Got from: https://www.titanwolf.org/Network/q/294c3504-cd64-4ec0-9353-42400013a081/y
        public class Gif
        {
            public static List<Frame> LoadAnimatedGif(string path)
            {
                //If path is not found, we should throw an IO exception
                if (!File.Exists(path))
                    throw new IOException("File does not exist");

                //Load the image
                var img = Image.FromFile(path);

                //Count the frames
                var frameCount = img.GetFrameCount(FrameDimension.Time);

                //List that will hold all the frames
                var frames = new List<Frame>();

                //Get the times stored in the gif
                //PropertyTagFrameDelay ((PROPID) 0x5100) comes from gdiplusimaging.h
                //More info on http://msdn.microsoft.com/en-us/library/windows/desktop/ms534416(v=vs.85).aspx
                var times = img.GetPropertyItem(0x5100).Value;

                //Convert the 4bit duration chunk into an int

                for (int i = 0; i < frameCount; i++)
                {
                    //convert 4 bit value to integer
                    var duration = BitConverter.ToInt32(times, 4 * i);

                    //Add a new frame to our list of frames
                    frames.Add(
                        new Frame()
                        {
                            Image = new Bitmap(img),
                            Duration = duration
                        });

                    //Set the write frame before we save it
                    img.SelectActiveFrame(FrameDimension.Time, i);


                }

                //Dispose the image when we're done
                img.Dispose();

                return frames;
            }
        }
        //Class to store each frame
        public class Frame
        {
            public Bitmap Image { get; set; }
            public int Duration { get; set; }
        }

    }
}
