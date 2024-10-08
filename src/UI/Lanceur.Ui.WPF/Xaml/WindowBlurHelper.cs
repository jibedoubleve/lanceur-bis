using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Lanceur.Ui.WPF.Xaml;

public class WindowBlurHelper
{
    // Enumeration for composition attributes
    public enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_ENABLE_ACRYLICBLURBEHIND = 4, // Windows 10 build 1709 and later
        ACCENT_ENABLE_HOSTBACKDROP = 5,      // Windows 11
    }

    // ACCENT_POLICY structure for configuring window visual effects
    [StructLayout(LayoutKind.Sequential)]
    public struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    // Structure used to define window attributes
    [StructLayout(LayoutKind.Sequential)]
    private struct WindowCompositionAttributeData
    {
        public int Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }
    
    [DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#138")]
    private static extern bool ShouldSystemUseDarkMode();
    
    [DllImport("user32.dll")] private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

    // Apply the blur or other effect to a window
    public static void EnableBlur(Window window)
    {
        // TODO: Refactor this to make the blur level a user-modifiable setting.
        const int blurLevel = 70;
        const int black = 0x000000;
        const int white = 0x111111;
        var windowHelper = new WindowInteropHelper(window);

        var color = ShouldSystemUseDarkMode() ? black: white;
        var accent = new AccentPolicy
        {
            AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND,
            GradientColor = (blurLevel << 24) | color // opacity, then color
        };

        var accentStructSize = Marshal.SizeOf(accent);
        var accentPtr = Marshal.AllocHGlobal(accentStructSize);
        Marshal.StructureToPtr(accent, accentPtr, false);

        var data = new WindowCompositionAttributeData
        {
            Attribute = 19, // WCA_ACCENT_POLICY
            Data = accentPtr,
            SizeOfData = accentStructSize
        };

        SetWindowCompositionAttribute(windowHelper.Handle, ref data);

        Marshal.FreeHGlobal(accentPtr);
    }
}
