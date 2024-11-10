using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Tetris
{
    public static class Images
    {
        public readonly static ImageSource Empty = LoadImage("Empty.png");
        public readonly static ImageSource I_Figure = LoadImage("I_Red.png");
        public readonly static ImageSource O_Figure = LoadImage("O_Yellow.png");
        public readonly static ImageSource J_Figure = LoadImage("J_Blue.png");
        public readonly static ImageSource L_Figure = LoadImage("L_Orange.png");
        public readonly static ImageSource S_Figure = LoadImage("S_Magenta.png");
        public readonly static ImageSource Z_Figure = LoadImage("Z_Green.png");
        public readonly static ImageSource T_Figure = LoadImage("T_Cyan.png");
        
        private static ImageSource LoadImage(string fileName)
        {
            return new BitmapImage(new Uri($"Images/{fileName}", UriKind.Relative));
        }
    }

}
