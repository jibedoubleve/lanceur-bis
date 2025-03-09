using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Lanceur.Core.Utils;

public static class ProcessHelper
{
    #region Methods

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

    [DllImport("user32.dll")] private static extern IntPtr WindowFromPoint(Win32Point point);

    /// <summary>
    ///     Retrieves the file path and description of the executable associated with the process
    ///     under the current mouse position.
    /// </summary>
    /// <returns>
    ///     A tuple containing the executable file path and its description. Returns <c>(null, null)</c>
    ///     if the information is unavailable.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     Thrown when the application lacks sufficient permissions to access the target process.
    ///     Administrative privileges may be required.
    /// </exception>
    /// <exception cref="Exception">
    ///     Thrown for any other unexpected errors encountered while retrieving process information.
    /// </exception>
    public static (string FileName, string FileDescription) GetExecutablePathAtMousePosition()
    {
        try
        {
            var c = GetMousePosition();
            var point = Win32Point.NewPoint(c.X, c.Y);
            var hWnd = WindowFromPoint(point);

            _ = GetWindowThreadProcessId(hWnd, out var processId);
            var ps = Process.GetProcessById((int)processId);
            return ps.GetInformation();
        }
        catch (Win32Exception ex)
        {
            if (ex.ErrorCode.ToString("X") == "80004005")
                throw new NotSupportedException(
                    "You don't have sufficient right to access the process. You probably must have administration rights for this application, which is not yet supported.",
                    ex
                );

            throw new NotSupportedException($"Cannot find the executable path: {ex.Message}", ex);
        }
    }

    #endregion

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