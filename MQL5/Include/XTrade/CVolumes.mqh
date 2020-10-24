#property library
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <XTrade\IndiBase.mqh>
#include <XTrade\Orders.mqh>
#include <XTrade\InputTypes.mqh>

class CVolumes : public IndiBase
{
protected:
public:
   CVolumes();
   ~CVolumes(void);
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
CVolumes::CVolumes() 
{
}

bool CVolumes::Init(ENUM_TIMEFRAMES timeframe)
{
   MqlParam params[];   
   if (GET(EnableRenko))
   {
      m_name = "MedianRenko/MedianRenko_volumes";
      ArrayResize(params, 3);
      params[0].type = TYPE_STRING;
      params[0].string_value = m_name;
      params[1].type = TYPE_INT;
      params[1].integer_value = 7;
      params[2].type = TYPE_INT;
      params[2].integer_value = PRICE_MEDIAN;
   } else {
      m_name = "AverageVolumes";
      ArrayResize(params, 3);
      params[0].type = TYPE_STRING;
      params[0].string_value = m_name;
      params[1].type = TYPE_INT;
      params[1].integer_value = 4;
      params[2].type = TYPE_INT;
      params[2].integer_value = VOLUME_TICK;

   }
   if (Initialized() || CheckIndicatorExist(m_name))
      return true;
   m_period = timeframe;
   SetSymbolPeriod(Utils.Symbol, m_period);

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

void CVolumes::Delete()
{
    if (Handle() != INVALID_HANDLE)
    {
        DeleteFromChart(Utils.Trade().ChartId(), Utils.Trade().IndiSubWindow());
    }
}

//+------------------------------------------------------------------+
//| Destructor                                                       |
//+------------------------------------------------------------------+
CVolumes::~CVolumes(void)
{
   Delete();
}

double CVolumes::GetData(const int buffer_num,const int index) const
{   
   double Buff[1];
   CopyBuffer(m_handle, buffer_num, index, 1, Buff); 
   return Buff[0];
}

void CVolumes::Process()
{
}

