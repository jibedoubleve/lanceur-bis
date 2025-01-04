using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Lanceur.Core.Utils;

public class ProcessHelper
{
    #region Methods

    private static Exception GenericException(Exception ex) => new NotSupportedException($"Cannot find the executable path: {ex.Message}", ex);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out Win32Point lpPoint);

    private static Win32Point GetMousePosition()
    {
        GetCursorPos(out var p);
        return Win32Point.NewPoint(p.X, p.Y);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern IntPtr WindowFromPoint(Win32Point point);

    public static PsInfo GetExecutablePath()
    {
        try
        {
            var c = GetMousePosition();
            var point = Win32Point.NewPoint(c.X, c.Y);
            var hWnd = WindowFromPoint(point);

            var threadId = GetWindowThreadProcessId(hWnd, out var processId);

            var ps = Process.GetProcessById((int)processId);
            var fileName = ps.MainModule.FileName;
            var fileDescription = ps.MainModule.FileVersionInfo.FileDescription;

            return new()
            {
                HWnd = $"ThreadId: {threadId} - ProcessId: {processId} - hWnd: {hWnd}",
                FileName = fileName,
                FileDescription = fileDescription
            };
        }
        catch (Win32Exception ex)
        {
            if (ex.ErrorCode.ToString("X") == "80004005")
                throw new NotSupportedException($"You don't have sufficient right to access the process. You probably must have administration rights for this application, which is not yet supported.", ex);
            else
                throw GenericException(ex);
        }
        catch (Exception ex) { throw GenericException(ex); }
    }

    #endregion Methods

    #region Structs

    [StructLayout(LayoutKind.Sequential)]
    private struct Win32Point
    {
        #region Fields

        public int X;
        public int Y;

        #endregion

        #region Methods

        public static Win32Point NewPoint(int x, int y) => new() { X = x, Y = y };

        #endregion
    }

    #endregion Structs
}