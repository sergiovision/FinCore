//+------------------------------------------------------------------+
//|                                                       IUtils.mqh |
//|                        Copyright 2018, MetaQuotes Software Corp. |
//|                                             https://www.mql5.com |
//+------------------------------------------------------------------+
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <XTrade\InitMQL.mqh>
#include <XTrade\ITradeService.mqh>
#include <Indicators\Indicator.mqh>
#include <XTrade\ITrade.mqh>


class Order;

class IUtils 
{
protected:
   ITrade* trade;
   
   // RISK Management Params
   double  dailyProfit; 
   double  dailyMaxGain;  
   bool hasTcpConnection;

public:
   long chartId;
   bool isHttp;

   IUtils()
   {
      dailyProfit = 0;
      dailyMaxGain = 0;
      hasTcpConnection = false;
      chartId = ChartID();
      isHttp = true;
   }
   
   string Symbol;
   ENUM_TIMEFRAMES Period;
   MqlTick tick;
   
   int DayOfWeek()
   {
      MqlDateTime tm;
      TimeCurrent(tm);
      return(tm.day_of_week);
   }
   
   virtual bool CheckRiskManager() 
   {       
       Signal retSignal(SignalToServer, SIGNAL_CHECK_TRADEALLOWED, this.GetAccountNumer(), Utils.chartId);
       CJAVal obj;
       obj["Balance"] = AccountInfoDouble(ACCOUNT_BALANCE);
       obj["Equity"] = AccountInfoDouble(ACCOUNT_EQUITY);
       obj["Account"] = (long)Utils.AccountNumber();
       retSignal.obj["Data"].Add(obj);
       Signal* newSignal = this.Service().SendSignal(&retSignal);
       if (newSignal != NULL)
       {
           if (newSignal.Value > 0) {
               if (newSignal.obj.FindKey("Data"))
               {   
                  Print(newSignal.obj["Data"].ToStr());
               }
               DELETE_PTR(newSignal);
               return false;
           }
       }
       DELETE_PTR(newSignal);
       return true;
   }
   
   
#ifdef   DEFINE_DLL_TCP
   
   virtual bool SetupTcpConnection(string address, int port, bool isEA) {
      SerializableEntity pars;
      pars.SetValue("Account", Utils.AccountNumber());
      pars.SetValue("ChartId", Utils.chartId);
      pars.SetValue("Host", address);
      pars.SetValue("Port", (long)port);
      pars.SetValue("Protocol", "tcp");
      if (isEA)
      {
         pars.SetValue("WaitOp", (long)HTTP_FINCORE_OP);
         pars.SetValue("WaitTimeout", (long)HTTP_FINCORE_TIMEOUT);
      }
      else {
         pars.SetValue("WaitOp", (long)HTTP_FINCORE_OP);
         pars.SetValue("WaitTimeout", (long)HTTP_FINCORE_TIMEOUT_INDI);
      }

      string parameters = pars.toString();
      string result = SetupLib("MQLConnector.dll", "MQLConnector.Library1", parameters);
      if (StringLen(result) > 0)
      {
         hasTcpConnection = false;
         Print("Failed to load dll: " + result);
         return false;
      }
      isHttp = false;
      hasTcpConnection = true;
      return true;
   }
      
#else 

#define DoListenMessage    
#define DoPostMessage
#define DoSendMessage

#endif   

   virtual double GetDailyProfit() 
   {
      return dailyProfit;
   }
   
   virtual double GetMaxGain() 
   {
      return dailyMaxGain;
   }
   
   virtual void SetDailyProfit(double profit)
   {
      dailyProfit = profit;
      if (dailyProfit > 0)
         dailyMaxGain = MathMax(dailyMaxGain, dailyProfit);
   }

   virtual ITrade* Trade() { return trade; }
   virtual void SetTrade(ITrade* tr) { trade = tr; }
   virtual bool IsMQL5() { return true; }
   virtual ITradeService* Service() { return NULL; }
   virtual double Ask() {return 0;}
   virtual double Bid() {return 0;}   
   virtual double Ask(string sym) {return 0;}
   virtual double Bid(string sym) {return 0;}

   virtual datetime CurrentTimeOnTF() {return 0;}
   virtual bool SelectOrder(long ticket) {return false;}
   virtual bool SelectOrderBySymbol(string sym) {
       bool Sel = PositionSelect(sym);
       return Sel;
   }
   virtual bool SelectOrderByPos(int Positiong) {return false;}
   
   virtual int OrdersTotal() {return 0;}
   virtual long GetAccountNumer() {return 0;}
   virtual double OrderSwap() {return 0;}
   virtual string OrderSymbol() {return "";}
   virtual string OrderComment() {return "";}
   virtual double OrderProfit() {return 0;}
   virtual double OrderCommission() {return 0;}
   virtual int    OrderType() {return 0;}
   virtual int    OrderMagicNumber() {return 0;}   
   virtual double OrderLots() {return 0;}
   virtual double OrderOpenPrice() {return 0;}
   virtual datetime OrderOpenTime() {return 0;}
   virtual double OrderStopLoss() {return 0;}
   virtual double OrderTakeProfit() {return 0;}
   virtual datetime OrderExpiration() {return 0;}
   virtual long OrderTicket() {return 0;}
   virtual bool IsTesting() {return false;}
   virtual long AccountNumber()
   {
      return AccountInfoInteger(ACCOUNT_LOGIN); 
   }
   
   virtual bool IsVisualMode() {    return false;  }
   
   virtual bool RefreshRates() {
      Info("RefreshRates not overriden!");
      return false;
   }
   
   virtual double AccountBalance() {
      return AccountInfoDouble(ACCOUNT_BALANCE);
   }
   
   virtual double AccountEquity() {
      return AccountInfoDouble(ACCOUNT_EQUITY);
   }
   
  
   virtual void Info(string message)
   {
      Print(message);
      Service().Log(message);
   }
   
   /*
   virtual void Info(long ticket, string message)
   {
      Print(StringFormat(" order: %d: %s", ticket, message));
      Service().Log(ticket, message);
   }
   */
   
   virtual void Debug(string message)
   {
      if (!IsTesting() || IsVisualMode())
         Print(message);
   }
   
   double NormalizePrice(string symbol, double price, double tck = 0)
   {
      double _tick = tck ? tck : SymbolInfoDouble(symbol,SYMBOL_TRADE_TICK_SIZE);
      int _digits = (int)SymbolInfoInteger(symbol,SYMBOL_DIGITS);
      
      if (tck) 
         return NormalizeDouble(MathRound(price/_tick)*_tick,_digits);
      else 
         return NormalizeDouble(price,_digits);
   }
   
   //////////////////////////
   virtual int Spread() {return 0;}
   virtual int StopLevel() {return 0;}
   virtual double StopLevelPoints() {return 0;}   
   
   virtual int TimeMinute(datetime date) {return 0;}
   //virtual bool  OrderClose(long ticket,double lots, double price, int slippage, color arrow_color) {return false;}
   //virtual bool  OrderClosePartially(long ticket, double lots, double price, int slippage) {return false;}
   //virtual bool OpenOrder(Order& order, int slippage) {return false;}

   //virtual bool  OrderModify(Order& order) {return false;}
     
   virtual bool IsNettingAccount() { return false; };

   //Rates  
   //virtual double iClose(ENUM_TIMEFRAMES tf, int shift) {return 0;}
   /// Indicators  
   virtual double iATR(ENUM_TIMEFRAMES timeframe, int period, int shift) {  return 0; }
   virtual double iMA(ENUM_TIMEFRAMES timeframe, int ma_period, int ma_shift, ENUM_MA_METHOD ma_method, ENUM_APPLIED_PRICE applied_price, int shift) {return 0;}
   virtual double iRSI(ENUM_TIMEFRAMES period, int ma_period, ENUM_APPLIED_PRICE  applied_price, int shift) {return 0;}
   virtual double iBands(ENUM_TIMEFRAMES period, int  bands_period, int  bands_shift, double  deviation, ENUM_APPLIED_PRICE  applied_price, int bufIndex, int shift) 
   {return 0;}
   virtual double iCustom(ENUM_TIMEFRAMES period, string name, int bufIndex, int shift) {return 0;}
   virtual double iCustom(ENUM_TIMEFRAMES period, string name, int param1, int param2, int param3, int bufIndex, int shift) {  return 0;  }
   virtual int iCustomHandle(ENUM_TIMEFRAMES period, string name, int param1, int param2, int param3) {  return 0;  }
   virtual int GetIndicatorData(CIndicator& indi, int BuffIndex, int startPos, int Count, double &Buffer[])
   {
     this.Info("GetIndicatorData not implemented");
     return -1;
   }
       
    virtual bool GetIndicatorMinMax( CIndicator& indi, double& Min, double& Max, TYPE_TREND& trend, int BuffIndex, int numBars)
    {
        this.Info("GetIndicatorMinMax not implemented");
        return false;
    }
    
    double ArrayPercentile (double& array[], double rank)
    {
         double copy[];
         int size=ArraySize(array);
         ArrayResize(copy,size);
         ArrayCopy(copy,array,0,0,WHOLE_ARRAY);
         ArraySort(copy);
         
         double percentile = rank*size;
         
         int arrayindex = (int)MathRound(percentile);
         percentile = copy[arrayindex-1];
         
         return(percentile);
    }
    
    double PercentileATR(string sym, ENUM_TIMEFRAMES tf, double rank, int atr_Period, int shift)
    {
         MqlRates rates[]; 
         ArraySetAsSeries(rates,true); 
         int copied = CopyRates(sym, tf, shift, atr_Period, rates);
         if (copied > 0)
         {
            double candles[];
            ArrayResize(candles, copied);
            for (int i = 0; i<copied;i++)
               candles[i] = rates[i].high-rates[i].low;
            return ArrayPercentile(candles, rank);            
         }
         return 0;
    }
        
    double PercentileATRIndi(ENUM_TIMEFRAMES tf, const double& low[], const double& high[], double rank, int atr_Period, int shift)
    {
         double candles[];
         int barsPerPeriod = (int)BarsPerPeriod(tf);
         if (barsPerPeriod == 1)
         {
            int arraySize = (int)atr_Period*barsPerPeriod;
            ArrayResize(candles, arraySize);
            int j = 0;
            for (int i = shift; i < (arraySize + shift);i++)
            {
               candles[j] = high[i]-low[i];
               j++;
            }
            return ArrayPercentile(candles, rank);            
         } else {
         
            return PercentileATR(_Symbol, tf, rank, atr_Period, shift); 
         
         }
    }
    
    double BarsPerPeriod(ENUM_TIMEFRAMES tf)
    {
       double period = PeriodSeconds(_Period)/60;
       double acttf = PeriodSeconds(tf)/60;
       double barsPerTma = (acttf / period);
       return barsPerTma;
    }

    
    void AddBuffer(int index, double& Buff[], bool asSeries, string label, int drawBegin, ENUM_INDEXBUFFER_TYPE indi_type = INDICATOR_DATA, double gapValue = EMPTY_VALUE)
    {
         SetIndexBuffer( index, Buff, indi_type );
         ArraySetAsSeries(Buff, asSeries);
         ArrayInitialize(Buff,gapValue); 
         PlotIndexSetInteger(index,PLOT_DRAW_BEGIN, drawBegin);
         
#ifdef __MQL5__
         PlotIndexSetString(index,PLOT_LABEL,label);  
         PlotIndexSetDouble(index,PLOT_EMPTY_VALUE, gapValue);
#else    
         SetIndexEmptyValue(index, gapValue);
#endif      
    }
    
        int ChartFirstVisibleBar(const long chart_ID=0) 
        { 
      //--- prepare the variable to get the property value 
         long result=-1; 
      //--- reset the error value 
         //ResetLastError(); 
      //--- receive the property value 
         if(!ChartGetInteger(chart_ID,CHART_FIRST_VISIBLE_BAR,0,result)) 
         { 
            //--- display the error message in Experts journal 
            //Print(__FUNCTION__+", Error Code = ",GetLastError()); 
         } 
      //--- return the value of the chart property 
         return((int)result); 
        }

    
    void SetIndiName(string name)
    {   
        IndicatorSetString(INDICATOR_SHORTNAME,name);
    }

   
      CROSS_TYPE CandleBodyCross(double& price[], MqlRates& rates[])
      {
         for(int i = ArraySize(rates)-1; i>=0;i--)
         {
            if ((rates[i].open <= price[i]) && (rates[i].close >= price[i]))
            {
               if (rates[i].open > rates[i].close)
                   return CROSS_DOWN;
               else 
                   return CROSS_UP;    
            }
         }
         return CROSS_NO;
      }
      
      CROSS_TYPE CandleCross(double price, MqlRates& rates[])
      {
         for(int i = 0; i < ArraySize(rates);i++)
         {
            if ((rates[i].low  <= price) && (rates[i].high >= price))
            {
               if (rates[i].open > rates[i].close)
                   return CROSS_DOWN;
               else 
                   return CROSS_UP;    
            }
         }
         return CROSS_NO;
      }

      bool LowerThanAll(double price, double A, double B, double C, double D)
      {
          return (price < A) && (A <= B) && (B <= C) && (C <= D);
      }
      
      bool LowerThanAll(double price, double A, double B)
      {
          return (price < A) && (A <= B);
      }

      
      bool HigherThanAll(double price, double A, double B, double C, double D)
      {
          return (price > A) && (A >= B) && (B >= C) && (C >= D);
      }
      
      bool HigherThanAll(double price, double A, double B)
      {
          return (price > A) && (A >= B);
      }
      
      virtual int Bars() { 
          Print("Bars not implemented!");
          return 0;
      }
      
      double CalculateTMA(ENUM_TIMEFRAMES tf, int tmaPeriod, int inx, const datetime& time[], const double& close[])
      {
         ENUM_TIMEFRAMES curtf = (ENUM_TIMEFRAMES)_Period;
         double dblSum  = (tmaPeriod + 1) * iClose(_Symbol,tf,inx);
         double dblSumw = (tmaPeriod + 1);
         int jnx, knx;         
         for ( jnx = 1, knx = tmaPeriod; jnx <= tmaPeriod; jnx++, knx-- )
         {
            double closeValue = 0;
            if (curtf == tf)
               closeValue = close[inx+jnx];
            else
               closeValue = iClose(_Symbol,tf,inx+jnx);
            dblSum  += ( knx * closeValue );
            dblSumw += knx;      
            
            if ( jnx <= inx )
            {         
               datetime timeValue = 0;
               if (curtf == tf)
                  timeValue = time[inx-jnx];
               else
                  timeValue = iTime(_Symbol,tf,inx-jnx);
      
               if (timeValue > time[0])
               {
                  //Print (" TimeFrameValue ", TimeFrameValue , " inx ", inx," jnx ", jnx, " iTime(_Symbol,TimeFrameValue,inx-jnx) ", TimeToStr(iTime(_Symbol,TimeFrameValue,inx-jnx)), " Time[0] ", TimeToStr(Time[0])); 
                  continue;
               }
               double closeValue2 = 0;
               if (curtf == tf)
                  closeValue2 = close[inx-jnx];
               else
                  closeValue2 = iClose(_Symbol,tf,inx-jnx);
      
               dblSum += ( knx * closeValue2 );
               dblSumw += knx;
            }
         }   
         return( dblSum / dblSumw );
      }

   bool ObjExist(string name)
   {
      if (ObjectFind(ChartID(),name) >-1 )
          return true;
      return false;
   }

   bool ObjDelete(string name)
   {
      return ObjectDelete(ChartID(), name);
   }
   
   bool ObjDeleteAll(string prefix)
   {
      return ObjectsDeleteAll(ChartID(), prefix) > 0;
   }
   
   bool ObjDeleteAll()
   {
      return ObjectsDeleteAll(this.ChartId()) > 0;
   }
     
   // Graphical objects
   //+------------------------------------------------------------------+
   //| Create the horizontal line                                       |
   //+------------------------------------------------------------------+
   bool HLineCreate(const string          name="HLine_max",// line name
                 const int             sub_window=0,// subwindow index
                 double                hprice=0,// line price
                 const color           clr=clrRed,        // line color
                 const ENUM_LINE_STYLE style=STYLE_SOLID, // line style
                 const int             width=1,           // line width
                 const bool            back=false,        // in the background
                 const bool            selection=true,    // highlight to move
                 const bool            hidden=true,       // hidden in the object list
                 const long            z_order=0,         // priority for mouse click
                 const string          tooltip="")
   {
      const long  chart_ID = ChartId();
      ObjectDelete(chart_ID,name);
   //--- reset the error value
      ResetLastError();
   //--- create a horizontal line
      if(!ObjectCreate(chart_ID,name,OBJ_HLINE,sub_window,0,hprice))
        {
         Print(__FUNCTION__,
               ": failed to create a horizontal line! Error code = ",GetLastError());
         return(false);
        }
   //--- set line color
      ObjectSetInteger(chart_ID,name,OBJPROP_COLOR,clr);
   //--- set line display style
      ObjectSetInteger(chart_ID,name,OBJPROP_STYLE,style);
   //--- set line width
      ObjectSetInteger(chart_ID,name,OBJPROP_WIDTH,width);
   //--- display in the foreground (false) or background (true)
      ObjectSetInteger(chart_ID,name,OBJPROP_BACK,back);
   //--- enable (true) or disable (false) the mode of moving the line by mouse
   //--- when creating a graphical object using ObjectCreate function, the object cannot be
   //--- highlighted and moved by default. Inside this method, selection parameter
   //--- is true by default making it possible to highlight and move the object
      ObjectSetInteger(chart_ID,name,OBJPROP_SELECTABLE,selection);
      ObjectSetInteger(chart_ID,name,OBJPROP_SELECTED,selection);
   //--- hide (true) or display (false) graphical object name in the object list
      ObjectSetInteger(chart_ID,name,OBJPROP_HIDDEN,hidden);
   //--- set the priority for receiving the event of a mouse click in the chart
      ObjectSetInteger(chart_ID,name,OBJPROP_ZORDER,z_order);
   
      ObjectSetString(chart_ID,name,OBJPROP_TOOLTIP,tooltip);
   //--- successful execution
      return(true);
   }
  
   //+------------------------------------------------------------------+
   //| Move horizontal line                                             |
   //+------------------------------------------------------------------+
   bool HLineMove(const string name="HLine", // line name
                  double       pricel=0, const string tooltip = "") // line price
   {
      const long  chart_ID=ChartId();
   
   //--- if the line price is not set, move it to the current Bid price level
      if(!pricel)
         pricel=SymbolInfoDouble(_Symbol,SYMBOL_BID);
   //--- reset the error value
      ResetLastError();
   //--- move a horizontal line
      if(!ObjectMove(chart_ID,name,0,0,pricel))
        {
         Print(__FUNCTION__,
               ": failed to move the horizontal line! Error code = ",GetLastError());
         return(false);
        }
      ObjectSetString(chart_ID,name,OBJPROP_TOOLTIP,tooltip);
   //--- successful execution
      return(true);
   }
   
   long ChartId()
   { 
      ITrade* parent = Trade();
      if (parent != NULL)
         return parent.ChartId();
      return ChartID(); 
   }
      
};

static IUtils* Utils = NULL;
IUtils* GetUtils()
{
    return Utils;
}

#include <XTrade\MQL5Utils.mqh>
