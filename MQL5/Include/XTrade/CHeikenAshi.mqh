#property library
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <XTrade\IndiBase.mqh>

class CHeikenAshi : public IndiBase
{
protected:
public:
   CHeikenAshi();
   ~CHeikenAshi(void);
   virtual bool Init(ENUM_TIMEFRAMES timeframe);
   void         Process();
   virtual void Trail(Order &order, int indent);
   virtual void Delete();
   virtual double    GetData(const int buffer_num,const int index) const;
   bool              Initialize();
   virtual int       Type(void) const { return(IND_CUSTOM); }
};
//+------------------------------------------------------------------+
//| Constructor                                                      |
//+------------------------------------------------------------------+
CHeikenAshi::CHeikenAshi() 
{
   m_name = "Heiken_Ashi";
}

bool CHeikenAshi::Init(ENUM_TIMEFRAMES timeframe)
{
   if (Initialized())
      return true;
   SetSymbolPeriod(Utils.Symbol,timeframe);   
   MqlParam params[];   
   ArrayResize(params,1);
   params[0].type = TYPE_STRING;
   params[0].string_value = m_name;
   
   m_bInited = Create(Utils.Symbol, (ENUM_TIMEFRAMES)m_period, IND_CUSTOM, 1, params);
   if (m_bInited)
     m_bInited = Initialize();
   if (m_bInited)
   { 
      FullRelease(!Utils.IsTesting());
      AddToChart(Utils.Trade().ChartId(), Utils.Trade().SubWindow());
      return true;
   }
   Utils.Info(StringFormat("Indicator %s - failed to load!!!!!!!!!!!!!", m_name));
   return false;
}

void CHeikenAshi::Delete()
{
    if (Handle() != INVALID_HANDLE)
    {
        DeleteFromChart(Utils.Trade().ChartId(), Utils.Trade().SubWindow());
    }
}

      
//+------------------------------------------------------------------+
//| Destructor                                                       |
//+------------------------------------------------------------------+
CHeikenAshi::~CHeikenAshi(void)
{
   Delete();
}

//+------------------------------------------------------------------+
//| Initialize indicator with the special parameters                 |
//+------------------------------------------------------------------+
bool CHeikenAshi::Initialize()
{
   if(CreateBuffers(m_symbol,m_period,5))
   {
      ((CIndicatorBuffer*)At(4)).Name("COLOR");
      //((CIndicatorBuffer*)At(1)).Name("KIJUNSEN_LINE");
      return(true);
   }
   //--- error
   return(false);
}


double CHeikenAshi::GetData(const int buffer_num,const int index) const
{   
   double Buff[2];
   CopyBuffer(m_handle, buffer_num, index, 1, Buff); 
   return Buff[0];
}


void CHeikenAshi::Process()
{
   double Value = GetData(0, 0);
}

void CHeikenAshi::Trail(Order &order, int indent)
{      
   if (!m_bInited)
      return;   
}


