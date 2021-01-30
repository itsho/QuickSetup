using System.Runtime.InteropServices;

namespace QuickSetup.UI.Infra
{
    public static class WinApi32
    {
        [DllImport("shell32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsUserAnAdmin();
    }
}