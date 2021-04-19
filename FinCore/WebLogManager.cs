using BusinessObjects;
using log4net;
using System;
using System.Text;

namespace FinCore
{
    public class WebLogManager : IWebLog
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(WebLogManager));

        private static readonly object lockObject = new object();
        protected StringBuilder text;
        IMessagingService service;

        public WebLogManager(IMessagingService serv)
        {
            service = serv;
            text = new StringBuilder($"***Logging started at {DateTime.Now.ToString()}***{Environment.NewLine}");
        }

        #region Interface Imp

        public string GetAllText()
        {
            return text.ToString();
        }

        public void ClearLog()
        {
            lock (lockObject)
            {
                text.Clear();
                string initMessage = $"***Logging started at {DateTime.Now.ToString()}***{Environment.NewLine}";
                text.Append(initMessage);
                service.SendMessage(WsMessageType.WriteLog, initMessage);
            }
        }

        public void Log(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;
            string msg = DateTime.Now + " " + message + Environment.NewLine;
            text.Append(msg);
            service.SendMessage(WsMessageType.WriteLog, msg);
        }


        public void Error(Exception e)
        {
            if (e == null)
                return;
            service.SendMessage(WsMessageType.WriteLog, e);
        }

        public void Info(object message)
        {
            if (message == null)
                return;
            log.Info(message);
        }

        public void Debug(object message)
        {
            if (message == null)
                return;
            log.Debug(message);
        }

        public void Error(string s)
        {
            if (string.IsNullOrEmpty(s))
                return;
            log.Error(s);
        }

        public void Error(string s, Exception e)
        {
            if (string.IsNullOrEmpty(s))
                return;
            log.Error(s, e);
        }

        #endregion
    }
}
