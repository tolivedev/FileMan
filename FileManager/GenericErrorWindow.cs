using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager
{
    class GenericErrorWindow : PopupWindow
    {
        public string msg;
        public GenericErrorWindow() : base()
        {
            Background = ConsoleColor.Red;
            Foreground = ConsoleColor.White;
        }
        protected override void DrawInterior()
        {
            WriteLine(msg);
            WriteLine("Press any key to continue...");
        }
    }
}
