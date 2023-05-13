using System;
using System.Runtime.InteropServices;
using Autofac;
using BusinessLogic.BusinessObjects;
using BusinessObjects;

public class ChartEventsHook
{
    private const int WM_KEYDOWN = 0x100;
    private static IntPtr _windowHandle;
    private static LowLevelKeyboardProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;

    private static IWebLog log;

    public ChartEventsHook(IntPtr windowHandle)
    {
        _windowHandle = windowHandle;
        log = MainService.thisGlobal.Container.Resolve<IWebLog>();
    }

    public void HookKeyboard()
    {
        // RuntimeModule module = GetRuntimeModule();
        // ModuleHandle
        int outId; //int pId;
        uint ThreadId = (uint)GetWindowThreadProcessId(_windowHandle, out outId);
        _hookID = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, IntPtr.Zero, ThreadId);
    }

    public void UnhookKeyboard()
    {
        UnhookWindowsHookEx(_hookID);
    }

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            log?.Info("Key Pressed: " + vkCode.ToString());
            //Console.WriteLine((Keys)vkCode);
        }

        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)] 
    private static extern IntPtr GetModuleHandle(string lpModuleName);
    [DllImport("kernel32", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    private static extern  int GetCurrentThreadId ();
    [DllImport("User32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    private static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);
    [DllImport("User32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("kernel32", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr OpenProcess(uint fdwAccess,bool fInherit,uint IDProcess);
    [DllImport("kernel32", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr VirtualAllocEx(IntPtr hProcess,int lpAddress,int dwSize,int flAllocationType,int flProtect);
    
    private const int WH_KEYBOARD_LL = 13;
}