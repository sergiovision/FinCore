#property library
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <XTrade\InputTypes.mqh>
#include <XTrade\IndiBase.mqh>
#include <XTrade\Orders.mqh>
#include <XTrade\SmoothAlgorithms.mqh>

class CBBands : public IndiBase
{
protected:
   ENUM_MA_METHOD MA_Method;
   Applied_price_ IPC;
   
public:
   CBBands();
   ~CBBands();
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
CBBands::CBBands() 
{
   m_name = "BBands";  
}

bool CBBands::Init(ENUM_TIMEFRAMES timeframe)
{
   if (Initialized())
      return true;      
   m_period = timeframe;
   
   if (GET(EnableRenko))
      return true;

   IPC = PRICE_CLOSE_;
   MA_Method = MODE_SMA;
  
   SetSymbolPeriod(Utils.Symbol, m_period);
   MqlParam params[];   
   ArrayResize(params,5);
   params[0].type = TYPE_STRING;
   params[0].string_value = m_name;
   params[1].type = TYPE_INT;
   params[1].integer_value = GET(BandsPeriod);
   params[2].type = TYPE_DOUBLE;
   params[2].double_value = GET(BandsDeviation);
   params[3].type = TYPE_INT;
   params[3].integer_value = MA_Method;
   params[4].type = TYPE_INT;
   params[4].integer_value = IPC;
      
   m_bInited = Create(Utils.Symbol, (ENUM_TIMEFRAMES)m_period, IND_CUSTOM, 5, params);
   if (m_bInited)
   {
      FullRelease(!Utils.IsTesting());
      AddToChart(Utils.Trade().ChartId(), Utils.Trade().SubWindow());
      return true;
   }
   Utils.Info(StringFormat("Indicator %s - failed to load!!!!!!!!!!!!!", m_name));
   return m_bInited;
}

void CBBands::Delete()
{
    if (Handle() != INVALID_HANDLE)
    {
        DeleteFromChart(Utils.Trade().ChartId(), Utils.Trade().SubWindow());
    }
}

bool CBBands::Initialize(const string symbol,const ENUM_TIMEFRAMES period, const int num_params,const MqlParam &params[]) 
{
   if(CreateBuffers(symbol,period,3))
   {
      ((CIndicatorBuffer*)At(0)).Name("Upper");
      ((CIndicatorBuffer*)At(1)).Name("Middle");
      ((CIndicatorBuffer*)At(2)).Name("Lower");
      return(true);
   }
   //--- error
   return(false);
}

//+------------------------------------------------------------------+
//| Destructor                                                       |
//+------------------------------------------------------------------+
CBBands::~CBBands(void)
{
   Delete();
}

double CBBands::GetData(const int buffer_num,const int index) const
{   
#ifdef __MQL4__   
   double val = iCustom(NULL
      ,m_period
      ,m_name
      ,BandsPeriod
      ,BandsDeviation
      ,MA_Method
      ,IPC
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

void CBBands::Process()
{
   double Value = GetData(0, 0);
}

void CBBands::Trail(Order &order, int indent)
{      
   if (!m_bInited)
     return;   
}


