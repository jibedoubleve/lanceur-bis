using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Lanceur.Core.Utils;

internal static class ProcessExtensions
{
    #region Fields

    private const int PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;

    #endregion

    #region Methods

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    private static string GetDescription(StringBuilder exePath)
    {
        var fileInfo = FileVersionInfo.GetVersionInfo(exePath.ToString());
        return fileInfo.FileDescription ?? "";
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(int processAccess, bool bInheritHandle, int processId);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool QueryFullProcessImageName(IntPtr hProcess, int dwFlags, StringBuilder lpExeName, ref int lpdwSize);

    public static (string FileName, string Descripiton) GetInformation(this Process process)
    {
        try
        {
            var hProcess = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, process.Id);
            if (hProcess == IntPtr.Zero) throw new NotSupportedException($"Cannot open process {process.ProcessName} (PID: {process.Id}), error: {Marshal.GetLastWin32Error()}");

            var exePath = new StringBuilder(1024);
            var size = exePath.Capacity;

            if (!QueryFullProcessImageName(hProcess, 0, exePath, ref size))
                throw new NotSupportedException($"Cannot get process path for {process.ProcessName} (PID: {process.Id}), error: {Marshal.GetLastWin32Error()}");

            CloseHandle(hProcess);
            return (FileName: exePath.ToString(), Descripiton: GetDescription(exePath));
        }
        catch (Exception ex) { throw new NotSupportedException($"Error processing process {process.ProcessName}: {ex.Message}", ex); }
    }

    #endregion
}