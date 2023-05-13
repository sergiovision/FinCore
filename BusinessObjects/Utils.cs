using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using BusinessObjects.BusinessObjects;
using Newtonsoft.Json;

namespace BusinessObjects;

public static class Utils
{
    /* public static bool HasAny(ICollection parameters)
    {
        if (parameters == null)
            return false;
        if (parameters.Count <= 0)
            return false;
        return true;
    }*/
    

    public static bool HasAny<T>(IEnumerable<T> parameters)
    {
        if (parameters == null)
            return false;
        if (!parameters.Any())
            return false;
        return true;
    }

    public static bool IsSameDay(DateTime d1, DateTime d2)
    {
        return d1.DayOfYear == d2.DayOfYear && d2.Year == d1.Year;
    }

    public  static List<T> ExtractList<T>(string dataStr)
    {
        List<T> positions = null;
        if (!string.IsNullOrEmpty(dataStr))
            positions = JsonConvert.DeserializeObject<List<T>>(dataStr);
        else
            positions = new List<T>();
        return positions;
    }
    
    public static string CopyFile(string folder, Terminal terminal, string file, string targetFolder)
    {
        return $@"xcopy /y {file} {targetFolder}{Environment.NewLine}";
    }

    public static bool IsMQL5(string path)
    {
        return path.Contains("MQL5");
    }
    
    public static string ReasonToString(int Reason)
    {
        switch (Reason)
        {
            case 0: //0
                return
                    "0 <REASON_PROGRAM> - Expert Advisor terminated its operation by calling the _ExpertRemove()_ function";
            case 1: //1
                return "1 <REASON_REMOVE> Program has been deleted from the chart";
            case 2: // 2
                return "2 <REASON_RECOMPILE> Program has been recompiled";
            case 3: //3
                return "3 <REASON_CHARTCHANGE> Symbol or chart period has been changed";
            case 4:
                return "4 <REASON_CHARTCLOSE> Chart has been closed";
            case 5:
                return "5 <REASON_PARAMETERS> Input parameters have been changed by a user";
            case 6:
                return
                    "6 <REASON_ACCOUNT> Another account has been activated or reconnection to the trade server has occurred due to changes in the account settings";
            case 7:
                return "7 <REASON_TEMPLATE> A new template has been applied";
            case 8:
                return
                    "8 <REASON_INITFAILED> This value means that _OnInit()_ handler has returned a nonzero value";
            case 9:
                return "9 <REASON_CLOSE> Terminal has been closed";
        }

        return $"Unknown reason: {Reason}";
    }

    public static bool FileLocked(string FileName)
    {
        FileStream fs = null;
        try
        {
            // NOTE: This doesn't handle situations where file is opened for writing by another process but put into write shared mode, it will not throw an exception and won't show it as write locked
            fs = File.Open(FileName, FileMode.Open, FileAccess.ReadWrite,
                FileShare.None); // If we can't open file for reading and writing then it's locked by another process for writing
        }
        catch (UnauthorizedAccessException) // https://msdn.microsoft.com/en-us/library/y973b725(v=vs.110).aspx
        {
            // This is because the file is Read-Only and we tried to open in ReadWrite mode, now try to open in Read only mode
            try
            {
                fs = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (Exception)
            {
                return true; // This file has been locked, we can't even open it to read
            }
        }
        catch (Exception)
        {
            return true; // This file has been locked
        }
        finally
        {
            fs?.Close();
        }
        return false;
    }
    
    [DllImport("kernel32.dll")]
    private static extern int GetPrivateProfileString(int Section, string Key,
        string Value, [MarshalAs(UnmanagedType.LPArray)] byte[] Result,
        int Size, string FileName);

    [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileStringW", SetLastError = true,
        CharSet = CharSet.Unicode)]
    private static extern int GetPrivateProfileStringW(string lpApplicationName, string lpKeyName, string lpDefault,
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)]
        char[] lpReturnedString, int nSize, string Filename);

    [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileStringW", CharSet = CharSet.Unicode)]
    private static extern int WritePrivateProfileStringW(string lpApplicationName, int lpKeyName, int lpString,
        string lpFileName);

    [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileStringW", CharSet = CharSet.Unicode)]
    private static extern int WritePrivateProfileStringW2(string lpApplicationName, string lpKeyName,
        string lpString,
        string lpFileName);

    
    private const int CHAR_BUFF_SIZE = 512;

    // The Function called to obtain the SectionHeaders,
    // and returns them in an Dynamic Array.
    public static string[] GetSectionNames(string path)
    {
        //    Sets the maxsize buffer to 500, if the more
        //    is required then doubles the size each time.
        for (var maxsize = CHAR_BUFF_SIZE;; maxsize *= 2)
        {
            //    Obtains the information in bytes and stores
            //    them in the maxsize buffer (Bytes array)
            var bytes = new byte[maxsize];
            var size = GetPrivateProfileString(0, "", "", bytes, maxsize, path);

            // Check the information obtained is not bigger
            // than the allocated maxsize buffer - 2 bytes.
            // if it is, then skip over the next section
            // so that the maxsize buffer can be doubled.
            if (size < maxsize - 2)
            {
                // Converts the bytes value into an ASCII char. This is one long string.
                var Selected = Encoding.ASCII.GetString(bytes, 0,
                    size - (size > 0 ? 1 : 0));
                // Splits the Long string into an array based on the "\0"
                // or null (Newline) value and returns the value(s) in an array
                return Selected.Split('\0');
            }
        }
    }
    
    public static string GetPrivateProfileString(string fileName, string sectionName, string keyName)
    {
        var ret = new char[CHAR_BUFF_SIZE];

        while (true)
        {
            var length = GetPrivateProfileStringW(sectionName, keyName, null, ret, ret.Length, fileName);
            if (length == 0)
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

            // This function behaves differently if both sectionName and keyName are null
            if (sectionName != null && keyName != null)
            {
                if (length == ret.Length - 1)
                    ret = new char[ret.Length * 2];
                else
                    return new string(ret, 0, length);
            }
            else
            {
                if (length == ret.Length - 2)
                    ret = new char[ret.Length * 2];
                else
                    return new string(ret, 0, length - 1);
            }
        }
    }
    
    private static int isDebug = -1;

    public static bool IsDebug()
    {
        if (isDebug == -1)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var isDebugBuild = assembly.GetCustomAttributes(false).OfType<DebuggableAttribute>()
                .Select(attr => attr.IsJITTrackingEnabled).FirstOrDefault();
            if (isDebugBuild)
            {
                isDebug = 1;
                return true;
            }

            isDebug = 0;
            return false;
        }

        return isDebug > 0 ? true : false;
    }

    public static string Encode(string value)
    {
        var hash = SHA1.Create();
        var encoder = new ASCIIEncoding();
        var combined = encoder.GetBytes(value ?? "");
        return BitConverter.ToString(hash.ComputeHash(combined)).ToLower().Replace("-", "");
    }

}
