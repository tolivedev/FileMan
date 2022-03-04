using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager
{
    /// <summary>
    /// Command line and buttons
    /// </summary>
    class CommandLine : Window
    {
        public string CurPath;
        public string Command;
        public string[] cmdHistory;
        int HistorySize, CurHistory;
        public CommandLine() : base()
        {
            Command = "";
            cmdHistory = new string[100]; // history for 100 last commands 
            HistorySize = 0; 
            canFocus = true;
            CurHistory = 0;
        }
        protected override void Place()
        {
            X = 0; Y = winH - 2; W = winW; H = 2;
        }
        private void ShowButton(int Num, string cmd, bool Reserved)
        {
            SetCursorPosition(10 * (Num - 1), 1);
            if (!Reserved) SetColorPair(ConsoleColor.Black, ConsoleColor.Cyan);
            else SetColorPair(ConsoleColor.Cyan, ConsoleColor.Cyan);
            Write($"F{Num}: {cmd,-5}");
        }
        protected override void Draw()
        {
            SetColorPair(ConsoleColor.White, ConsoleColor.Black);
            SetCursorPosition(0, 1);
            WriteSpacesTo(1, false);
            ShowButton(1, "Help", false);
            ShowButton(2, "About", false);
            ShowButton(3, "Type", false);
            ShowButton(4, "ChDrv", false);
            ShowButton(5, "Copy", false);
            ShowButton(6, "Move", false);
            ShowButton(7, "MkDr", false);
            ShowButton(8, "Del ", false);
            ShowButton(9, "Setup", false);
            ShowButton(10, "Exit", false);
            if (isFocused()) SetColorPair(ConsoleColor.White, ConsoleColor.Black);
            else SetColorPair(ConsoleColor.DarkGray, ConsoleColor.Black);
            SetCursorPosition(0, 0);
            WriteLimited(CurPath, 15, true);
            Write(" :> ");
            WriteLimited(Command, 0, true);
            WriteSpacesTo(0, true);
        }
        public override void PostCommand() { Command = ""; }
        public override string Input(ConsoleKeyInfo cmdKey)
        {
            if (cmdKey.KeyChar >= ' ') Command += cmdKey.KeyChar;
            switch (cmdKey.Key)
            {
                case ConsoleKey.UpArrow:
                    if (CurHistory > 0) CurHistory--;
                    if (CurHistory < HistorySize) Command = cmdHistory[CurHistory];
                    break;
                case ConsoleKey.DownArrow:
                    if (CurHistory < HistorySize - 1) CurHistory++;
                    if (CurHistory < HistorySize) Command = cmdHistory[CurHistory];
                    break;
                case ConsoleKey.F5: Command = "cp "; break;
                case ConsoleKey.Backspace: if (Command.Length > 0) Command = Command.Substring(0, Command.Length - 1); break;
                case ConsoleKey.Enter:
                    if (HistorySize < 100) cmdHistory[HistorySize++] = Command;
                    else
                    {
                        for (int i = 0; i < 99; i++) cmdHistory[i] = cmdHistory[i + 1];
                        cmdHistory[99] = Command;
                    }
                    CurHistory = HistorySize - 1;
                    return Command;
            }
            Redraw(); return null;
        }
    }
}
