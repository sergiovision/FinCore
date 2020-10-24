#property library
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <XTrade\InputTypes.mqh>
#include <XTrade\IndiBase.mqh>
#include <XTrade\SmoothAlgorithms.mqh>

class CFChannel : public IndiBase
{
protected:
   
public:
   CFChannel();
   ~CFChannel();
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
CFChannel::CFChannel() 
{
   m_name = "Fractal_Channel";  
}

bool CFChannel::Init(ENUM_TIMEFRAMES timeframe)
{
   if (Initialized())
      return true;      
   m_period = timeframe;
     
   SetSymbolPeriod(Utils.Symbol, m_period);
   MqlParam params[];   
   ArrayResize(params,2);
   params[0].type = TYPE_STRING;
   params[0].string_value = m_name;
   params[1].type = TYPE_INT;
   params[1].integer_value = 4; //InpFrames
      
   m_bInited = Create(Utils.Symbol, (ENUM_TIMEFRAMES)m_period, IND_CUSTOM, 2, params);
   if (m_bInited)
   {
      FullRelease(!Utils.IsTesting());
      AddToChart(Utils.Trade().ChartId(), Utils.Trade().SubWindow());
      return true;
   }
   Utils.Info(StringFormat("Indicator %s - failed to load!!!!!!!!!!!!!", m_name));
   return m_bInited;
}

void CFChannel::Delete()
{
    if (Handle() != INVALID_HANDLE)
    {
        DeleteFromChart(Utils.Trade().ChartId(), Utils.Trade().SubWindow());
    }
}

bool CFChannel::Initialize(const string symbol,const ENUM_TIMEFRAMES period, const int num_params,const MqlParam &params[]) 
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
CFChannel::~CFChannel(void)
{
   Delete();
}

double CFChannel::GetData(const int buffer_num,const int index) const
{   
   //return CIndicator::GetData(buffer_num, index);   
   double Buff[1];
   //ArrayResize(Buff, 1);
   //ArraySetAsSeries(Buff, true);
   int res = CopyBuffer(m_handle, buffer_num, index, 1, Buff); 
   if (res > 0)
      return Buff[0];
   else 
      return 0;
}

void CFChannel::Process()
{
   double Value = GetData(0, 0);
}

void CFChannel::Trail(Order &order, int indent)
{      
   if (!m_bInited)
     return;   
}


