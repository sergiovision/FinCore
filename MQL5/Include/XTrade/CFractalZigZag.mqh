#property library
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <XTrade\IndiBase.mqh>
#include <XTrade\Orders.mqh>
class TradeSignals;

class CFractalZigZag : public IndiBase
{
protected:
   int  ZZDepth;
   int  ZZDev;
public:
   CFractalZigZag(TradeSignals* s);
   ~CFractalZigZag();
   virtual bool Init(ENUM_TIMEFRAMES timeframe);
   virtual bool Process(Signal& signal);
   virtual void Trail(Order &order, int indent) {}
   virtual void Delete();
   virtual double GetData(const int buffer_num,const int index) const;
   bool CFractalZigZag::Initialize(const string symbol,const ENUM_TIMEFRAMES period, const int num_params,const MqlParam &params[]); 

   
   virtual int       Type(void) const { return(IND_CUSTOM); }
};
//+------------------------------------------------------------------+
//| Constructor                                                      |
//+------------------------------------------------------------------+
CFractalZigZag::CFractalZigZag(TradeSignals* s) 
   :IndiBase(s)  
{
   m_name = "FractalZigZag";    
}

bool CFractalZigZag::Init(ENUM_TIMEFRAMES timeframe)
{
   if (Initialized())
      return true;
   m_period = timeframe;
   SetSymbolPeriod(signals.Symbol, m_period);
   
   ZZDepth = 12;
   ZZDev = 5;

   MqlParam params[];   
   ArrayResize(params,3);
   params[0].type = TYPE_STRING;
   params[0].string_value = m_name;
   params[1].type = TYPE_INT;
   params[1].integer_value = ZZDepth;
   params[2].type = TYPE_INT;
   params[2].integer_value = ZZDev;
   
   m_bInited = Create(signals.Symbol, (ENUM_TIMEFRAMES)m_period, IND_CUSTOM, 3, params);
   if (m_bInited)
   {
      FullRelease(!Utils.IsTesting());
      AddToChart(signals.chartID, signals.subWindow);
      return true;

   }
   Utils.Info(StringFormat("Indicator %s - failed to load!!!!!!!!!!!!!", m_name));
   return m_bInited;
}

bool CFractalZigZag::Initialize(const string symbol,const ENUM_TIMEFRAMES period, const int num_params,const MqlParam &params[]) 
{
#ifdef  __MQL5__
   if(CreateBuffers(symbol,period,2))
   {
      //--- create buffers
      ((CIndicatorBuffer*)At(0)).Name("Fractal Up");
      ((CIndicatorBuffer*)At(1)).Name("Fractal Down");
      //--- ok
      return(true);
   }
   //--- error
   return(false);
#else 
   return(true);
#endif   
}


void CFractalZigZag::Delete()
{
    if (Handle() != INVALID_HANDLE)
    {
        DeleteFromChart(signals.chartID, signals.indiSubWindow);
    }
}

//+------------------------------------------------------------------+
//| Destructor                                                       |
//+------------------------------------------------------------------+
CFractalZigZag::~CFractalZigZag(void)
{
   Delete();
}

double CFractalZigZag::GetData(const int buffer_num,const int index) const
{   
   double Buff[1];
   CopyBuffer(m_handle, buffer_num, index, 1, Buff); 
   return Buff[0];
}

bool CFractalZigZag::Process(Signal& signal)
{
   double TopMin = 0;
   double TopMax = 0;
   TYPE_TREND TopTrend = 0;
   Utils.GetIndicatorMinMax(this, TopMin, TopMax, TopTrend, 1, CANDLE_PATTERN_MAXBARS);

   double BottomMin = 0;
   double BottomMax = 0;
   TYPE_TREND BottomTrend = 0;
   Utils.GetIndicatorMinMax(this, BottomMin, BottomMax, BottomTrend, 0, CANDLE_PATTERN_MAXBARS);
         
   //Utils.Info(StringFormat("TopMin %d, TopMax %d", TopMin, TopMax));
   //MqlRates rates[];
   //ArrayResize(rates, 2);
   //ArraySetAsSeries(rates, true);    
   //CopyRates(signals.Symbol, (ENUM_TIMEFRAMES)m_period, 0, 2, rates);
      
   //double priceHigh = MathMax(rates[0].high, rates[1].high);
   //double priceLow = MathMin(rates[0].low, rates[1].low);

   if ( (TopMax > 0) )
   {
      signal.Init(false);
      signal.Value = -1; 
      signal.type = SignalSELL;
      //signals.StatusString = StringFormat("TREND(%s) %s On %s ", EnumToString(signals.Trend),EnumToString(signal.type), EnumToString(FractalZigZagIndicator));
      Utils.Info(StringFormat("TopMax %g", TopMax));
      return true;
   }         
   if ( (BottomMax > 0))
   {
      signal.Init(false);
      signal.Value = 1;
      signal.type = SignalBUY;
      //signals.StatusString = StringFormat("TREND(%s) %s On %s ", EnumToString(signals.Trend), EnumToString(signal.type), EnumToString(FractalZigZagIndicator));
      Utils.Info(StringFormat("BottomMax %g", BottomMax));
      return true;
   }
   return false;
}

