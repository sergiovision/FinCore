#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <XTrade\IUtils.mqh>
#include <XTrade\Orders.mqh>

#ifdef THRIFT
#include <XTrade\TradeConnector.mqh>
#endif

IUtils* CreateUtils(short Port, string EA) export 
{
   return new MQL5Utils(Port, EA);  
}

class MQL5Utils: public IUtils
{
protected:
   ITradeService* service;
public:
   MQL5Utils(short Port, string EA)
   {
       Symbol = _Symbol;
       Period = (ENUM_TIMEFRAMES)_Period;
#ifdef THRIFT   
       service = new TradeConnector(Port, EA);
#else 
       service = new ITradeService(Port, EA);
#endif
   }

   virtual ~MQL5Utils()
   {
//#ifdef DEFINE_DLL_TCP   
       //if (hasTcpConnection)
       //  DestroyLib(chartId);
//#endif

       DELETE_PTR(service);       
   }   
   
   ITradeService* Service()
   {
      return service;  
   }   
   
   bool IsMQL5()
   {
      return true;
   }
   
   double Ask()
   {
      return SymbolInfoDouble(Symbol, SYMBOL_ASK);
   }
   
   double Bid()
   {
      return SymbolInfoDouble(Symbol, SYMBOL_BID);
   }
   
   double Ask(string sym)
   {
      return SymbolInfoDouble(sym, SYMBOL_ASK);
   }
   
   double Bid(string sym)
   {
      return SymbolInfoDouble(sym, SYMBOL_BID);
   }

   datetime CurrentTimeOnTF()
   {
      datetime Time[];
      int count = 1;   // number of elements to copy
      ArraySetAsSeries(Time, true);
      CopyTime(Symbol, Period,0,count,Time);
      return Time[0];
   }
   
   bool SelectOrder(long ticket)
   {
      return PositionSelectByTicket(ticket);
   }
   
   bool SelectOrderByPos(int index)
   {
      ENUM_ACCOUNT_MARGIN_MODE margin_mode=(ENUM_ACCOUNT_MARGIN_MODE)AccountInfoInteger(ACCOUNT_MARGIN_MODE);
      //---
      if(margin_mode==ACCOUNT_MARGIN_MODE_RETAIL_HEDGING)
        {
         ulong ticket=PositionGetTicket(index);
         if(ticket==0)
            return(false);
        }
      else
        {
         string name=PositionGetSymbol(index);
         if(name=="")
            return(false);
        }
      return true;     
   }
   
   long OrderTicket()
   {
      return (int)PositionGetInteger(POSITION_IDENTIFIER);
   }
   
   int TimeMinute(datetime date)
   {
      MqlDateTime tm;
      TimeToStruct(date,tm);
      return(tm.min);
   }

   long GetAccountNumer()
   {
      return AccountInfoInteger(ACCOUNT_LOGIN);
   }
   double OrderSwap()
   {
      return PositionGetDouble(POSITION_SWAP);
   }
   string OrderSymbol()
   {
      string sym = PositionGetString(POSITION_SYMBOL);
      return sym;
   }
   string OrderComment()
   {
      return PositionGetString(POSITION_COMMENT);
   }
   double OrderProfit()
   {
      return PositionGetDouble(POSITION_PROFIT);
   }
   double OrderCommission()
   {
      return PositionGetDouble(POSITION_COMMISSION);
   }
   
   int OrderType()
   {
      return (int)PositionGetInteger(POSITION_TYPE);
   }
   
   int OrderMagicNumber()
   {
      int magic = (int)PositionGetInteger(POSITION_MAGIC);
      return magic;
   }
   
   double OrderLots()
   {
      return PositionGetDouble(POSITION_VOLUME);   
   }
   
   double OrderOpenPrice()
   {
      return PositionGetDouble(POSITION_PRICE_OPEN);   
   }
   
   double OrderStopLoss()
   {
      return PositionGetDouble(POSITION_SL);   
   }
   
   double OrderTakeProfit()
   {
      return PositionGetDouble(POSITION_TP);   
   }
         
   datetime OrderOpenTime()
   {
      return((datetime)PositionGetInteger(POSITION_TIME));
   }
   
   datetime OrderExpiration()
   {
      return((datetime)OrderGetInteger(ORDER_TIME_EXPIRATION));
   }
   
   bool IsTesting()
   {
      return MQLInfoInteger((int)MQL5_TESTING) ? true : false;
      //return (bool)MQLInfoInteger(MQL_TESTER);
   }
   
   bool IsVisualMode()
   {
      return (bool)MQLInfoInteger(MQL_VISUAL_MODE);
   }

   bool RefreshRates()
   {   
      //--- refresh rates
      //if(!m_symbol.RefreshRates())
      //   return(false);
      //--- protection against the return value of "zero"
      //if(m_symbol.Ask()==0 || m_symbol.Bid()==0)
      //   return(false);
      //---
      return SymbolInfoTick(Symbol, tick);
   }
   
   double AccountBalance()
   {
      return(AccountInfoDouble(ACCOUNT_BALANCE));
   }
   
   int Spread()
   {
       return (int)SymbolInfoInteger(this.Symbol, SYMBOL_SPREAD);
   }

   int StopLevel()
   {
      int stop = (int)SymbolInfoInteger(this.Symbol, SYMBOL_TRADE_STOPS_LEVEL);
      return (int)stop;
   }
   
   int OrdersTotal()
   {
       return PositionsTotal();
   }
   
   double StopLevelPoints()
   {
      return StopLevel()*Point();
   }
   
   ////////////////////////////////////////////////////////////////////////////////////
   ////////////////////////////////////////////////////////////////////////////////////
   double iCustom(ENUM_TIMEFRAMES period, string name, int bufIndex, int shift)
   {
       int handle = iCustom(Symbol, period, name, bufIndex, shift);
       if (handle != INVALID_HANDLE)
       {
           double result[1];
           CopyBuffer(handle, bufIndex, shift, 1, result);
           IndicatorRelease(handle);    
           return result[0];
       }
       return 0;
   }
   
   virtual int GetIndicatorData(CIndicator& indi, int BuffIndex, int startPos, int Count, double &Buffer[])
   { 
       //ArraySetAsSeries(Buffer, true);
       return CopyBuffer(indi.Handle(), BuffIndex, startPos, Count, Buffer);
   }
   
    virtual bool GetIndicatorMinMax( CIndicator& indi, double& Min, double& Max, TYPE_TREND& trend, int BuffIndex, int numBars)
    {
        int i = numBars - 1;
        double value = 0;
        Min = DBL_MAX;
        Max = DBL_MIN;
        double aver = 0;
        int count = numBars;
        double Buff[];
        ArrayResize(Buff, numBars);
        ArraySetAsSeries(Buff, true);
        if (CopyBuffer(indi.Handle(), BuffIndex, 0, numBars, Buff) > 0)
        {
           i = ArrayMinimum(Buff);
           if (i >= 0)
              Min = Buff[i];
           i = ArrayMaximum(Buff);
           if (i >= 0)
              Max = Buff[i];
           i = numBars - 1;   
           for (; i >= 0; i--)
           {
             value = Buff[i];
             aver += value;
           }
        } else {
            Utils.Info(StringFormat("CopyBuffer failed for Indicator: %s", indi.Name()));
            return false;
        }    
        //if (count == 0)
        //   return false;
        aver = aver / count;
        if(aver < value)
           trend=UPPER;
        if(aver > value) 
           trend=DOWN;
        if(aver == value) 
           trend=LATERAL;
        if ((Min == DBL_MAX) || (Max == DBL_MIN))
          return false;
        return true;
    }

   virtual int iCustomHandle(ENUM_TIMEFRAMES period, string name, int param1, int param2, int param3) 
   {
       if (param1 == -1)
         return iCustom(Symbol, period, name);       
       if (param2 == -1)
         return iCustom(Symbol, period, name, param1);       
       
       if (param3 == -1)
         return iCustom(Symbol, period, name, param1, param2);     
           
       return iCustom(Symbol, period, name, param1, param2, param3);       
   }
   
   int Bars() 
   {
     return (int)Utils.ChartFirstVisibleBar() +(int) ChartGetInteger(0, CHART_VISIBLE_BARS) ;
   }

};
