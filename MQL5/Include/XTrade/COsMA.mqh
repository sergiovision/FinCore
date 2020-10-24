#property library
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <XTrade\IndiBase.mqh>
#include <XTrade\Orders.mqh>
#include <XTrade\InputTypes.mqh>

class COsMA : public IndiBase
{
protected:
   int fast_ema_period;
   int slow_ema_period;
   int signal_period;
public:
   COsMA();
   ~COsMA(void);
   virtual bool Init(ENUM_TIMEFRAMES timeframe);
   virtual void Process();
   virtual void Trail(Order &order, int indent) {}
   virtual void Delete();
   virtual double GetData(const int buffer_num,const int index) const;
   
   virtual int       Type(void) const { return(IND_CUSTOM); }
};
//+------------------------------------------------------------------+
//| Constructor                                                      |
//+------------------------------------------------------------------+
COsMA::COsMA() 
{
   m_name = "OsMAColor";    
}

bool COsMA::Init(ENUM_TIMEFRAMES timeframe)
{
   if (Initialized())
      return true;
   m_period = timeframe;
   SetSymbolPeriod(Utils.Symbol, m_period);
   MqlParam params[4];   
   //ArrayResize(params,4);
   params[0].type = TYPE_STRING;
   params[0].string_value = m_name;
   params[1].type = TYPE_INT;
   params[1].integer_value = 12;
   params[2].type = TYPE_INT;
   params[2].integer_value = 26;
   params[3].type = TYPE_INT;
   params[3].integer_value = 9;
   //params[4].type = TYPE_INT;
   //params[4].integer_value = PRICE_OPEN;
   
   m_bInited = Create(Utils.Symbol, (ENUM_TIMEFRAMES)m_period, IND_CUSTOM, ArraySize(params), params);
   if (m_bInited)
   {
      FullRelease(!Utils.IsTesting());
      AddToChart(Utils.Trade().ChartId(), Utils.Trade().IndiSubWindow());
      return true;

   }
   Utils.Info(StringFormat("Indicator %s - failed to load!!!!!!!!!!!!!", m_name));
   return m_bInited;
}

void COsMA::Delete()
{
    if (Handle() != INVALID_HANDLE)
    {
        DeleteFromChart(Utils.Trade().ChartId(), Utils.Trade().IndiSubWindow());
    }
}

//+------------------------------------------------------------------+
//| Destructor                                                       |
//+------------------------------------------------------------------+
COsMA::~COsMA(void)
{
   Delete();
}

double COsMA::GetData(const int buffer_num,const int index) const
{   
#ifdef __MQL4__   
   double val = iCustom(NULL,m_period,m_name,fast_ema_period,slow_ema_period,signal_period,index);
   //Utils.Info(StringFormat("OsMA BufIndex=%d, index=%d, val=%g", buffer_num, index, val));
   return val;
#else   
   double Buff[1];
   CopyBuffer(m_handle, buffer_num, index, 1, Buff); 
   return Buff[0];
#endif    
}

void COsMA::Process()
{
   double totalMax = 0;
   double totalMin = 0;
   TYPE_TREND totalTrend = 0;
   Utils.GetIndicatorMinMax(this, totalMin, totalMax, totalTrend, 0, 200);
   
   double OSMAValue = GetData(0, 0);          
   double OSMAValuePrev = GetData(0, 1);          
   double osmaMax = 0;
   double osmaMin = 0;
   TYPE_TREND osmaTrend = 0;
   Utils.GetIndicatorMinMax(this, osmaMin, osmaMax, osmaTrend, 0, CANDLE_PATTERN_MAXBARS);
   
   if( (MathAbs(osmaMax/totalMax) < 0.1) && (MathAbs(osmaMin/totalMin) < 0.1) )
      return;
   
   double osMABuf[CANDLE_PATTERN_MAXBARS];
   Utils.GetIndicatorData(this, 0, 0, CANDLE_PATTERN_MAXBARS, osMABuf);
            
   bool osMABUY = (osMABuf[0] > osMABuf[1]) && (osMABuf[2] > osMABuf[1]) && (OSMAValue < 0); // dip
   bool osMASELL = (osMABuf[0] < osMABuf[1]) && (osMABuf[2] < osMABuf[1]) && (OSMAValue > 0); // peak
         
   MqlRates rates[];
   ArrayResize(rates, 2);
   ArraySetAsSeries(rates, true);    
   CopyRates(Utils.Symbol, (ENUM_TIMEFRAMES)m_period, 0, 2, rates);
   
   
   double priceHigh = MathMax(rates[0].high, rates[1].high);
   double priceLow = MathMin(rates[0].low, rates[1].low);

   //bool AfterNews = (!signals.InNewsPeriod); 
   /*if (EnableNews)
   {
      if ( ((signals.Trend == UPPER) || (signals.Trend == LATERAL)) 
            //&& AfterNews
            && (priceLow <= bandLowMin)
            //&& (mfiTrend == UPPER)
            && osMABUY
            )
      {
         signal.Init(false);   
         signal.Value = 1;
         signal.type = SignalBUY;
         if (signals.thrift.GetLastNewsEvent() != NULL)
            signal.SetName(signals.thrift.GetLastNewsEvent().GetName());
         signals.StatusString = StringFormat("TREND(%s) Before News %s On %s ", EnumToString(signals.Trend), EnumToString(signal.type), EnumToString(OsMAIndicator));
         return true;
      }         
      if ( ((signals.Trend == DOWN) || (signals.Trend == LATERAL))
            && AfterNews
            //&& (priceHigh >= bandUpperMax)
            //&& (mfiTrend == DOWN)
            && osMASELL
             )
      {
         signal.Init(false);
         signal.Value = -1; 
         signal.type = SignalSELL;
         if (signals.thrift.GetLastNewsEvent()!= NULL)
            signal.SetName(signals.thrift.GetLastNewsEvent().GetName());
         signals.StatusString = StringFormat("TREND(%s) Before News %s On %s ", EnumToString(signals.Trend),EnumToString(signal.type), EnumToString(OsMAIndicator));
         return true;
      }  
   
   } else {
   */
      if (GET(AllowSELL) //&& ((signals.Trend == DOWN) || (signals.Trend == LATERAL))
            //&& (priceHigh >= bandUpperMax)
            && (osmaTrend == DOWN)
            //&& osMASELL
             )
      {
         RaiseMarketSignal(-1, "SELL On OsMa");
         return ;
      }

      if ( GET(AllowBUY) //&& ((signals.Trend == UPPER) || (signals.Trend == LATERAL)) 
            //&& (priceLow <= bandLowMin)
            //&& (mfiTrend == UPPER)
            && (osmaTrend == UPPER)
            //&& osMABUY
            )
      {
         RaiseMarketSignal(1, "BUY On OsMa");
         return;
      }         
      
}

