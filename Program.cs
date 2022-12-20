
using System.Text;

namespace BMP2HUB75
{
    internal class Program
    {
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

            var image = CreateBMP(imageData);
            StringBuilder sb = new StringBuilder();
            foreach (var point in image.data)
            {
                //todo create initializer for 2 bit image buffer or more bit buffer using sram at some point
            }
        }

        static BMP CreateBMP(byte[] data)
        {
            BMP imag = new();
            int i = 0;

            //file header setup
            imag.header.bfType = (ushort)(data[i] + (data[++i] << 8));
            imag.header.bfSize = (uint)(data[++i] + (data[++i] << 8) + (data[++i] << 16) + (data[++i] << 24));
            imag.header.bfReserved = 0; //reserved
            i += 4;
            imag.header.bfOffset = (uint)(data[++i] + (data[++i] << 8) + (data[++i] << 16) + (data[++i] << 24));

            //image data header setup
            imag.info.biSize = (uint)(data[++i] + (data[++i] << 8) + (data[++i] << 16) + (data[++i] << 24));
            imag.info.biWidth = data[++i] + (data[++i] << 8) + (data[++i] << 16) + (data[++i] << 24);
            imag.info.biHeight = data[++i] + (data[++i] << 8) + (data[++i] << 16) + (data[++i] << 24);
            imag.info.biPlanes = 1;
            i += 2;//unused
            imag.info.biBitCount = (ushort)(data[++i] + (data[++i] << 8));
            imag.info.biCompression = (uint)(data[++i] + (data[++i] << 8) + (data[++i] << 16) + (data[++i] << 24));
            imag.info.biSizeImage = (uint)(data[++i] + (data[++i] << 8) + (data[++i] << 16) + (data[++i] << 24));
            imag.info.biXPelsPerMeter = 0;
            i += 4;
            imag.info.biYPelsPerMeter = 0;
            i += 4;
            imag.info.biClrUsed = (uint)(data[++i] + (data[++i] << 8) + (data[++i] << 16) + (data[++i] << 24));
            imag.info.biClrImportant = (uint)(data[++i] + (data[++i] << 8) + (data[++i] << 16) + (data[++i] << 24));

            if (imag.info.biCompression == 3) Die("compressed bmp not supported!");

            //read in color table
            if (imag.info.biClrUsed == 0)
            {
                if (imag.info.biBitCount == 1 || imag.info.biBitCount == 2 || imag.info.biBitCount == 4)
                {
                    imag.colorTable = new uint[((int)Math.Pow(2, imag.info.biBitCount))];
                    for (int x = 0; x < ((int)Math.Pow(2, imag.info.biBitCount)); x++)
                    {
                        imag.colorTable[x] = (uint)(data[++i] + (data[++i] << 8) + (data[++i] << 16) + (data[++i] << 24));
                    }
                }
            }
            else
            {
                imag.colorTable = new uint[imag.info.biBitCount];
                for (int x = 0; x < imag.info.biBitCount; x++)
                {
                    imag.colorTable[x] = (uint)(data[++i] + (data[++i]<< 8) + (data[++i] << 16) + (data[++i] << 24));
                }
            }

            if ((i + 1) == imag.header.bfOffset) Console.WriteLine("Image headers read in correctly");
            imag.data = new byte[imag.info.biSizeImage];
            for (int t = 0; t < imag.info.biSizeImage; t++)
            {
                imag.data[t] = data[++i];
            }
            Console.WriteLine("Image data read in");

            return imag;
        }       

        static void Die(string message)
        {
            Console.WriteLine(message);
            while (Console.Read() == -1) { }

            Environment.Exit(0);
        }


        public struct BMP
        {
            public BITMAPFILEHEADER header;
            public BITMAPINFOHEADER info;
            public byte[] colorMasks;
            public uint[] colorTable;
            public byte[] data;
        }

        public struct BITMAPFILEHEADER
        {
            public ushort bfType;
            public uint bfSize;
            public uint bfReserved;
            public uint bfOffset;
        }

        public struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
            public ushort biStreamedData;
            public ushort biCompressionHeader;
        }
    }
}