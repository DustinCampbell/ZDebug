using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Xml.Linq;

namespace ZDebug.UI.Utilities
{
    public static partial class Storage
    {
        private static class WindowPlacement
        {
            [StructLayout(LayoutKind.Sequential)]
            private struct RECT
            {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct POINT
            {
                public int X;
                public int Y;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct WINDOWPLACEMENT
            {
                public int length;
                public int flags;
                public int showCmd;
                public POINT minPosition;
                public POINT maxPosition;
                public RECT normalPosition;
            }

            private const int SW_SHOWNORMAL = 1;
            private const int SW_SHOWMINIMIZED = 2;

            [DllImport("user32.dll")]
            private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

            [DllImport("user32.dll")]
            private static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

            public static void Restore(Window window, XElement xml)
            {
                try
                {
                    var placement = new WINDOWPLACEMENT();

                    placement.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                    placement.flags = 0;

                    placement.showCmd = (int)xml.Element("showCmd");
                    placement.showCmd = (placement.showCmd == SW_SHOWMINIMIZED ? SW_SHOWNORMAL : placement.showCmd);

                    placement.minPosition.X = (int)xml.Element("minPosition").Attribute("X");
                    placement.minPosition.Y = (int)xml.Element("minPosition").Attribute("Y");

                    placement.maxPosition.X = (int)xml.Element("maxPosition").Attribute("X");
                    placement.maxPosition.Y = (int)xml.Element("maxPosition").Attribute("Y");

                    placement.normalPosition.Left = (int)xml.Element("normalPosition").Attribute("Left");
                    placement.normalPosition.Top = (int)xml.Element("normalPosition").Attribute("Top");
                    placement.normalPosition.Right = (int)xml.Element("normalPosition").Attribute("Right");
                    placement.normalPosition.Bottom = (int)xml.Element("normalPosition").Attribute("Bottom");

                    var interopHelper = new WindowInteropHelper(window);
                    SetWindowPlacement(interopHelper.Handle, ref placement);
                }
                catch
                {
                    // Parsing placement XML failed. Fail silently.
                }
            }

            public static XElement Save(Window window)
            {
                var interopHelper = new WindowInteropHelper(window);
                var placement = new WINDOWPLACEMENT();
                GetWindowPlacement(interopHelper.Handle, out placement);

                return
                    new XElement("placement",
                        new XElement("showCmd", placement.showCmd),
                        new XElement("minPosition",
                            new XAttribute("X", placement.minPosition.X),
                            new XAttribute("Y", placement.minPosition.Y)),
                        new XElement("maxPosition",
                            new XAttribute("X", placement.maxPosition.X),
                            new XAttribute("Y", placement.maxPosition.Y)),
                        new XElement("normalPosition",
                            new XAttribute("Left", placement.normalPosition.Left),
                            new XAttribute("Top", placement.normalPosition.Top),
                            new XAttribute("Right", placement.normalPosition.Right),
                            new XAttribute("Bottom", placement.normalPosition.Bottom)));
            }
        }
    }
}
