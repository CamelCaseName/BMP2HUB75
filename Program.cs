using System.Text;

namespace BMP2HUB75
{
    internal class Program
    {
        //generated with wolframalpha "Table[round((1.01093222170971^x)-1), {x,0,255}]"
        //the constant is the 255th root of 16
        private static readonly byte[] rgb8to4LUT = new byte[256] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 9, 9, 9, 9, 9, 9, 9, 9, 9, 10, 10, 10, 10, 10, 10, 10, 10, 11, 11, 11, 11, 11, 11, 11, 11, 12, 12, 12, 12, 12, 12, 12, 13, 13, 13, 13, 13, 13, 14, 14, 14, 14, 14, 14, 14, 15, 15, 15 };

        private const int maxColorIter = 4;
        private const int maxColorDepth = maxColorIter - 1;

        static void Main(string[] args)
        {
            if (args.Length <= 0)
                Die("no arguments given");

            Console.WriteLine("opening image at " + args[0]);

            if (!File.Exists(args[0]))
                Die(args[0] + " does not exist!");

            var imageData = File.ReadAllBytes(args[0]);

            if (imageData[0] + (imageData[1] << 8) != 0x4D42)
                Die("not a bitmap!");

            var image = new Bitmap.Bitmap(imageData);

            StringBuilder sb = new StringBuilder();

            //todo create initializer for 1 and 2 bit image buffer or more bit buffer using sram at some point
            //todo do bit reduction using the percentage of image, so value between highest and lowest value in image so that most contrast and imageData is kept (if applicable)

            //**this is for 4 bit flash mode, so bibitcount >= 12**
            sb.Append("const unsigned char buffer[");
            sb.Append(image.info.biHeight * image.info.biWidth * maxColorIter / 2); //8 led per led struct
            sb.Append("] PROGMEM = { ");

            int pixelsSet = 0;

            for (int i = 0; i < maxColorIter; i++)
            {
                sb.Remove(sb.Length - 1, 1);
                sb.Append('\n');
                int height = image.info.biHeight / 2;
                int width = image.info.biWidth;

                if (image.info.biBitCount == 8)
                {
                    for (int index = 0; index < image.imageData.Length / 2; index++)
                    {
                        int redUpper = (int)((RGB888To444(image.colorTable[image.imageData[height * width - index]]) & (1 << maxColorDepth - i + 8)) >> maxColorDepth - i + 8);
                        int greenUpper = (int)((RGB888To444(image.colorTable[image.imageData[height * width - index]]) & (1 << maxColorDepth - i + 4)) >> maxColorDepth - i + 4);
                        int blueUpper = (int)((RGB888To444(image.colorTable[image.imageData[height * width - index]]) & (1 << maxColorDepth - i)) >> maxColorDepth - i);
                        int redLower = (int)((RGB888To444(image.colorTable[image.imageData[(image.imageData.Length / 2) + height * width - index]]) & (1 << maxColorDepth - i + 8)) >> maxColorDepth - i + 8);
                        int greenLower = (int)((RGB888To444(image.colorTable[image.imageData[(image.imageData.Length / 2) + height * width - index]]) & (1 << maxColorDepth - i + 4)) >> maxColorDepth - i + 4);
                        int blueLower = (int)((RGB888To444(image.colorTable[image.imageData[(image.imageData.Length / 2) + height * width - index]]) & (1 << maxColorDepth - i)) >> maxColorDepth - i);
                        sb.Append((redLower << 5) + (greenLower << 4) + (blueLower << 3) + (redUpper << 2) + (greenUpper << 1) + blueUpper);
                        sb.Append(',');
                        sb.Append(' ');

                        ++pixelsSet;

                        if (index % width == 0)
                            --height;
                    }
                }
                else
                {
                    for (int index = 0; index < image.imageData.Length / 6; index++)
                    {
                        if (index % 64 == 0)
                            sb.Append('\n');
                        if (index % width == 0)
                            --height;
                        int redUpper = (RGB8To4(image.imageData[(image.imageData.Length / 2) + (height * width + index % width) * 3 + 2]) & (1 << maxColorDepth - i)) >> maxColorDepth - i;
                        int greenUpper = (RGB8To4(image.imageData[(image.imageData.Length / 2) + (height * width + index % width) * 3 + 1]) & (1 << maxColorDepth - i)) >> maxColorDepth - i;
                        int blueUpper = (RGB8To4(image.imageData[(image.imageData.Length / 2) + (height * width + index % width) * 3]) & (1 << maxColorDepth - i)) >> maxColorDepth - i;
                        int redLower = (RGB8To4(image.imageData[(height * width + index % width) * 3 + 2]) & (1 << maxColorDepth - i)) >> maxColorDepth - i;
                        int greenLower = (RGB8To4(image.imageData[(height * width + index % width) * 3 + 1]) & (1 << maxColorDepth - i)) >> maxColorDepth - i;
                        int blueLower = (RGB8To4(image.imageData[(height * width + index % width) * 3]) & (1 << maxColorDepth - i)) >> maxColorDepth - i;
                        sb.Append((redLower << 5) + (greenLower << 4) + (blueLower << 3) + (redUpper << 2) + (greenUpper << 1) + blueUpper);
                        sb.Append(',');
                        sb.Append(' ');

                        ++pixelsSet;
                    }
                }
            }
            pixelsSet /= maxColorIter;
            pixelsSet *= 2;

            sb.Remove(sb.Length - 2, 2);
            sb.Append("\n\n};");

            Console.WriteLine($"Read and converted {pixelsSet} pixels at {image.info.biWidth}x{image.info.biHeight} and {maxColorIter} bits per channel");

            Console.WriteLine(sb.ToString());
        }

        public static uint RGB888To444(uint color) => (uint)((rgb8to4LUT[(color >> 16) & 255] << 8) + (rgb8to4LUT[(color >> 8) & 255] << 4) + rgb8to4LUT[color & 255]);
        public static byte RGB8To4(byte color) => rgb8to4LUT[color & 255];

        public static void Die(string message)
        {
            Console.WriteLine(message);
            while (Console.Read() == -1) { }

            Environment.Exit(0);
        }
    }
}