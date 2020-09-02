using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.BusinessObjects;

namespace BusinessObjects
{

    public interface IMessagingServer
    {
        public bool MulticastText(string text);
        public bool Start();
        public bool Stop();
    }

    public interface IMessagingService
    {
        public IMessagingServer Server { get; }
        public IMessagingServer LocalServer { get; }
        public void SendMessage(WsMessage wsMessage);
        public void SendMessage<T>(WsMessageType type, T obj);

        public void SendTcpMessage(WsMessage wsMessage);
        public void SendTcpMessage<T>(WsMessageType type, T obj);
    }
}