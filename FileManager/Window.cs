using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager
{
    abstract class Window
    {
        private Window Next;                       // List of all windows
        private static Window First = null;
        private static Window Last = null;
        public int X, Y, W, H;                     // X,Y - position; W - width; H - height;
        protected bool visible;                    // If this window is visible
        protected bool canFocus;                   // Iff this window can have keyboard focus
        public static Window FocusWindow = null;   // Window that has keyboard focus
        public static int winW = -1, winH = -1;    // Window width and height
        protected int CX, CY, OfX;                 // Current coordinates for printing in window and left offset
        private static int CurX = -1, CurY = -1;   // Current coordinates and colors set in system
        private static ConsoleColor CurFore = ConsoleColor.White,
                                    CurBack = ConsoleColor.Black;
        public static string errorFile;
        public static void logError(string errMsg)
        {
            File.AppendAllText(errorFile, $"{DateTime.Now} Error code: {errMsg}\n");
        }
        public Window()
        {
            if (Last == null) 
                First = this;
            else Last.Next = this;
            Next = null; 
            Last = this;
            X = 5; Y = 5; W = 5; H = 5; CX = 0; CY = 0; OfX = 0;
            visible = true; 
            canFocus = false;
        }
        protected abstract void Draw();   // Abstract function - draws the window
        protected abstract void Place();  // Abstract function - positions the window on screen
        public void Redraw()
        {
            if (visible) Draw();
        }
        public static void RedrawAllWindows(bool force)
        {
            Window Cur;
             if ((winW != Console.WindowWidth) || (winH != Console.WindowHeight))
            {
                winW = Console.WindowWidth;
                winH = Console.WindowHeight;
                Console.SetBufferSize(winW, winH);  // -------------------------------------------- !!!!!!!!!!!!!
                for (Cur = First; Cur != null; Cur = Cur.Next) 
                    Cur.Place();
            }
            else if (!force) return;
            for (Cur = First; Cur != null; Cur = Cur.Next) Cur.Redraw();
        }
        public void Show()
        {
            visible = true; RedrawAllWindows(true);
        }
        public void Hide()
        {
            visible = false; RedrawAllWindows(true);
        }
        public virtual void PostCommand() { }
        public virtual string Input(ConsoleKeyInfo cmdKey) { return null; }
        protected bool isFocused()
        {
            return (FocusWindow == this);
        }
        public static void FocusNextWindow()
        {
            if (FocusWindow != null) FocusWindow = FocusWindow.Next;
            do
            {
                if (FocusWindow == null) FocusWindow = First;
                if (FocusWindow.canFocus) break;
                FocusWindow = FocusWindow.Next;
            } while (true);
        }
        protected void SetColorPair(ConsoleColor Fore, ConsoleColor Back)
        {
            if (CurFore != Fore) { Console.ForegroundColor = Fore; CurFore = Fore; }
            if (CurBack != Back) { Console.BackgroundColor = Back; CurBack = Back; }
        }
        protected void SetCursorPosition(int _X, int _Y)
        {
            CX = X + _X + OfX; CY = Y + _Y;
        }
        protected void Write(string str)
        {
            if ((CurX != CX) || (CurY != CY)) 
                try { Console.SetCursorPosition(CX, CY); } catch { logError("Console window too small."); }
            Console.Write(str); CX += str.Length; CurX = CX; CurY = CY;
        }
        protected void WriteLine(string str)
        {
            Write(str); CX = X + OfX; CY++;
        }
        protected void WriteLineCentered(string str)
        {
            CX = X + (W - str.Length) / 2; WriteLine(str);
        }
        protected void WriteLimited(string str, int RightOffset, bool end)
        {
            int MaxW = X + W - RightOffset - CX;
            if (str.Length <= MaxW) Write(str);
            else if (end) Write(str.Substring(str.Length - MaxW));
            else Write(str.Substring(0, MaxW));
        }
        protected void WriteSpacesTo(int RightOffset, bool RestoreCursor)
        {
            int MaxW = X + W - RightOffset - CX;
            for (int i = 0; i < MaxW; i++) Write(" ");
            if (RestoreCursor && (MaxW > 0)) { CurX -= MaxW; Console.SetCursorPosition(CurX, CurY); }
        }
    }

    abstract class FramedWindow : Window
    {  // ---------------------------------------------------- Window that has a frame
        public ConsoleColor Foreground, Background;
        public FramedWindow() : base()
        {
            Foreground = ConsoleColor.White;
            Background = ConsoleColor.Black;
        }
        private void DrawEdgedLine(string Left, string Interior, string Right, int Size)
        {
            Write(Left);
            for (int i = 0; i < Size; i++) Write(Interior);
            WriteLine(Right);
        }
        protected abstract void DrawInterior();
        protected override void Draw()
        {
            SetColorPair(Foreground, Background); OfX = 0;
            SetCursorPosition(0, 0);
            DrawEdgedLine("\u2554", "\u2550", "\u2557", W - 2);
            for (int i = 0; i < H - 2; i++) DrawEdgedLine("\u2551", " ", "\u2551", W - 2);
            DrawEdgedLine("\u255A", "\u2550", "\u255D", W - 2);
            OfX = 1; SetCursorPosition(0, 1); DrawInterior();
        }
    }
    abstract class PopupWindow : FramedWindow
    {  // ----------------------------------------------- Window with a message that shows on top
        public PopupWindow() : base()
        {
            Background = ConsoleColor.Gray;
            Foreground = ConsoleColor.Black;
            W = 60; H = 4; visible = false;
        }
        protected override void Place()
        {
            X = (winW - W) / 2; Y = (winH - H - 2) / 2;
        }
        public void ShowModal()
        {
            Show(); Console.ReadKey(true); Hide();
        }
    }
    abstract class ListWindow : FramedWindow
    {  // ------------------------------------------------ Window with a scrollable list
        public int Cursor, Start, Num;
        public ListWindow() : base()
        {
            Cursor = 0; Start = 0; Num = 0; canFocus = true;
        }
        protected abstract void DrawText(int n);
        private void DrawLine(int n, bool Selected)
        {
            SetCursorPosition(0, n + 1);
            if (Selected) SetColorPair(ConsoleColor.Black, ConsoleColor.Yellow);
            else SetColorPair(Foreground, Background);
            DrawText(Start + n);
            WriteSpacesTo(1, false);
            SetColorPair(Foreground, Background);
        }
        protected override void DrawInterior()
        {
            for (int i = 0; i < H - 2; i++)
            {
                if ((Start + i) >= Num) break;
                DrawLine(i, ((Start + i) == Cursor) && isFocused());
            }
        }
        public override string Input(ConsoleKeyInfo cmdKey)
        {
            switch (cmdKey.Key)
            {
                case ConsoleKey.UpArrow:
                    if (Cursor == 0) break;
                    DrawLine(Cursor - Start, false); Cursor--;
                    if (Cursor < Start) { Start = Cursor; DrawInterior(); }
                    else DrawLine(Cursor - Start, true);
                    break;
                case ConsoleKey.DownArrow:
                    if (Cursor == (Num - 1)) break;
                    DrawLine(Cursor - Start, false); Cursor++;
                    if (Cursor - H + 3 > Start) { Start = Cursor - H + 3; DrawInterior(); }
                    else DrawLine(Cursor - Start, true);
                    break;
            }
            return null;
        }
    }
    class FileListWindow : ListWindow
    {  // ------------------------------------------------------- One of file manager panels
        public string[] files;
        public string[] directories;
        bool right;
        int FL;
        public string currFolder;
        public FileListWindow OtherPanel;
        public bool RootFolder;
        public FileListWindow(bool _right, string Folder) : base()
        {
            currFolder = Folder;
            if (currFolder == "") currFolder = Directory.GetCurrentDirectory();
            try { ListDirectories(); } catch { currFolder = Directory.GetCurrentDirectory(); ListDirectories(); logError("ListDirectory global error."); }
            right = _right;
        }
        protected override void Place()
        {
            if (right) { X = winW / 2; W = winW - X; }
            else { X = 0; W = winW / 2; }
            Y = 0; H = winH - 10;
        }
        public void ListDirectories()
        {
            RootFolder = (currFolder.Substring(1) == @":\");
            try { files = Directory.GetFiles(currFolder); } catch { files = new string[0]; logError("Scaninng files in " + currFolder + "."); }
            try { directories = Directory.GetDirectories(currFolder); } catch { directories = new string[0]; logError("Scaninng directories in " + currFolder + "."); }
            Num = files.Length + directories.Length + (RootFolder ? 0 : 1);
            FL = currFolder.Length; 
            if (currFolder[FL - 1] != '\\') 
                FL++;
            if (Cursor >= Num) Cursor = Num - 1;
        }
        public string GetCurrentFile()
        {
            if ((Cursor == 0) && !RootFolder) return "..";
            int n = Cursor - (RootFolder ? 0 : 1);
            if (n < directories.Length) return directories[n];
            return files[n - directories.Length];
        }
        protected override void DrawText(int n)
        {
            if (!RootFolder)
            {
                if (n == 0)
                {
                    Write("..");
                    WriteSpacesTo(10, false); Write(" <FOLDER>");
                    return;
                }
                n--;
            }
            if (n < directories.Length)
            {
                WriteLimited(directories[n].Substring(FL), 10, false);
                WriteSpacesTo(10, false); Write(" <FOLDER>");
            }
            else WriteLimited(files[n - directories.Length].Substring(FL), 1, false);
        }
        protected override void DrawInterior()
        {
            base.DrawInterior();
            SetCursorPosition(2, 0); Write(" ");
            SetColorPair(ConsoleColor.Green, Background);
            WriteLimited($"{currFolder} ", 3, true);
            SetCursorPosition(2, H - 1); Write(" ");
            SetColorPair(ConsoleColor.Green, Background);
            WriteLimited($"{directories.Length} folders, {files.Length} files. ", 3, true);
        }
        public override string Input(ConsoleKeyInfo cmdKey)
        {
            base.Input(cmdKey);
            switch (cmdKey.Key)
            {
                case ConsoleKey.Enter: return "cd '" + GetCurrentFile() + "'";
                case ConsoleKey.F3: if (RootFolder || (Cursor > 0)) return "tp '" + GetCurrentFile() + "'"; break;
                case ConsoleKey.F4: return "chdrv";
                case ConsoleKey.F5: if (RootFolder || (Cursor > 0)) return "cp '" + GetCurrentFile() + "' '" + OtherPanel.currFolder + "'"; break;
                case ConsoleKey.F6: if (RootFolder || (Cursor > 0)) return "mv '" + GetCurrentFile() + "' '" + OtherPanel.currFolder + "'"; break;
                case ConsoleKey.F7: return "md";
                case ConsoleKey.F8: if (RootFolder || (Cursor > 0)) return "rm '" + GetCurrentFile() + "'"; break;
            }
            return null;
        }
    }
    class DescriptionWindow : FramedWindow
    {  // -------------------------------------------------- Description window
        public FileListWindow left, right;
        public DescriptionWindow() : base() { }
        protected override void Place()
        {
            X = 0; W = winW; Y = winH - 10; H = 8;
        }
        public void DrawFileInfo()
        {
            string fname;
            if (FocusWindow == left) fname = left.GetCurrentFile();
            else if (FocusWindow == right) fname = right.GetCurrentFile();
            else return;
            SetColorPair(Foreground, Background);
            SetCursorPosition(0, 2);
            WriteLimited(fname, 6, true); WriteSpacesTo(6, false); WriteLine("");

            FileInfo fileDscrpt = new FileInfo(fname);
            try
            {
                WriteLimited($"Creation time: {fileDscrpt.CreationTime}", 6, true); WriteSpacesTo(6, false); WriteLine("");
                WriteLimited($"Attributes: {fileDscrpt.Attributes} bytes", 6, true); WriteSpacesTo(6, false); WriteLine("");
                if (fileDscrpt.Length < 1024) { WriteLimited($"Size of file: {fileDscrpt.Length} bytes", 6, true); WriteSpacesTo(6, false); WriteLine(""); ; }
                if ((fileDscrpt.Length > 1024) && (fileDscrpt.Length < 1048576)) { WriteLimited($"Size of file: {fileDscrpt.Length / 1024} Kbytes", 6, true); WriteSpacesTo(6, false); WriteLine(""); }
                if ((fileDscrpt.Length > 1048576) && (fileDscrpt.Length < 1073741824)) { WriteLimited($"Size of file: {fileDscrpt.Length / 1048576} Mbytes", 6, true); WriteSpacesTo(6, false); WriteLine(""); }
                if (fileDscrpt.Length > 1073741824) { WriteLimited($"Size of file: {fileDscrpt.Length / 1073741824} Gbytes", 6, true); WriteSpacesTo(6, false); WriteLine(""); }
            }
            catch { logError("Generic file error: fileAttr/creation time/length corrupted."); };
            SetColorPair(ConsoleColor.White, ConsoleColor.Black);
        }
        protected override void DrawInterior()
        {
            OfX = 5; DrawFileInfo();
        }
    }
}
