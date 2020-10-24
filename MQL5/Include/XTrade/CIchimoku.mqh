#property library
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <XTrade\InputTypes.mqh>
#include <XTrade\IndiBase.mqh>
#include <XTrade\Orders.mqh>

#define MODE_TENKANSEN  0

#define  MODE_KIJUNSEN 1
 
#define MODE_SENKOUSPANA 2
 
#define MODE_SENKOUSPANB 3
 
#define MODE_CHIKOUSPAN  4
 

class CIchimoku : public IndiBase
  {
protected:
   int               m_tenkan_sen;
   int               m_kijun_sen;
   int               m_senkou_span_b;
   bool TrailIchiLevel(Order& order, double ask, double bid, double SL, double TP, double level);
public:
   CIchimoku();
   ~CIchimoku(void);
   virtual bool Init(ENUM_TIMEFRAMES timeframe);
   virtual void Process();
   virtual void Trail(Order &order, int indent);
   virtual void Delete();
   int               TenkanSenPeriod(void)        const { return(m_tenkan_sen);    }
   int               KijunSenPeriod(void)         const { return(m_kijun_sen);     }
   int               SenkouSpanBPeriod(void)      const { return(m_senkou_span_b); }
   virtual double    GetData(const int buffer_num,const int index) const;
   double            TenkanSen(const int index) const;
   double            KijunSen(const int index) const;
   double            SenkouSpanA(const int index) const;
   double            SenkouSpanB(const int index) const;
   double            ChinkouSpan(const int index) const;
   bool              Initialize(const string symbol,const ENUM_TIMEFRAMES period,
                                const int tenkan_sen,const int kijun_sen,const int senkou_span_b);
   //--- method of identifying
   virtual int       Type(void) const { return(IND_ICHIMOKU); }
  };
//+------------------------------------------------------------------+
//| Constructor                                                      |
//+------------------------------------------------------------------+
CIchimoku::CIchimoku() 
   :m_tenkan_sen(-1)
   ,m_kijun_sen(-1)
   ,m_senkou_span_b(-1)
{
   m_name = "Ichimoku";
}

bool CIchimoku::Init(ENUM_TIMEFRAMES timeframe)
{
   if (Initialized())
      return true;
   SetSymbolPeriod(Utils.Symbol, timeframe);
   m_tenkan_sen = (int)GET(IshimokuPeriod1);
   m_kijun_sen = (int)GET(IshimokuPeriod2);
   m_senkou_span_b = (int)GET(IshimokuPeriod3);

   MqlParam params[];   
   ArrayResize(params,4);
   params[0].type = TYPE_STRING;
   params[0].string_value = m_name;
   params[1].type = TYPE_INT;
   params[1].integer_value = m_tenkan_sen;
   params[2].type = TYPE_INT;
   params[2].integer_value = m_kijun_sen;
   params[3].type = TYPE_INT;
   params[3].integer_value = m_senkou_span_b;   
   m_bInited = Create(Utils.Symbol, (ENUM_TIMEFRAMES)m_period, IND_CUSTOM, 4, params);
   m_bInited = Initialize(Utils.Symbol,(ENUM_TIMEFRAMES)m_period, m_tenkan_sen, m_kijun_sen, m_senkou_span_b);
   if (m_bInited)
   { 
      FullRelease(!Utils.IsTesting());
      AddToChart(Utils.Trade().ChartId(), Utils.Trade().SubWindow());
      return true;
   }
   Utils.Info(StringFormat("Indicator %s - failed to load!!!!!!!!!!!!!", m_name));
   return false;
}

void CIchimoku::Delete()
{
    if (Handle() != INVALID_HANDLE)
    {
        DeleteFromChart(Utils.Trade().ChartId(), Utils.Trade().SubWindow());
    }
}
      
//+------------------------------------------------------------------+
//| Destructor                                                       |
//+------------------------------------------------------------------+
CIchimoku::~CIchimoku(void)
{
   Delete();
}

//+------------------------------------------------------------------+
//| Initialize indicator with the special parameters                 |
//+------------------------------------------------------------------+
bool CIchimoku::Initialize(const string symbol,const ENUM_TIMEFRAMES period,
                            const int tenkan_sen,const int kijun_sen,const int senkou_span_b)
{
   m_tenkan_sen   =tenkan_sen;
   m_kijun_sen    =kijun_sen;
   m_senkou_span_b=senkou_span_b;
#ifdef  __MQL5__
   if(CreateBuffers(symbol,period,5))
   {
      //--- string of status of drawing
      m_name  ="Ichimoku";
      m_status="("+symbol+","+PeriodDescription()+","+
               IntegerToString(tenkan_sen)+","+IntegerToString(kijun_sen)+","+
               IntegerToString(senkou_span_b)+") H="+IntegerToString(m_handle);
      //--- save settings
      //--- create buffers
      ((CIndicatorBuffer*)At(0)).Name("TENKANSEN_LINE");
      ((CIndicatorBuffer*)At(1)).Name("KIJUNSEN_LINE");
      ((CIndicatorBuffer*)At(2)).Name("SENKOUSPANA_LINE");
      ((CIndicatorBuffer*)At(2)).Offset(kijun_sen);
      ((CIndicatorBuffer*)At(3)).Name("SENKOUSPANB_LINE");
      ((CIndicatorBuffer*)At(3)).Offset(kijun_sen);
      ((CIndicatorBuffer*)At(4)).Name("CHIKOUSPAN_LINE");
      ((CIndicatorBuffer*)At(4)).Offset(-kijun_sen);
      //--- ok
      return(true);
   }
   //--- error
   return(false);
#else 
   return(true);
#endif   
}

double CIchimoku::GetData(const int buffer_num,const int index) const
{   
#ifdef __MQL4__
   double val = iIchimoku(NULL,m_period,m_tenkan_sen,m_kijun_sen,m_senkou_span_b,buffer_num,index);
   //Utils.Info(StringFormat("Ichimoku BufIndex=%d, index=%d, val=%g", buffer_num, index, val));
   return val;
#else    
   double Buff[1];
   CopyBuffer(m_handle, buffer_num, index, 1, Buff); 
   //Utils.Info(StringFormat("Ichimoku BufIndex=%d, index=%d, val=%g", buffer_num, index, val));
   return Buff[0];
#endif   
}
//+------------------------------------------------------------------+
//| Access to TenkanSen buffer of "Ichimoku Kinko Hyo"               |
//+------------------------------------------------------------------+
double CIchimoku::TenkanSen(const int index) const
{
   return(GetData(MODE_TENKANSEN,index));
}
//+------------------------------------------------------------------+
//| Access to KijunSen buffer of "Ichimoku Kinko Hyo"                |
//+------------------------------------------------------------------+
double CIchimoku::KijunSen(const int index) const
{
   return(GetData(MODE_KIJUNSEN,index));
}
//+------------------------------------------------------------------+
//| Access to SenkouSpanA buffer of "Ichimoku Kinko Hyo"             |
//+------------------------------------------------------------------+
double CIchimoku::SenkouSpanA(const int index) const
{
   return(GetData(MODE_SENKOUSPANA,index));
}
//+------------------------------------------------------------------+
//| Access to SenkouSpanB buffer of "Ichimoku Kinko Hyo"             |
//+------------------------------------------------------------------+
double CIchimoku::SenkouSpanB(const int index) const
{
   return(GetData(MODE_SENKOUSPANB,index));
}
//+------------------------------------------------------------------+
//| Access to ChikouSpan buffer of "Ichimoku Kinko Hyo"              |
//+------------------------------------------------------------------+
double CIchimoku::ChinkouSpan(const int index) const
{
   return(GetData(MODE_CHIKOUSPAN,index));
}

//#endif

void CIchimoku::Process()
{
    double TRedValue = TenkanSen(0);
    double KBlueValue = KijunSen(0);
    double ABValue = SenkouSpanA(0);
    double BBValue = SenkouSpanB(0);

    double IshiMax = MathMax(ABValue, BBValue);
    double IshiMin = MathMin(ABValue, BBValue);
    double mediaPrice = (Utils.tick.ask + Utils.tick.bid)/2.0;
    double delta = (Utils.Spread() + Utils.StopLevel() + GET(TrailingIndent) )*Point();
    //signals.deltaCross = delta;
    
    //signal.Init(signal.UseAsFilter);
    //signal.type = SignalQuiet;
    //int trend = 
    
    if (Utils.HigherThanAll(KBlueValue, IshiMax, IshiMin))
        Utils.Trade().SetTrend(UPPER);
        //signals.Trend = UPPER;
    else {
       if (Utils.LowerThanAll(KBlueValue, IshiMin, IshiMax))
           Utils.Trade().SetTrend(DOWN);
           //signals.Trend = DOWN;
       else 
           Utils.Trade().SetTrend(LATERAL);
           //signals.Trend = LATERAL;
    }

    //double mfiMax = 0;
    //double mfiMin = 0;
    //TYPE_TREND mfiTrend = 0;
    //Utils.GetIndicatorMinMax(signals.MFI, mfiMin, mfiMax, mfiTrend, 0, CANDLE_PATTERN_MAXBARS);
   
    double IchiMinFlat = 0;          
    double IchiMaxFlat = 0;    
    TYPE_TREND IchiMiniTrend = 0;     
    
    Utils.GetIndicatorMinMax(this, IchiMinFlat, IchiMaxFlat, IchiMiniTrend, MODE_SENKOUSPANB, ISHIMOKU_PLAIN_NOTRADE);
    double differenceM = MathAbs(IchiMaxFlat - IchiMinFlat)/delta;
    if ((differenceM <= 1.0) ) 
    {
       Utils.Trade().SetTrend(LATERAL);
       //signals.StatusString = StringFormat("TREND(%s) ABdiff=%g", EnumToString(signals.Trend), differenceM);
    } else 
    {
       //signals.StatusString = StringFormat("TREND(%s) ABdiff=%g", EnumToString(signals.Trend),  differenceM);
    }  
    
    //if (!signal.UseAsFilter)
    { 
         MqlRates rates[];
         ArrayResize(rates, 2);
         ArraySetAsSeries(rates, true);    
         CopyRates(Utils.Symbol, (ENUM_TIMEFRAMES)m_period, 0, 2, rates);
       double cloudDistance = 0;
       if (Utils.tick.bid > IshiMax)
           cloudDistance = MathAbs((Utils.tick.bid - IshiMax)/Point());
       if (Utils.tick.ask < IshiMin)
           cloudDistance = MathAbs((IshiMin - Utils.tick.ask)/Point());
           
       double defaultCloudDistance = Utils.Trade().ATROnIndicator(SL_PERCENTILE);
       
       bool isUpCandle =  ((rates[1].close > IshiMax) && (rates[1].close > rates[1].open)); //(Utils.tick.bid > bandUpperMax) &&// &&  && (rates[1].close > rates[1].open);
       bool isDownCandle = (rates[1].close < IshiMin) && (rates[1].close < rates[1].open);// && (rates[0].close < rates[0].open));// &&  && (rates[1].close < rates[1].open);
       
       double TenkanMin = 0;
       double TenkanMax = 0;
       TYPE_TREND TenkanTrend = 0;
       Utils.GetIndicatorMinMax(this, TenkanMin, TenkanMax, TenkanTrend, MODE_TENKANSEN, CANDLE_PATTERN_MAXBARS);
       
       double KijunMin = 0;
       double KijunMax = 0;
       TYPE_TREND KijunTrend = 0;
       Utils.GetIndicatorMinMax(this, KijunMin, KijunMax, KijunTrend, MODE_KIJUNSEN, CANDLE_PATTERN_MAXBARS);
       
       // First Sell Signal             
       if ( GET(AllowBUY) //&& (TRedValue > KBlueValue) && 
            //&& isUpCandle 
            && (Utils.Trade().Trend() != DOWN)
            && (cloudDistance < defaultCloudDistance)
            && (Utils.tick.bid > IshiMax)
            //((Utils.tick.bid > bandUpperMax) &&( signals.Trend == LATERAL) ))
            )
       {
           RaiseMarketSignal(1, StringFormat("BUY On %s", EnumToString((ENUM_INDICATORS)GET(FilterIndicator))));
           return;
       }
       
       // First Buy Signal
       if ( GET(AllowSELL)// && (TRedValue < KBlueValue ) &&
             && (Utils.Trade().Trend() != UPPER)
             //&& isDownCandle //|| ((Utils.tick.bid > bandUpperMax) &&( signals.Trend == LATERAL) ))
             && (cloudDistance < defaultCloudDistance)
             && (Utils.tick.ask < IshiMin)
          )
       {
           RaiseMarketSignal(-1, StringFormat("SELL On %s", EnumToString((ENUM_INDICATORS)GET(FilterIndicator))));
           return ;
       }
    }
}

void CIchimoku::Trail(Order &order, int indent)
{      
    if (!m_bInited)
        return;
    //if (GET(EnableNews))
    //{
    //    if (!signals.InNewsPeriod)
    //      return;
    //}
    if ( order.Valid() && order.Select() )
    {

       double TRedValue = TenkanSen(0);
       double KBlueValue = KijunSen(0);
       double ABValue = SenkouSpanA(0);
       double BBValue = SenkouSpanB(0);

       double IshiMax = MathMax(ABValue, BBValue);
       double IshiMin = MathMin(ABValue, BBValue);             
       double IshiMedium = (IshiMax + IshiMin)/2.0;
       double mediaPrice = (Utils.tick.ask + Utils.tick.bid)/2.0;
       double Pt = Point();
       Utils.Trade().SetTrailDelta((Utils.Spread() + indent + Utils.StopLevelPoints())*Pt);
       
       //order.stopLoss = Utils.OrderStopLoss();
       //order.takeProfit = Utils.OrderTakeProfit();    
       order.openPrice = Utils.OrderOpenPrice();
       order.profit = order.RealProfit();
                
       double SL = order.StopLoss(false);
       double TP = order.TakeProfit(false);
       double OP = order.openPrice;
       double Profit = order.profit;             
       if (MathAbs(OP - mediaPrice) <= (Utils.Trade().TrailDelta()*2))
          return; // Skip trailing
        double Spread = Utils.Spread() * Pt;
   
       if (order.type == OP_BUY)
       {
          double startLevel = order.openPrice + Spread;
          if (TrailLevel(order, Utils.tick.ask, Utils.tick.bid, SL, TP, IshiMax, startLevel))
             return;
       }
       if (order.type == OP_SELL)
       {
          double startLevel = order.openPrice - Spread;
          if (TrailLevel(order, Utils.tick.ask, Utils.tick.bid, SL, TP, IshiMin, startLevel))
             return;
       }
       // methods.ChangeOrder(order, SL, TP, Utils.OrderExpiration(), methods.TrailingColor);
    }
   
}

