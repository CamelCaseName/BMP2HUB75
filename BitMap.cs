namespace BMP2HUB75.Bitmap
{
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

    internal class Bitmap
    {
        public BITMAPFILEHEADER header;
        public BITMAPINFOHEADER info;
        public byte[] colorMasks = Array.Empty<byte>();
        public uint[] colorTable = Array.Empty<uint>();
        public byte[] imageData = Array.Empty<byte>();

        public Bitmap(byte[] data)
        {

            int i = 0;

            //file header setup
            header.bfType = (ushort)(data[i] + (data[++i] << 8));
            header.bfSize = (uint)(data[++i] + (data[++i] << 8) + (data[++i] << 16) + (data[++i] << 24));
            header.bfReserved = 0; //reserved
            i += 4;
            header.bfOffset = (uint)(data[++i] + (data[++i] << 8) + (data[++i] << 16) + (data[++i] << 24));

            //image imageData header setup
            info.biSize = (uint)(data[++i] + (data[++i] << 8) + (data[++i] << 16) + (data[++i] << 24));
            info.biWidth = data[++i] + (data[++i] << 8) + (data[++i] << 16) + (data[++i] << 24);
            info.biHeight = data[++i] + (data[++i] << 8) + (data[++i] << 16) + (data[++i] << 24);
            info.biPlanes = 1;
            i += 2;//unused
            info.biBitCount = (ushort)(data[++i] + (data[++i] << 8));
            info.biCompression = (uint)(data[++i] + (data[++i] << 8) + (data[++i] << 16) + (data[++i] << 24));
            info.biSizeImage = (uint)(data[++i] + (data[++i] << 8) + (data[++i] << 16) + (data[++i] << 24));
            info.biXPelsPerMeter = 0;
            i += 4;
            info.biYPelsPerMeter = 0;
            i += 4;
            info.biClrUsed = (uint)(data[++i] + (data[++i] << 8) + (data[++i] << 16) + (data[++i] << 24));
            info.biClrImportant = (uint)(data[++i] + (data[++i] << 8) + (data[++i] << 16) + (data[++i] << 24));

            if (info.biCompression == 3) Program.Die("compressed bmp not supported!");

            //read in color table
            if (info.biClrUsed == 0)
            {
                if (info.biBitCount <= 8)
                {
                    colorTable = new uint[((int)Math.Pow(2, info.biBitCount))];
                    for (int x = 0; x < ((int)Math.Pow(2, info.biBitCount)); x++)
                    {
                        colorTable[x] = (uint)(data[++i] + (data[++i] << 8) + (data[++i] << 16) + (data[++i] << 24));
                    }
                }
            }
            else
            {
                colorTable = new uint[info.biBitCount];
                for (int x = 0; x < info.biBitCount; x++)
                {
                    colorTable[x] = (uint)(data[++i] + (data[++i] << 8) + (data[++i] << 16) + (data[++i] << 24));
                }
            }

            if ((i + 1) == header.bfOffset) Console.WriteLine("Image headers read in correctly");

            imageData = new byte[info.biSizeImage];
            for (int t = 0; t < info.biSizeImage; t++)
            {
                imageData[t] = data[++i];
            }

            Console.WriteLine("Image imageData read in");
        }
    }
}