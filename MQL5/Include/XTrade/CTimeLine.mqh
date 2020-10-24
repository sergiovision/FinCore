#property library
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <XTrade\InputTypes.mqh>
#include <XTrade\IndiBase.mqh>
#include <XTrade\Signals.mqh>
//#include <XTrade\TradeSignals.mqh>
#include <XTrade\GenericTypes.mqh>

class CTimeLine : public IndiBase
{
  
public:
   int SubWindow;
   CTimeLine();
   ~CTimeLine();
   bool Init(ENUM_TIMEFRAMES timeframe);
   void Process();
   virtual void Delete();
   virtual int       Type(void) const { return(0); }
   // bool OpenNewsSTOPOrders(Signal& signal);
};
//+------------------------------------------------------------------+
//| Constructor                                                      |
//+------------------------------------------------------------------+
CTimeLine::CTimeLine() 
{
   m_bInited = false;
   m_name = "TimeLine";   
   SubWindow = 1;
}

bool CTimeLine::Init(ENUM_TIMEFRAMES timeframe)
{
   if (Initialized())
      return true;

   SetSymbolPeriod(Utils.Symbol, timeframe);
   //signals.NewsMinsRemained = INT_MIN;
   
   int                  InpFontSize  = 9;           // Font size
   int                  InpSpacing = 8;             // Date/Time spacing

   MqlParam params[8];
   params[0].type = TYPE_STRING;
   params[0].string_value = m_name;
   params[1].type = TYPE_INT;
   params[1].integer_value = Utils.Service().MagicNumber();
   params[2].type = TYPE_INT;
   params[2].integer_value = ThriftPORT;
   params[3].type = TYPE_INT;
   params[3].integer_value = SubWindow;  // SubWindow
   params[4].type = TYPE_INT;
   params[4].integer_value = GET(InfoTextColor); //InpTextColor
   params[5].type = TYPE_INT;
   params[5].integer_value = InpFontSize;
   params[6].type = TYPE_INT;
   params[6].integer_value = InpSpacing;
   //params[7].type = TYPE_INT;
   //if (GET(EnableRSI) || GET(EnableStochastic))
   //   params[7].integer_value = 80;
   //else 
   //   params[7].integer_value = 28;
   params[7].type = TYPE_INT;
   bool isUsedByIndicatorOnRenkoChart = true;//!Utils.IsTesting();   
   params[7].integer_value = isUsedByIndicatorOnRenkoChart; // Use on Charts = should be true = 1
   
   m_bInited = Create(Utils.Symbol, (ENUM_TIMEFRAMES)m_period, IND_CUSTOM, ArraySize(params), params);
   if (m_bInited)
   {
      AddToChart(Utils.Trade().ChartId(), Utils.Trade().IndiSubWindow());
      FullRelease(true);
   }
   return m_bInited;
}

void CTimeLine::Delete()
{
   if (Handle() != INVALID_HANDLE)
   {
     DeleteFromChart(Utils.Trade().ChartId(), Utils.Trade().IndiSubWindow());
   }
}

//+------------------------------------------------------------------+
//| Destructor                                                       |
//+------------------------------------------------------------------+
CTimeLine::~CTimeLine(void)
{
}

//--------------------------------------------------------------------
void CTimeLine::Process()
{   
   //datetime curtime = TimeCurrent();
   //return false;
}
