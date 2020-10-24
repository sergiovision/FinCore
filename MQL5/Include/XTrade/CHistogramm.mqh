#property library
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <XTrade\InputTypes.mqh>
#include <XTrade\IndiBase.mqh>
#include <XTrade\Orders.mqh>

class CHistogramm : public IndiBase
{
public:
   CHistogramm();
   ~CHistogramm();
   virtual bool Init(ENUM_TIMEFRAMES timeframe);
   virtual void Process();
   virtual void Trail(Order &order, int indent);
   virtual void Delete();
   void         SetGlobaVariables();
   virtual double GetData(const int buffer_num,const int index) const;
   virtual int  Type(void) const { return(IND_CUSTOM); }
   virtual bool      Initialize(const string symbol,const ENUM_TIMEFRAMES period,
                                const int num_params,const MqlParam &params[]);
};

CHistogramm::CHistogramm() 
{
   m_name = "LevelsHistogram";  
}

//+------------------------------------------------------------------+
//| Constructor                                                      |
//+------------------------------------------------------------------+
bool CHistogramm::Init(ENUM_TIMEFRAMES timeframe)
{
   if (Initialized())
      return true;      
   m_period = timeframe;
   
   int DayTheHistogram   = 10;          // Days The Histogram
   int DaysForCalculation= 365;          // Days for calculation(-1 all)
   int RangePercent      = 70;          // Range%
   
   SetSymbolPeriod(Utils.Symbol, m_period);
   MqlParam params[];   
   ArrayResize(params, 5);
   params[0].type = TYPE_STRING;
   params[0].string_value = m_name;
   params[1].type = TYPE_INT;
   params[1].integer_value = DayTheHistogram;
   params[2].type = TYPE_INT;
   params[2].integer_value = DaysForCalculation;
   params[3].type = TYPE_INT;
   params[3].integer_value = RangePercent;
   params[4].type = TYPE_BOOL;
   params[4].integer_value = 0;
   
   m_bInited = Create(Utils.Symbol, (ENUM_TIMEFRAMES)m_period, IND_CUSTOM, 5, params);
   if (m_bInited)
   {
      FullRelease(!Utils.IsTesting());
      AddToChart(Utils.Trade().ChartId(), Utils.Trade().SubWindow());
      return true;

   }
   Utils.Info(StringFormat("Indicator %s - failed to load!!!!!", m_name));
   return m_bInited;
}

void CHistogramm::Delete()
{
    if (Handle() != INVALID_HANDLE)
    {
        DeleteFromChart(Utils.Trade().ChartId(), Utils.Trade().IndiSubWindow());
    }
}

bool CHistogramm::Initialize(const string symbol,const ENUM_TIMEFRAMES period, const int num_params,const MqlParam &params[]) 
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
CHistogramm::~CHistogramm(void)
{
   Delete();
}

double CHistogramm::GetData(const int buffer_num,const int index) const
{   
   double Buff[1];
   
   int res = CopyBuffer(m_handle, buffer_num, index, 1, Buff); 
   if (res > 0)
      return Buff[0];
   else 
      return 0;
}


void CHistogramm::Process()
{
   double Value = GetData(0, 0);          
}


void CHistogramm::Trail(Order &order, int indent)
{      
   if (!m_bInited)
     return;
}
