using System.Collections.Generic;

namespace BusinessObjects
{
    public interface ITerminalEvents
    {
        void UpdatePositions(long magicId, long AccountNumber, IEnumerable<PositionInfo> pos);

        void UpdateSLTP(long magicId, long AccountNumber, IEnumerable<PositionInfo> pos);
        // methods and functions
        List<PositionInfo> GetAllPositions();
        void DeletePosition(long Ticket);
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