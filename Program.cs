using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

class Program
{
    public const int SUBSTITUTE_KEYCODE = 32; // SPAZIO https://keycode.info/
    public const int HOWMANYSTRIKES = 1; // In multipli di 6, per cui 1 sono 6 spazi
    private const int WH_KEYBOARD_LL = 13;
    private static LowLevelKeyboardProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll")]
    static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [STAThread]
    static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        _hookID = SetHook(_proc);
        Application.Run();

        UnhookWindowsHookEx(_hookID);
    }

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private static void PrintSubstitute()
    {
        const int KEYEVENTF_EXTENDEDKEY = 0x1;
        const int KEYEVENTF_KEYUP = 0x2;
        UnhookWindowsHookEx(_hookID);
        keybd_event(SUBSTITUTE_KEYCODE, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
        keybd_event(SUBSTITUTE_KEYCODE, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr)0);
        _hookID = SetHook(_proc);
    }

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        int i;
        Keys key = (Keys)Marshal.ReadInt32(lParam);
        if (key == Keys.Tab)
        {
            for (i = 0; i < HOWMANYSTRIKES; i++)
            {
                PrintSubstitute();
            }
            System.Threading.Thread.Sleep(50);
            for (i = 0; i < HOWMANYSTRIKES; i++)
            {
                PrintSubstitute();
            }
            System.Threading.Thread.Sleep(50);
            for (i = 0; i < HOWMANYSTRIKES; i++)
            {
                PrintSubstitute();
            }
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }
}
