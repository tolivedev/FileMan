using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager
{
    class ErrorWindow : PopupWindow
    {
        public ErrorWindow() : base()
        {
            Background = ConsoleColor.Red;
            Foreground = ConsoleColor.White;
        }
        protected override void DrawInterior()
        {
            WriteLine("Command is incorrect.");
            WriteLine("Type 'help' or '/?' to list command.");
        }
    }
}
