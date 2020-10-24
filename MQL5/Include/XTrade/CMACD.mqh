#property library
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <XTrade\IndiBase.mqh>
#include <XTrade\Orders.mqh>
#include <XTrade\InputTypes.mqh>

class CMACD : public IndiBase
{
protected:
   int fast_ema_period;
   int slow_ema_period;
   int signal_period;
public:
   CMACD();
   ~CMACD(void);
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
CMACD::CMACD() 
{
   m_name = "MedianRenko/MedianRenko_MACD";
}

bool CMACD::Init(ENUM_TIMEFRAMES timeframe)
{
   if (Initialized() || CheckIndicatorExist(m_name))
      return true;
   m_period = timeframe;
   SetSymbolPeriod(Utils.Symbol, m_period);
   MqlParam params[5];   
   params[0].type = TYPE_STRING;
   params[0].string_value = m_name;
   params[1].type = TYPE_INT;
   params[1].integer_value = 9;
   params[2].type = TYPE_INT;
   params[2].integer_value = 18;
   params[3].type = TYPE_INT;
   params[3].integer_value = 9;
   params[4].type = TYPE_INT;
   params[4].integer_value = true;
   
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

void CMACD::Delete()
{
    if (Handle() != INVALID_HANDLE)
    {
        DeleteFromChart(Utils.Trade().ChartId(), Utils.Trade().IndiSubWindow());
    }
}

//+------------------------------------------------------------------+
//| Destructor                                                       |
//+------------------------------------------------------------------+
CMACD::~CMACD(void)
{
   Delete();
}

double CMACD::GetData(const int buffer_num,const int index) const
{   
   double Buff[1];
   CopyBuffer(m_handle, buffer_num, index, 1, Buff); 
   return Buff[0];
}

void CMACD::Process()
{
}

