#property library
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <XTrade\IndiBase.mqh>
#include <XTrade\InputTypes.mqh>

class CCandle : public IndiBase
{
protected:
   string prevTime;
public:
   CCandle();
   ~CCandle(void);
   virtual bool Init(ENUM_TIMEFRAMES timeframe);
   virtual void Process();
   virtual void Trail(Order &order, int indent) {}
   virtual void Delete();
   virtual double    GetData(const int buffer_num,const int index) const;
   bool              Initialize();
   virtual int       Type(void) const { return(IND_CUSTOM); }
   void CleanGlobalVars();
};
//+------------------------------------------------------------------+
//| Constructor                                                      |
//+------------------------------------------------------------------+
CCandle::CCandle() 
{
   m_name = "CandlePatterns";
}

void CCandle::CleanGlobalVars()
{
    GlobalVariablesDeleteAll("CandleSignal");
    string strMagic = IntegerToString(Utils.Service().MagicNumber());
    //signals.thrift.InitNewsVariables(strMagic);
}   

bool CCandle::Init(ENUM_TIMEFRAMES timeframe)
{
   if (Initialized())
      return true;
   CleanGlobalVars();
   SetSymbolPeriod(Utils.Symbol,timeframe);   
   MqlParam params[];   
   ArrayResize(params,3);
   params[0].type = TYPE_STRING;
   params[0].string_value = m_name;
   params[1].type = TYPE_INT;
   params[1].integer_value = GET(NumBarsToAnalyze);
   params[2].type = TYPE_INT;
   params[2].integer_value = Utils.Service().MagicNumber();
   m_bInited = Create(Utils.Symbol, (ENUM_TIMEFRAMES)m_period, IND_CUSTOM, 3, params);
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

void CCandle::Delete()
{
    if (Handle() != INVALID_HANDLE)
    {
        DeleteFromChart(Utils.Trade().ChartId(), Utils.Trade().SubWindow());
    }
}
      
//+------------------------------------------------------------------+
//| Destructor                                                       |
//+------------------------------------------------------------------+
CCandle::~CCandle(void)
{
   Delete();
}

//+------------------------------------------------------------------+
//| Initialize indicator with the special parameters                 |
//+------------------------------------------------------------------+
bool CCandle::Initialize()
{
#ifdef  __MQL5__
   if(CreateBuffers(m_symbol,m_period,5))
   {
      ((CIndicatorBuffer*)At(4)).Name("CANDLE");
      return(true);
   }
   //--- error
   return(false);
#else 
   return(true);
#endif   
}


double CCandle::GetData(const int buffer_num,const int index) const
{   
#ifdef __MQL4__
   double val = iCustom(NULL,m_period, m_name, buffer_num, index);
   Utils.Info(StringFormat("CandlePatterns BufIndex=%d, index=%d, val=%g", buffer_num, index, val));
   return val;
#else    
   double Buff[2];
   CopyBuffer(m_handle, buffer_num, index, 1, Buff); 
   return Buff[0];
#endif   
}

void CCandle::Process()
{
    /*
    double result[];
    double resultTime[];
    int array_size = 4;
    //ArrayResize(result, array_size);
    //ArrayResize(resultTime, array_size);
    ArraySetAsSeries(result,false);
    ArraySetAsSeries(resultTime,false);
    int i = 0; 
    CopyBuffer(IndiHandle, 0, 0, array_size, result);
    CopyBuffer(IndiHandle, 1, 0, array_size, resultTime);
    datetime signalTime = (datetime)resultTime[i];
    double res = result[0];
    */

      string strMagic = IntegerToString(Utils.Service().MagicNumber());
      datetime time = (datetime) GlobalVariableGet("CandleSignalTime" + strMagic);
      string SignalTime = TimeToString(time);
      if (StringCompare(prevTime, SignalTime) == 0)
          return ; // Signal handled;
      datetime currentTime = Utils.CurrentTimeOnTF();
      //string CurrentTime = TimeToString(currentTime);
      int PeriodsPassed = (int)(currentTime-time)/PeriodSeconds();
      if ( PeriodsPassed > 1)
         return ; // Signal too old;
      
      //signal.Init(signal.UseAsFilter);
      //signal.type = SignalQuiet;

      double res = GlobalVariableGet("CandleSignal" + strMagic);
      
      //if (MathAbs(res) < 2)
      //   return false;
                  
      for (int i = 0; i < GlobalVariablesTotal() ; i++)
      {
         string name = GlobalVariableName(i);
         if (StringFind(name, "CandleSignalName" + strMagic) >= 0) 
         {
            int startPos = StringFind(name, "|");
            if (startPos > 0)
            {
               string strName = StringSubstr(name, startPos + 1);
               //signal.SetName(strName);
            }   
            break;
         }
      }
      
       if (StringCompare(SignalTime, prevTime) != 0)
          prevTime = SignalTime;
       
       //if (MathAbs(res) == 1.5)             
       //else signal.Value = 0;
       //signals.StatusString += StringFormat("/ Candle %s", EnumToString(signal.type)); 
      
        //    return false;
       //for (int i = 0; i <1000; i++)
       //{
          //if (result[i] != 0)
          //{
            //Print(StringFormat("Bufffer %d = %g", i, result[i]));                  
          //}
       //}
}
