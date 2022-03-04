using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager
{
    /// <summary>
    /// Drive selection window
    /// </summary>
    class ChangeDrive : PopupWindow
    {
        DriveInfo[] drvs;
        int Cursor = 0;
        public ChangeDrive() : base()
        {
            drvs = DriveInfo.GetDrives();
            H = drvs.Length + 2; Cursor = 0;
        }
        private string GetSize(Int64 x) { return $"{x / (102410241024)} Gb"; }
        private void DrawLine(int n, bool Selected)
        {
            SetCursorPosition(0, n + 1);
            if (Selected) SetColorPair(ConsoleColor.Black, ConsoleColor.Yellow);
            else SetColorPair(Foreground, Background);
            Write(drvs[n].Name); WriteSpacesTo(54, false);
            Write($"{drvs[n].DriveType}"); WriteSpacesTo(45, false);
            if ((drvs[n].DriveType != DriveType.Network) && drvs[n].IsReady)
            {
                Write(drvs[n].VolumeLabel); WriteSpacesTo(35, false);
                Write(drvs[n].DriveFormat); WriteSpacesTo(28, false);
                Write(GetSize(drvs[n].TotalFreeSpace)); Write(" / ");
                Write(GetSize(drvs[n].TotalSize));
            }
            WriteSpacesTo(1, false);
        }
        protected override void DrawInterior()
        {
            for (int i = 0; i < drvs.Length; i++) DrawLine(i, i == Cursor);
        }
        public string Choose()
        {
            Show();
            do
            {
                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.UpArrow:
                        if (Cursor > 0) { DrawLine(Cursor, false); Cursor--; DrawLine(Cursor, true); }
                        break;
                    case ConsoleKey.DownArrow:
                        if (Cursor < drvs.Length - 1) { DrawLine(Cursor, false); Cursor++; DrawLine(Cursor, true); }
                        break;
                    case ConsoleKey.Enter:
                        Hide(); return drvs[Cursor].Name;
                }
            } while (true);
        }
    }
}
