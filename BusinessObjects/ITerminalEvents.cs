using System.Collections.Generic;
using BusinessObjects.BusinessObjects;

namespace BusinessObjects
{
    public interface ITerminalEvents
    {
        void AddOrders(long magicId, long AccountNumber, IEnumerable<PositionInfo> pos);
        void UpdateOrders(long magicId, long AccountNumber, IEnumerable<PositionInfo> pos);
        void DeleteOrders(long magicId, long AccountNumber, IEnumerable<PositionInfo> pos);
        
        void UpdatePositions(long magicId, long AccountNumber, IEnumerable<PositionInfo> pos);

        void UpdateSLTP(long magicId, long AccountNumber, IEnumerable<PositionInfo> pos);
        

        // methods and functions
        List<PositionInfo> GetAllPositions();
        List<PositionInfo> GetPositions4Adviser(long adviserId);
        PositionInfo getPosition(long ticket);
        void DeletePosition(long ticket);
        List<DealInfo> GetTodayDeals();

        TodayStat GetTodayStat();
        bool CheckTradeAllowed(SignalInfo signal);

        void UpdateBalance(int TerminalId, decimal Balance, decimal Equity);

        #region Interface Imp

        public void InsertPosition(PositionInfo pos);
        public void UpdatePosition(PositionInfo pos);
        public void UpdatePositionFromClient(PositionInfo pos);
        public void RemovePosition(long Ticket);

        #endregion
    }
}