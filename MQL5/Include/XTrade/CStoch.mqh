#property library
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <XTrade\IndiBase.mqh>
#include <XTrade\Orders.mqh>
#include <XTrade\InputTypes.mqh>
#include <XTrade\SSA\CSSAParamSet.mqh>

class CStoch : public IndiBase
{
protected:
   CSSAStochParamSet Params;
public:
   CStoch();
   ~CStoch(void);
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
CStoch::CStoch() 
{
}

bool CStoch::Init(ENUM_TIMEFRAMES timeframe)
{
   MqlParam params[];   

   if (GET(EnableRenko))
   {
      m_name = "MedianRenko/MedianRenko_Stochastic";
      ArrayResize(params, 4);
      params[0].type = TYPE_STRING;
      params[0].string_value = m_name;
      params[1].type = TYPE_INT;
      params[1].integer_value = 14;
      params[2].type = TYPE_INT;
      params[2].integer_value = 3;
      params[3].type = TYPE_INT;
      params[3].integer_value = 3;

   } else {
      m_name = "Market/SSA Stochastic";
      /*Params.InpKPeriod;
      Params.InpDPeriod;
      Params.InpSlowing;
      Params.InpLevels;     
      Params.SSA_OPTIONS;
      Params.ForecastMethod;
      Params.SegmentLength;
      Params.SW;
      Params.FastNoiseLevelK;
      Params.FastNoiseLevelD;
      Params.DataConvertMethod;
      Params.ForecastUpdateON;
      Params.RefreshPeriod;
      Params.ForecastPoints;
      Params.BackwardShift;
     
      Params.INTERFACE;
          // MagicNumber
      Params.VISUAL_OPTIONS;
      Params.NormalColorK;
      Params.PredictColorK;
      Params.NormalColorD;
      Params.PredictColorD;
      Params.LengthWithPrediction;
      Params.ENUM_CONVERSION_LCL;  
      Params.FrcstConvertMethod;
      */
      
      ArrayResize(params, 2);
      params[0].type = TYPE_STRING;
      params[0].string_value = m_name;
      params[1].type = TYPE_INT;
      params[1].integer_value = 14;

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

void CStoch::Delete()
{
    if (Handle() != INVALID_HANDLE)
    {
        DeleteFromChart(Utils.Trade().ChartId(), Utils.Trade().IndiSubWindow());
    }
}

//+------------------------------------------------------------------+
//| Destructor                                                       |
//+------------------------------------------------------------------+
CStoch::~CStoch(void)
{
   Delete();
}

double CStoch::GetData(const int buffer_num,const int index) const
{   
   double Buff[1];
   CopyBuffer(m_handle, buffer_num, index, 1, Buff); 
   return Buff[0];
}

void CStoch::Process()
{
}

