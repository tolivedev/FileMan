using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager
{
    class Help1 : PopupWindow
    {
        public Help1() : base()
        {
            H = 20;
        }
        protected override void DrawInterior()
        {
            WriteLineCentered("Command List:");
            WriteLine("ls : List directories and files.");
            WriteLine("   type 'ls' to list all.");
            WriteLine("   use 'ls -p Number' to start from page <Number>.");
            WriteLine("   use 'ls -f' to list only files.");
            WriteLine("   use 'ls -d' to list only directories.");
            WriteLine("md : Make new directory.");
            WriteLine("   type 'md new_directory'.");
            WriteLine("cd : Change current directory.");
            WriteLine("   type 'cd new_directory'");
            WriteLine("rm : Remove directory or file.");
            WriteLine("   type 'rm file_name' or 'rm directory_name'.");
            WriteLine("mv : Move directory or file.");
            WriteLine("   type 'mv file_name new_location'.");
            WriteLine("cp : Copy directory or file.");
            WriteLine("   type 'cp file_name new_location'.");
            WriteLine("tp : Print file on screen.");
            WriteLine("   type 'tp file_name'.");
        }
    }
    class Help2 : PopupWindow
    {
        public Help2() : base()
        {
            H = 20;
        }
        protected override void DrawInterior()
        {
            WriteLineCentered("Buttons Help:");
            WriteLine("F1 : Help window.");
            WriteLine("F2 : About window.");
            WriteLine("F3 : Type file on screen.");
            WriteLine("F4 : Change Drive.");
            WriteLine("F5 : Copy file or directory.");
            WriteLine("F6 : Move file or directory.");
            WriteLine("F7 : Make new directory.");
            WriteLine("F8 : Delete file or directory.");
            WriteLine("F9 : Change program settings.");
            WriteLine("F10 : Exit from program.");

        }
    }
}
