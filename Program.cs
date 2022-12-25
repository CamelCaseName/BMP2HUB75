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

            var image = new Bitmap.Bitmap(imageData);

            StringBuilder sb = new StringBuilder();
            foreach (var point in image.data)
            {
                //todo create initializer for 2 bit image buffer or more bit buffer using sram at some point
            }
        }

        public static void Die(string message)
        {
            Console.WriteLine(message);
            while (Console.Read() == -1) { }

            Environment.Exit(0);
        }
    }
}