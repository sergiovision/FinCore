#property library
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <XTrade\InputTypes.mqh>
#include <XTrade\IndiBase.mqh>
#include <XTrade\Orders.mqh>

class CNATR : public IndiBase
{
protected:
   int InpAtrPeriod; 
   int InpAtrPercent;   
   bool PercentileATR;

public:
   CNATR();
   ~CNATR();
   virtual bool Init(ENUM_TIMEFRAMES timeframe);
   virtual void Process();
   virtual void Trail(Order &order, int indent);
   virtual void Delete();
   virtual double GetData(const int buffer_num,const int index) const;
   virtual int  Type(void) const { return(IND_CUSTOM); }
    virtual bool      Initialize(const string symbol,const ENUM_TIMEFRAMES period,
                                const int num_params,const MqlParam &params[]); 
};

//+------------------------------------------------------------------+
//| Constructor                                                      |
//+------------------------------------------------------------------+
CNATR::CNATR() 
{
   m_name = "NATR";  
}

bool CNATR::Init(ENUM_TIMEFRAMES timeframe)
{
   if (Initialized())
      return true;      
   m_period = timeframe;
   InpAtrPeriod = (int)GET(NumBarsToAnalyze); 
   InpAtrPercent = int((1.0 - SL_PERCENTILE)*100.0);//TMAPercentileRank;   
   PercentileATR = false;
  
   SetSymbolPeriod(Utils.Symbol, m_period);
   MqlParam params[];   
   ArrayResize(params,4);
   params[0].type = TYPE_STRING;
   params[0].string_value = m_name;
   params[1].type = TYPE_INT;
   params[1].integer_value = InpAtrPeriod;
   params[2].type = TYPE_INT;
   params[2].integer_value = InpAtrPercent;
   params[3].type = TYPE_INT;
   params[3].integer_value = PercentileATR;
      
   m_bInited = Create(Utils.Symbol, (ENUM_TIMEFRAMES)m_period, IND_CUSTOM, 4, params);
   if (m_bInited)
   {
      FullRelease(!Utils.IsTesting());
      AddToChart(Utils.Trade().ChartId(), Utils.Trade().IndiSubWindow());
      return true;

   }
   Utils.Info(StringFormat("Indicator %s - failed to load!!!!!!!!!!!!!", m_name));
   return m_bInited;
}

void CNATR::Delete()
{
#ifdef __MQL5__
    if (Handle() != INVALID_HANDLE)
    {
        DeleteFromChart(Utils.Trade().ChartId(), Utils.Trade().IndiSubWindow());
    }
#endif  
}

bool CNATR::Initialize(const string symbol,const ENUM_TIMEFRAMES period, const int num_params,const MqlParam &params[]) 
{
#ifdef  __MQL5__
   if(CreateBuffers(symbol,period,2))
   {
      //--- create buffers
      ((CIndicatorBuffer*)At(0)).Name("NATR");
      //((CIndicatorBuffer*)At(1)).Name("TR");
      //--- ok
      return(true);
   }
   //--- error
   return(false);
#else 
   return(true);
#endif   
}


//+------------------------------------------------------------------+
//| Destructor                                                       |
//+------------------------------------------------------------------+
CNATR::~CNATR(void)
{
   Delete();
}

double CNATR::GetData(const int buffer_num,const int index) const
{   
#ifdef __MQL4__   
   double val = iCustom(NULL
      ,m_period
      ,m_name
      ,m_period
      ,tma_period
      ,atr_multiplier
      ,atr_period
      ,TrendThreshold
      ,PercentileATR
      ,PercentileRank
      ,buffer_num,index);
   //Utils.Info(StringFormat("OsMA BufIndex=%d, index=%d, val=%g", buffer_num, index, val));
   return val;
#else   
   //return CIndicator::GetData(buffer_num, index);   
   double Buff[1];
   //ArrayResize(Buff, 1);
   //ArraySetAsSeries(Buff, true);
   
   int res = CopyBuffer(m_handle, buffer_num, index, 1, Buff); 
   if (res > 0)
      return Buff[0];
   else 
      return 0;
#endif    
}


void CNATR::Process()
{
   double Value = GetData(0, 0);          
}


void CNATR::Trail(Order &order, int indent)
{      
   if (!m_bInited)
     return;
   double atr = GetData(0, 0);          
   MqlRates rates[];
   ArrayResize(rates, 3);
   ArraySetAsSeries(rates, true);
   CopyRates(Utils.Symbol, (ENUM_TIMEFRAMES)m_period, 0, 3, rates);
   
}