using BusinessObjects.BusinessObjects;

namespace BusinessObjects
{
    public interface ISignalHandler
    {
        SignalInfo ListenSignal(long flags, long objectId);
        void PostSignal(SignalInfo signal, IMessagingServer server);
        void ProcessMessage(WsMessage wsMessage, IMessagingServer server);
    }
}