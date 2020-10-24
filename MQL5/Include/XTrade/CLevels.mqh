#property library
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <XTrade\InputTypes.mqh>
#include <XTrade\IndiBase.mqh>
#include <XTrade\Orders.mqh>

class CLevels : public IndiBase
{
public:
   string levels_string;
   CLevels();
   ~CLevels();
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

//+------------------------------------------------------------------+
//| Constructor                                                      |
//+------------------------------------------------------------------+
CLevels::CLevels() 
{
   m_name = "Levels";  
}

bool CLevels::Init(ENUM_TIMEFRAMES timeframe)
{
   if (Initialized())
      return true;      
   m_period = timeframe;
   //levels_string = (string)GET(Levels);
  
   SetSymbolPeriod(Utils.Symbol, m_period);
   MqlParam params[];   
   ArrayResize(params,2);
   params[0].type = TYPE_STRING;
   params[0].string_value = m_name;
   params[1].type = TYPE_STRING;
   params[1].string_value = levels_string;
   
   m_bInited = Create(Utils.Symbol, (ENUM_TIMEFRAMES)m_period, IND_CUSTOM, ArraySize(params), params);
   if (m_bInited)
   {
      //Utils.SetLevelsGlobaVariable(levels_string);
      FullRelease(!Utils.IsTesting());
      AddToChart(Utils.Trade().ChartId(), Utils.Trade().SubWindow());
      return true;

   }
   Utils.Info(StringFormat("Indicator %s - failed to load!!!!!!!!!!!!!", m_name));
   return m_bInited;
}


void CLevels::Delete()
{
    if (Handle() != INVALID_HANDLE)
    {
        DeleteFromChart(Utils.Trade().ChartId(), Utils.Trade().IndiSubWindow());
    }
}

bool CLevels::Initialize(const string symbol,const ENUM_TIMEFRAMES period, const int num_params,const MqlParam &params[]) 
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
CLevels::~CLevels(void)
{
   Delete();
}

double CLevels::GetData(const int buffer_num,const int index) const
{   
   double Buff[1];
   
   int res = CopyBuffer(m_handle, buffer_num, index, 1, Buff); 
   if (res > 0)
      return Buff[0];
   else 
      return 0;
}


void CLevels::Process()
{
   double Value = GetData(0, 0);          
}


void CLevels::Trail(Order &order, int indent)
{      
   if (!m_bInited)
     return;

}