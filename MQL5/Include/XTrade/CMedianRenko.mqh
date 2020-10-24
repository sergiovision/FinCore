#property library
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <XTrade\IndiBase.mqh>
#include <XTrade\Orders.mqh>
#include <XTrade\SmoothAlgorithms.mqh>
#include <XTrade\InputTypes.mqh>


//
// SHOW_INDICATOR_INPUTS *NEEDS* to be defined, if the EA needs to be *tested in MT5's backtester*
// -------------------------------------------------------------------------------------------------
// Using '#define SHOW_INDICATOR_INPUTS' will show the MedianRenko indicator's inputs 
// NOT using the '#define SHOW_INDICATOR_INPUTS' statement will read the settigns a chart with 
// the MedianRenko indicator attached.
//
//#ifndef  SHOW_INDICATOR_INPUTS
//#define SHOW_INDICATOR_INPUTS
//#endif

#define RENKO_INDICATOR_NAME_SHORT "Median and Turbo renko indicator bundle" 

//
// You need to include the MedianRenko.mqh header file
//

#include <XTrade/MedianRenko/MedianRenko.mqh>
#include <XTrade/MedianRenko/RenkoPatterns.mqh>
//
//  To use the MedainRenko indicator in your EA you need do instantiate the indicator class (MedianRenko)
//  and call the Init() method in your EA's OnInit() function.
//  Don't forget to release the indicator when you're done by calling the Deinit() method.
//  Example shown in OnInit & OnDeinit functions below:

//#define CURRENT_UNCOMPLETED_BAR  0
//#define LAST_COMPLETED_BAR       1

class CMedianRenko : public IndiBase
{
protected:
   MedianRenko *medianRenko;
   string chartIndicatorName;
   int openBars;
   int closeBars;
public:
   CMedianRenko();
   ~CMedianRenko();
   MedianRenko* GetMR() { return medianRenko; }
   virtual bool Init(ENUM_TIMEFRAMES timeframe);
   virtual void Process();
   virtual void Trail(Order &order, int indent);
   virtual void Delete();
   virtual int  Type(void) const { return(IND_CUSTOM); }
   virtual bool      Initialize(const string symbol,const ENUM_TIMEFRAMES period,
                                const int num_params,const MqlParam &params[]);
   bool AddToChart(const long chart,const int subwin);                               
   void GetRenkoInfo(int offset, int &_iteration);
   bool IsNewBar();
   MqlRates   RenkoRatesInfoArray[];
   bool GetMqlRates(MqlRates &ratesInfoArray[], int start, int count);
};

//+------------------------------------------------------------------+
//| Constructor                                                      |
//+------------------------------------------------------------------+
CMedianRenko::CMedianRenko()
{
   m_name = CUSTOM_CHART_NAME;  
   openBars = 1;
   closeBars = 1;
   bAlreadyExist = false;
   chartIndicatorName = RENKO_INDICATOR_NAME_SHORT; 
}

bool CMedianRenko::GetMqlRates(MqlRates &ratesInfoArray[], int start, int count) 
{
   return this.medianRenko.GetMqlRates(ratesInfoArray, start, count);
}

bool CMedianRenko::AddToChart(const long chart,const int subwin)
{
   if (bAlreadyExist)
      return true;
   //Sleep(500);
   int handle  = medianRenko.GetHandle();
   if(ChartIndicatorAdd(chart,subwin, handle))
   {
     int totalindicators = ChartIndicatorsTotal(chart,subwin);
     int index = totalindicators - 1;
     m_name = ChartIndicatorName(chart,subwin, index);
     chartIndicatorName = m_name;
     return(true);
   }
   //--- failed
   return(false);
}

bool CMedianRenko::Init(ENUM_TIMEFRAMES timeframe)
{
   if (Initialized())
      return true;
   m_period = timeframe;
   
   bool isUsedByIndicatorOnRenkoChart = CheckIndicatorExist(CUSTOM_CHART_NAME); 

   medianRenko = new MedianRenko(isUsedByIndicatorOnRenkoChart);
   if (medianRenko == NULL)
      return false;
   if (medianRenko.Init() == INVALID_HANDLE)
   {
      Utils.Info("MedianRenko failed to initialize");
      m_bInited = false;
      return false;
   }
   m_bInited = true;
   if (AddToChart(Utils.Trade().ChartId(), Utils.Trade().SubWindow()))
   {
      Print(StringFormat("%s Added to Chart successfully", m_name));
   }
   return m_bInited;
}

void CMedianRenko::Delete()
{
     m_name = chartIndicatorName; //RENKO_INDICATOR_NAME;//RENKO_INDICATOR_NAME_SHORT;
     if (!ChartIndicatorDelete(Utils.Trade().ChartId(), Utils.Trade().SubWindow(), m_name))
     {
         Print( StringFormat("Failed to delete indicator <%s> from chart", m_name) );
         // m_name = chartIndicatorName;
         //if (!DeleteFromChart(Utils.Trade().ChartId(), Utils.Trade().SubWindow()))
         //{
         //  ResetLastError();
         //  if (!ChartIndicatorDelete(Utils.Trade().ChartId(), Utils.Trade().SubWindow(), m_name))
         //     Utils.Info(StringFormat("Failed to release indicator from chart <%s> error: %d", m_name, GetLastError()));
         //}
     }
     if (medianRenko != NULL)
     {
        medianRenko.Deinit();
        DELETE_PTR(medianRenko);
     }
    m_bInited = false;
}

bool CMedianRenko::Initialize(const string symbol,const ENUM_TIMEFRAMES period, const int num_params,const MqlParam &params[]) 
{
   return(true);
}

//+------------------------------------------------------------------+
//| Destructor                                                       |
//+------------------------------------------------------------------+
CMedianRenko::~CMedianRenko(void)
{
   // Delete();
}

void CMedianRenko::Process()
{
   //double Value = GetData(0, 0);
}

void CMedianRenko::Trail(Order &order, int indent)
{      
   if (!m_bInited)
     return;   
}

bool CMedianRenko::IsNewBar()
{
   if (this.medianRenko == NULL)
       return false;
   return this.medianRenko.IsNewBar();
}

void CMedianRenko::GetRenkoInfo(int offset, int &_iteration)
{
   int _startAtBar = offset; // first bar to get
   int _numberOfBars = (int)MathMax(openBars,closeBars) + 2;
      
   if(!medianRenko.GetMqlRates(this.RenkoRatesInfoArray,_startAtBar,_numberOfBars))
   {
      Print(__FUNCTION__," failed on GetMqlRates");
      return;
   }
   
   //
   // filter out filler bars 
   //
   
   _iteration++;
   int _fillerCount = 0;
   for (int i = LAST_COMPLETED_BAR; i<_numberOfBars; i++)
   {
      if((this.RenkoRatesInfoArray[i].real_volume == 0) &&
         (this.RenkoRatesInfoArray[i].tick_volume == 0))
            _fillerCount++;
   }
   
   if(_fillerCount > 0)
   {
      GetRenkoInfo((offset + _fillerCount),_iteration);
      return;
   }
   
   //
   
}

