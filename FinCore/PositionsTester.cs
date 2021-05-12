using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autofac;
using BusinessObjects;
using BusinessObjects.BusinessObjects;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace FinCore
{
    public class PositionsTester : ITerminalEvents
    {
        private static readonly Random random = new Random();
        private readonly List<PositionInfo> _positions = new List<PositionInfo>();

        private readonly Timer _timer;

        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(2000);
        private readonly Random _updateOrNotRandom = new Random();

        private readonly object _updateStockPricesLock = new object();
        private readonly IMessagingServer server;

        public PositionsTester()
        {
            server = Program.Container.Resolve<IMessagingServer>();
            _positions = GeneratePositions();
            _timer = new Timer(UpdatePosition, null, _updateInterval, _updateInterval);
        }

        private IHubClients Clients { get; }

        public List<PositionInfo> GetAllPositions()
        {
            return _positions;
        }

        public List<PositionInfo> GetPositions4Adviser(long adviserId)
        {
            throw new NotImplementedException();
        }

        private static List<PositionInfo> GeneratePositions()
        {
            return new List<PositionInfo>
            {
                new PositionInfo {Symbol = "EURUSD", Ticket = 1123, Lots = 0.01, Profit = 3},
                new PositionInfo {Symbol = "AUDUSD", Ticket = 2429, Lots = 0.02, Profit = 3.1},
                new PositionInfo {Symbol = "BRENT", Ticket = 34402, Lots = 0.04, Profit = 1.1},
                new PositionInfo {Symbol = "BRN", Ticket = 23432, Lots = 1.0, Profit = -1}
            };
        }

        private void UpdatePosition(object state)
        {
            lock (_updateStockPricesLock)
            {
                IEnumerator<PositionInfo> enumerator = _positions.GetEnumerator();
                var hasNext = enumerator.MoveNext();
                while (hasNext)
                {
                    var position = enumerator.Current;
                    if (!ChangePositions(position))
                        break;
                    hasNext = enumerator.MoveNext();
                }

                enumerator.Dispose();
            }
        }

        private bool ChangePositions(PositionInfo position)
        {
            var r = _updateOrNotRandom.NextDouble();
            if (r < 0.5)
            {
                position.Update();
                UpdatePosition(position);
            }
            else if (r >= 0.5 && r <= 0.65)
            {
                position = new PositionInfo
                    {Symbol = "AUDUSD", Ticket = (long) (r * 1000), Lots = 0.02, Profit = 0.0};
                InsertPosition(position);
                return false;
            }
            else if (r >= 0.9 && r < 1.0)
            {
                RemovePosition(position.Ticket);
                return false;
            }

            return true;
        }

        #region Interface Imp

        public void InsertPosition(PositionInfo pos)
        {
            _positions.Add(pos);

            var message = new WsMessage();
            message.From = "Server";
            message.Type = WsMessageType.InsertPosition;
            message.Message = JsonConvert.SerializeObject(pos);
            var send = JsonConvert.SerializeObject(message);
            server.MulticastText(send);
        }

        public void UpdatePosition(PositionInfo pos)
        {
            var message = new WsMessage();
            message.From = "Server";
            message.Type = WsMessageType.UpdatePosition;
            message.Message = JsonConvert.SerializeObject(pos);
            var send = JsonConvert.SerializeObject(message);
            server.MulticastText(send);
        }

        public void UpdatePositionFromClient(PositionInfo pos)
        {
        }

        public void RemovePosition(long Ticket)
        {
            var pos = _positions.Where(x => x.Ticket == Ticket).FirstOrDefault();
            if (pos != null)
            {
                _positions.Remove(pos);
                var message = new WsMessage();
                message.From = "Server";
                message.Type = WsMessageType.RemovePosition;
                message.Message = Ticket.ToString();
                var send = JsonConvert.SerializeObject(message);
                server.MulticastText(send);
            }
        }

        public void AddOrders(long magicId, long AccountNumber, IEnumerable<PositionInfo> pos)
        {
            throw new NotImplementedException();
        }

        public void UpdateOrders(long magicId, long AccountNumber, IEnumerable<PositionInfo> pos)
        {
            throw new NotImplementedException();
        }

        public void DeleteOrders(long magicId, long AccountNumber, IEnumerable<PositionInfo> pos)
        {
            throw new NotImplementedException();
        }

        public void UpdatePositions(long magicId, long AccountNumber, IEnumerable<PositionInfo> pos)
        {
            // this is a real update positions method. Empty for tester
        }

        public List<DealInfo> GetTodayDeals()
        {
            //throw new NotImplementedException();
            return new List<DealInfo>();
        }

        public void UpdateSLTP(long magicId, long AccountNumber, IEnumerable<PositionInfo> pos)
        {
            //throw new NotImplementedException();
        }

        public void DeletePosition(long Ticket)
        {
            throw new NotImplementedException();
        }

        public TodayStat GetTodayStat()
        {
            throw new NotImplementedException();
        }

        public void UpdateBalance(int TerminalId, decimal Balance, decimal Equity)
        {
            throw new NotImplementedException();
        }

        public bool CheckTradeAllowed(SignalInfo signal)
        {
            return true;
        }

        #endregion
    }
}