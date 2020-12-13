using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace WebServer.HelpfulStaff
{
    public static class ConsoleColorPrinter
    {
        public static void WriteLine(string message, ConsoleColor textColor)
        {
            ConsoleColor pre = Console.ForegroundColor;
            Console.ForegroundColor = textColor;
            Console.WriteLine(message);
            Console.ForegroundColor = pre;
        }

        public static void Write(string message, ConsoleColor textColor)
        {
            ConsoleColor pre = Console.ForegroundColor;
            Console.ForegroundColor = textColor;
            Console.Write(message);
            Console.ForegroundColor = pre;
        }

        public static void WriteLine(string[] partsOfMessage, ConsoleColor[] partsColors)
        {
            ConsoleColor pre = Console.ForegroundColor;

            ConsoleColor[] colors = partsColors;
            if (colors.Length < partsOfMessage.Length)
            {
                colors = new ConsoleColor[partsOfMessage.Length];
                Array.Copy(partsColors, colors, partsColors.Length);
                for (int i = partsColors.Length; i < colors.Length; i++) 
                    colors[i] = partsColors[partsColors.Length - 1];
            }

            for (int i = 0; i < partsOfMessage.Length - 1; i++)
            {
                ConsoleColorPrinter.Write(partsOfMessage[i], colors[i]);
            }
            ConsoleColorPrinter.WriteLine(partsOfMessage[partsOfMessage.Length - 1], colors[partsOfMessage.Length - 1]);

            Console.ForegroundColor = pre;
        }

        public static void WriteTime(ConsoleColor color)
        {
            ConsoleColorPrinter.Write($"{DateTime.Now.ToString()} ", color);
        }

        public static void WriteLineWithTime(string message, ConsoleColor textColor, ConsoleColor timeColor)
        {
            ConsoleColorPrinter.WriteTime(timeColor);
            ConsoleColorPrinter.WriteLine(message, textColor);
        }
    }
}