using System;

namespace BusinessObjects
{
    public interface IWebLog
    {
        void Log(string message);

        //void Log(string scope, string message);
        void Error(Exception e);
        void Error(string s);
        void Error(string s, Exception e);
        string GetAllText();
        void ClearLog();
        void Info(object message);
        void Debug(object message);
    }
}