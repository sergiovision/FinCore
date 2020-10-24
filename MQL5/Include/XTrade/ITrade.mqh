//+------------------------------------------------------------------+
//|                                                       IUtils.mqh |
//|                        Copyright 2018, MetaQuotes Software Corp. |
//|                                             https://www.mql5.com |
//+------------------------------------------------------------------+
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <XTrade\InitMQL.mqh>

class OrderSelection;
class Order;
class PendingOrder;

class ITrade
{
public:
   virtual double ContractsToLots(int op_type, ushort nContracts) = 0;
   virtual int    GetDailyATR() const = 0;
   virtual int    ATROnIndicator(double rank) = 0;
   virtual int    DefaultStopLoss() = 0;
   virtual int    DefaultTakeProfit() = 0;
   virtual double StopLoss(double price, int op_type) = 0;
   virtual double TakeProfit(double price, int op_type) = 0;
   virtual OrderSelection* Orders() = 0;
   virtual void   SaveOrders() = 0;
   virtual bool   CloseOrder(Order* order) = 0;
   
   virtual bool CloseOrderPartially(Order& order, double newLotSize) = 0;
   virtual bool ChangeOrder(Order& order, double stoploss, double takeprofit) = 0;
   virtual Order* OpenExpertOrder(int Value, string Name) = 0;

   virtual bool AllowVStops() = 0;
   virtual long GeneratePendingOrderTicket(int type) = 0;
   virtual long ChartId() const = 0;
   virtual int SubWindow() const = 0;
   virtual int IndiSubWindow() const = 0;
   virtual void AddUpdateByTicket(long Ticket) = 0;
   virtual void UpdateStopLossesTakeProfits(bool forceUpdate) = 0; 
   virtual void SetTrailDelta(double trailDelta) = 0;
   virtual double TrailDelta() = 0;
   virtual void SetTrend(TYPE_TREND t) = 0;
   virtual TYPE_TREND Trend() = 0;   
   virtual Order* InitManualOrder(int type) = 0;
   virtual Order* InitFromPending(PendingOrder* pend) = 0;
};
