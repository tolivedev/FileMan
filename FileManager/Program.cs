using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
//using System.Text.Json;
using System.Xml.Serialization;
using System.IO;

namespace FileManager
{


    class About : PopupWindow
    {
        public About() : base()
        {
            Foreground = ConsoleColor.Black;
            Background = ConsoleColor.Yellow;
            H = 7;
        }
        protected override void DrawInterior()
        {
            WriteLineCentered("About:");
            WriteLine($"FM v0.5");
            WriteLine("Oms\n");
        }
    }
    
    

    
    class Program
    {
        static int pageLines = 0;
        static int pageSize = default;
        static string command, configFile;
        static int cmdPtr;
        enum ListMode { Files, Directories, All };
        enum ProcessMode { Copy, Move, Delete };
        private static ProcessMode pMode;
        private static string pSource, pDest;
        private static CommandLine cmdLine;
        private static FileListWindow leftPanel, rightPanel;
        private static GenericErrorWindow ErrWin;
        public static void logError(string errMsg)
        {
            Window.logError(errMsg);
        }
        public static void ShowError(string errMsg)
        {
            ErrWin.msg = errMsg;
            ErrWin.ShowModal();
        }
        private static string getWord()
        {
            char finChar;
            int startPtr;
            while ((cmdPtr < command.Length) && (command[cmdPtr] <= ' ')) cmdPtr++;
            if (cmdPtr >= command.Length) return null;
            if (command[cmdPtr] == '\'') { finChar = '\''; cmdPtr++; }
            else if (command[cmdPtr] == '"') { finChar = '"'; cmdPtr++; }
            else finChar = ' ';
            if (cmdPtr == command.Length) return null;
            startPtr = cmdPtr; cmdPtr = command.IndexOf(finChar, cmdPtr);
            if (cmdPtr < 0) cmdPtr = command.Length + 1; else cmdPtr++;
            return command.Substring(startPtr, cmdPtr - startPtr - 1);
        }
        private static bool PageWriteLine(string str)
        {
            if (pageLines < 0) { pageLines++; return false; }
            Console.WriteLine(str);
            pageLines++; if (pageLines < pageSize) return false;
            Console.WriteLine();
            Console.Write("Press ESC to exit or any other key for next page...");
            if (Console.ReadKey(true).Key == ConsoleKey.Escape) return true;
            pageLines = 0; Console.Clear(); return false;
        }
        static void ListDirectories()
        {
            ListMode Lmode = ListMode.All;
            Console.Clear();
            string Folder = cmdLine.CurPath;
            //pageSize = Window.winH - 3;
            pageLines = 0;
            do
            {
                string word = getWord();
                if (word == null) break;
                if (word[0] != '-') Folder = word;
                else switch (word)
                    {
                        case "-p":
                            word = getWord();
                            if (word == null) break;
                            try { pageLines = -pageSize * Int32.Parse(word); } catch { logError("Type conversion error." + word + " is not an integer."); };
                            break;
                        case "-f": Lmode = ListMode.Files; break;
                        case "-d": Lmode = ListMode.Directories; break;
                    }
            } while (true);
            string[] files = Directory.GetFiles(Folder);
            string[] directories = Directory.GetDirectories(Folder);
            if (PageWriteLine("")) return;
            if (PageWriteLine($"Listing of {Folder} :")) return;
            if (PageWriteLine("")) return;
            int FL = Folder.Length; if (Folder[FL - 1] != '\\') FL++;
            //for (int i = 0; i < directories.Length; i++) Console.WriteLine($"{directories[i].Substring(FL),-50}<FOLDER>");
            char[] AttrsPacked = new char[10];
            if (Lmode != ListMode.Files) foreach (string directory in directories)
                {
                    DirectoryInfo DirInfo = new DirectoryInfo(directory);
                    string AttrsFull = $"{DirInfo.Attributes}";
                    int ptr = 0;
                    for (int num = 0; num < 10; num++)
                    {
                        while ((ptr < AttrsFull.Length) && (AttrsFull[ptr] == ' ')) ptr++;
                        if (ptr == AttrsFull.Length) { AttrsPacked[num] = ' '; continue; }
                        AttrsPacked[num] = AttrsFull[ptr]; ptr = AttrsFull.IndexOf(',', ptr);
                        if (ptr < 0) ptr = AttrsFull.Length; else ptr++;
                    }
                    if (PageWriteLine($"{DirInfo.FullName.Substring(FL),-30} {DirInfo.CreationTime,-20} {new string(AttrsPacked),10}        <FOLDER>")) return;
                }
            //for (int i = 0; i < files.Length; i++) Console.WriteLine(files[i].Substring(FL));
            if (Lmode != ListMode.Directories) foreach (string file in files)
                {
                    FileInfo FlInfo = new FileInfo(file);
                    string FLength = "";
                    if (FlInfo.Length < 1024) FLength = $"{FlInfo.Length} bytes";
                    if ((FlInfo.Length > 1024) && (FlInfo.Length < 1048576)) FLength = $"{FlInfo.Length / 1024} Kbytes";
                    if ((FlInfo.Length > 1048576) && (FlInfo.Length < 1073741824)) FLength = $"{FlInfo.Length / 1048576} Mbytes";
                    if (FlInfo.Length > 1073741824) FLength = $"{FlInfo.Length / 1073741824} Gbytes";
                    string AttrsFull = $"{FlInfo.Attributes}";
                    int ptr = 0;
                    for (int num = 0; num < 10; num++)
                    {
                        while ((ptr < AttrsFull.Length) && (AttrsFull[ptr] == ' ')) ptr++;
                        if (ptr == AttrsFull.Length) { AttrsPacked[num] = ' '; continue; }
                        AttrsPacked[num] = AttrsFull[ptr]; ptr = AttrsFull.IndexOf(',', ptr);
                        if (ptr < 0) ptr = AttrsFull.Length; else ptr++;
                    }
                    if (PageWriteLine($"{FlInfo.FullName.Substring(FL),-30} {FlInfo.CreationTime,-20} {new string(AttrsPacked),10} {FLength,15}")) return;
                }
            if (PageWriteLine("")) return;
            if (PageWriteLine($"{directories.Length} folders.")) return;
            if (PageWriteLine($"{files.Length} files.")) return;
            if (PageWriteLine("")) return;
            Console.Write("Press any key to return..."); Console.ReadKey(true);
        }
        private static void TypeFile(string myFile)
        {
            Console.Clear();
            string[] tpSTR;
            pageLines = 0;
            try { tpSTR = File.ReadAllLines(myFile); }
            catch { ShowError($"Incorrect file name <{myFile}>. Press any key to return..."); Console.ReadKey(true); logError("Incorrect file name: " + myFile + "."); return; }
            foreach (string STR in tpSTR) if (PageWriteLine(STR)) return;
            Console.Write("Press any key to return..."); Console.ReadKey(true);
        }
        private static void ProcessFile(string Name)
        {
            try
            {
                var fiSource = new FileInfo(pSource + Name);
                if (pMode == ProcessMode.Delete) fiSource.Delete();
                else
                {
                    if (pDest[pDest.Length - 1] != '\\') Name = pDest;
                    else Name = pDest + Name;
                    //var fiDest = new FileInfo(Name);
                    if (pMode == ProcessMode.Copy) fiSource.CopyTo(Name);
                    else fiSource.MoveTo(Name);
                }
            }
            catch
            {
                Console.Clear();
                //ShowError()
                ShowError("File operation error. Press any key to return..."); Console.ReadKey(true);
                logError("File operation error.");
                return;
            }
        }
        private static void ProcessRecursively(string Folder)
        {
            if (pMode != ProcessMode.Delete)
                try { Directory.CreateDirectory(pDest + Folder.Substring(pSource.Length)); }
                catch
                {
                    Console.Clear();
                    ShowError("Cannot create directory. Press any key to return..."); Console.ReadKey(true);
                    logError("Directory creation error.");
                    return;
                }
            string[] fd;
            fd = Directory.GetDirectories(Folder);
            foreach (string cur in fd)
                ProcessRecursively(cur);
            fd = Directory.GetFiles(Folder);
            foreach (string cur in fd)
                ProcessFile(cur.Substring(pSource.Length));
            try { if (pMode != ProcessMode.Copy) Directory.Delete(Folder); }
            catch
            {
                Console.Clear();
                ShowError("Cannot delete directory. Press any key to return..."); Console.ReadKey(true);
                logError("Deleting directory error.");
                return;
            }
        }
        private static void Process()
        {
            pSource = getWord(); if (pSource == null) return;
            if (pMode != ProcessMode.Delete)
            {
                pDest = getWord();
                if (pDest == null) pDest = cmdLine.CurPath;
            }
            FileInfo flInfo = new FileInfo(pSource);
            if ((flInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                if (pSource[pSource.Length - 1] != '\\') pSource = pSource + '\\';
                if (pMode != ProcessMode.Delete)
                {
                    if (pDest[pDest.Length - 1] != '\\') pDest = pDest + '\\';
                    pDest = pDest + flInfo.Name;
                    Directory.CreateDirectory(pDest);
                    pDest = pDest + "\\";
                }
                ProcessRecursively(pSource);
            }
            else
            {
                pSource = flInfo.DirectoryName;
                if (pSource[pSource.Length - 1] != '\\') pSource = pSource + '\\';
                if (pMode != ProcessMode.Delete)
                {
                    FileAttributes attr = File.GetAttributes(pDest);
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        if (pDest[pDest.Length - 1] != '\\') pDest = pDest + '\\';
                }
                ProcessFile(flInfo.Name);
            }
        }
        private static string GetFocusFolder()
        {
            if (Window.FocusWindow == leftPanel) return leftPanel.currFolder;
            if (Window.FocusWindow == rightPanel) return rightPanel.currFolder;
            if (Window.FocusWindow == cmdLine) return cmdLine.CurPath;
            return null;
        }
        private static void SetFocusFolder(string DirName)
        {
            try { Directory.SetCurrentDirectory(DirName); }
            catch { Console.Clear(); ShowError($"Incorrect directory name <{DirName}>. Press any key to return..."); Console.ReadKey(true); logError("Incorrect directory name: " + DirName + "."); };
            if (Window.FocusWindow == leftPanel)
            {
                leftPanel.currFolder = Directory.GetCurrentDirectory();
                leftPanel.Cursor = 0; leftPanel.Start = 0;
            }
            if (Window.FocusWindow == rightPanel)
            {
                rightPanel.currFolder = Directory.GetCurrentDirectory();
                rightPanel.Cursor = 0; rightPanel.Start = 0;
            }
            if (Window.FocusWindow == cmdLine) cmdLine.CurPath = Directory.GetCurrentDirectory();
        }
        private static void MakeDirectory(string DirName)
        {
            if (DirName == null)
            {
                Console.Clear();
                Console.Write("Press enter new directory name :> ");
                DirName = Console.ReadLine();
            }
            try { Directory.CreateDirectory(DirName); }
            catch
            {
                ShowError($"Cannot create {DirName} directory. Press any key to return..."); Console.ReadKey(true);
                logError("Directory creation error: " + DirName + ".");
            }
        }
        private static Config LoadConfig()
        {
            /*
            string json = File.ReadAllText("config.ini");
            return JsonSerializer.Deserialize<Config>(json);
            */
            string xml = File.ReadAllText(configFile);
            StringReader stringReader = new StringReader(xml);
            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            Config tmp = (Config)serializer.Deserialize(stringReader);
            return tmp;
        }
        private static void SaveConfig(Config config)
        {
            /*
            string json = JsonSerializer.Serialize<Config>(config);
            File.WriteAllText("config.ini", json);
            */
            config.cmdLineFolder = cmdLine.CurPath;
            StringWriter stringWriter = new StringWriter();
            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            serializer.Serialize(stringWriter, config);
            string xml = stringWriter.ToString();
            if (File.Exists(configFile)) File.Delete(configFile);
            File.WriteAllText(configFile, xml);
        }
        static void Main(string[] args)
        {
            configFile = Directory.GetCurrentDirectory() + "\\config.ini";
            Window.errorFile = Directory.GetCurrentDirectory() + "\\error.log";
            Config config;
            try { config = LoadConfig(); }
            catch
            {
                config = new Config();
                config.Default();
                logError("Config reading error.");
            }
            pageSize = config.PageSize;
            var descWnd = new DescriptionWindow();
            leftPanel = new FileListWindow(false, config.StartFolder1);
            rightPanel = new FileListWindow(true, config.StartFolder2);
            leftPanel.OtherPanel = rightPanel;
            rightPanel.OtherPanel = leftPanel;
            cmdLine = new CommandLine();
            
            descWnd.left = leftPanel; 
            descWnd.right = rightPanel;
            
            //cmdLine.CurPath = Directory.GetCurrentDirectory();
            cmdLine.CurPath = config.cmdLineFolder;
            var drvWnd = new ChangeDrive();
            var HelpWnd1 = new Help1();
            var HelpWnd2 = new Help2();
            var AboutWnd = new About();
            var errWnd = new ErrorWindow();
            ErrWin = new GenericErrorWindow();
            //Window.FocusNextWindow();
            Window.FocusWindow = cmdLine;
            bool NeedRedraw = false;
            ConsoleKeyInfo cmdKey;
            do
            {
                Window.RedrawAllWindows(NeedRedraw); NeedRedraw = false;
                descWnd.DrawFileInfo();
                cmdKey = Console.ReadKey(true);
                switch (cmdKey.Key)
                {
                    case ConsoleKey.Tab:
                        Window.FocusNextWindow();
                        NeedRedraw = true;
                        Directory.SetCurrentDirectory(GetFocusFolder());
                        continue;
                    case ConsoleKey.F1: HelpWnd1.ShowModal(); HelpWnd2.ShowModal(); continue;
                    case ConsoleKey.F2: AboutWnd.ShowModal(); continue;
                    case ConsoleKey.F9: config.Input(); NeedRedraw = true; pageSize = config.PageSize; continue;
                    case ConsoleKey.F10: SaveConfig(config); return;
                }
                command = Window.FocusWindow.Input(cmdKey);
                if (command == null) continue;
                cmdPtr = 0;
                //string DirName;
                switch (getWord())
                {
                    case "ls": ListDirectories(); break;  // ------------------------------------- List with parameters
                    case "help":  // -------------------------------------------------------------- Help
                    case "/?": HelpWnd1.ShowModal(); HelpWnd2.ShowModal(); break;  // ------------ Help
                    case "chdrv": SetFocusFolder(drvWnd.Choose()); break;  // --------------------- Change Drive
                    case "cd": SetFocusFolder(getWord()); break;  // ------------------------------ Change Directory
                    case "md": MakeDirectory(getWord()); break;  // ------------------------------- Make Directory
                    case "rm": pMode = ProcessMode.Delete; Process(); break;  // ------------------ Delete Directory
                    case "mv": pMode = ProcessMode.Move; Process(); break;  // -------------------- Move Directory
                    case "cp": pMode = ProcessMode.Copy; Process(); break;  // -------------------- Copy file/Directory
                    case "tp": TypeFile(getWord()); break; // ------------------------------------- Type file
                    case "exit": SaveConfig(config); return;
                    default: errWnd.ShowModal(); break;
                }
                leftPanel.ListDirectories();
                rightPanel.ListDirectories();
                Window.FocusWindow.PostCommand(); NeedRedraw = true;
            } while (true);
        }
    }
}
