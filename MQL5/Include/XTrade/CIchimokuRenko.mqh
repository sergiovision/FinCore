#property library
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <XTrade\InputTypes.mqh>
#include <XTrade\IndiBase.mqh>
#include <XTrade\Orders.mqh>
#include <XTrade\CIchimoku.mqh>
#include <XTrade\CMedianRenko.mqh>
#include <XTrade\MedianRenko\RenkoPatterns.mqh>

class CIchimokuRenko : public IndiBase
{
protected:
   int               m_tenkan_sen;
   int               m_kijun_sen;
   int               m_senkou_span_b;
   CMedianRenko      *medianRenko;
   CRenkoPatterns    renkoPatterns;
public:
   CIchimokuRenko();
   ~CIchimokuRenko(void);
   void SetMedianRenko(CMedianRenko *medianRenko);
   
   virtual bool Init(ENUM_TIMEFRAMES timeframe);
   virtual void Process();
   virtual void Trail(Order &order, int indent);
   virtual void Delete();
     
   int               TenkanSenPeriod(void)        const { return(m_tenkan_sen);    }
   int               KijunSenPeriod(void)         const { return(m_kijun_sen);     }
   int               SenkouSpanBPeriod(void)      const { return(m_senkou_span_b); }
   //virtual double    GetData(const int buffer_num,const int index) const;
   //virtual void      Refresh(const int flags=OBJ_ALL_PERIODS) { }
   double            TenkanSen(const int index) const;
   double            KijunSen(const int index) const;
   double            SenkouSpanA(const int index) const;
   double            SenkouSpanB(const int index) const;
   double            ChinkouSpan(const int index) const;
   bool              Initialize(const string symbol,const ENUM_TIMEFRAMES period,
                                const int tenkan_sen,const int kijun_sen,const int senkou_span_b);
//#endif  
   //--- method of identifying
   virtual int       Type(void) const { return(IND_ICHIMOKU); }
   bool OkToStartBacktest(void);

};
//+------------------------------------------------------------------+
//| Constructor                                                      |
//+------------------------------------------------------------------+
CIchimokuRenko::CIchimokuRenko()
   :m_tenkan_sen(-1)
   ,m_kijun_sen(-1)
   ,m_senkou_span_b(-1)
{
   m_name = "MedianRenko/MedianRenko_Ichimoku";
}

void CIchimokuRenko::SetMedianRenko(CMedianRenko *mRenko) {
   medianRenko = mRenko;   

}

bool CIchimokuRenko::Init(ENUM_TIMEFRAMES timeframe)
{
   if (Initialized())
      return true;
   string symbol = _Symbol;
   long chartID = Utils.Trade().ChartId();
   int subWin = Utils.Trade().SubWindow();
      
   SetSymbolPeriod(symbol, timeframe);
   m_tenkan_sen = (int)GET(IshimokuPeriod1);
   m_kijun_sen = (int)GET(IshimokuPeriod2);
   m_senkou_span_b = (int)GET(IshimokuPeriod3);

   MqlParam params[5];   
   params[0].type = TYPE_STRING;
   params[0].string_value = m_name;
   params[1].type = TYPE_INT;
   params[1].integer_value = m_tenkan_sen;
   params[2].type = TYPE_INT;
   params[2].integer_value = m_kijun_sen;
   params[3].type = TYPE_INT;
   params[3].integer_value = m_senkou_span_b;   
   params[4].type = TYPE_BOOL;
   bool isUsedByIndicatorOnRenkoChart = true;//!Utils.IsTesting();   
   params[4].integer_value = isUsedByIndicatorOnRenkoChart;

   m_bInited = Create(symbol, (ENUM_TIMEFRAMES)m_period, IND_CUSTOM, ArraySize(params), params);
   m_bInited = Initialize(symbol,(ENUM_TIMEFRAMES)m_period, m_tenkan_sen, m_kijun_sen, m_senkou_span_b);
   if (m_bInited)
   { 
      FullRelease(!Utils.IsTesting());
      AddToChart(chartID, subWin);
      return true;
   }
   Utils.Info(StringFormat("Indicator <%s> - failed to load!!", m_name));
   return false;
}

void CIchimokuRenko::Delete()
{
    if (Handle() != INVALID_HANDLE)
    {
        DeleteFromChart(Utils.Trade().ChartId(), Utils.Trade().SubWindow());
    }
}
      
//+------------------------------------------------------------------+
//| Destructor                                                       |
//+------------------------------------------------------------------+
CIchimokuRenko::~CIchimokuRenko(void)
{
   Delete();
}

//+------------------------------------------------------------------+
//| Initialize indicator with the special parameters                 |
//+------------------------------------------------------------------+
bool CIchimokuRenko::Initialize(const string symbol,const ENUM_TIMEFRAMES period,
                            const int tenkan_sen,const int kijun_sen,const int senkou_span_b)
{
   m_tenkan_sen   =tenkan_sen;
   m_kijun_sen    =kijun_sen;
   m_senkou_span_b=senkou_span_b;

   if(CreateBuffers(symbol,period,5))
   {
      //--- string of status of drawing
      //m_name  ="Ichimoku";
      m_status="("+symbol+","+PeriodDescription()+","+
               IntegerToString(tenkan_sen)+","+IntegerToString(kijun_sen)+","+
               IntegerToString(senkou_span_b)+") H="+IntegerToString(m_handle);
      //--- save settings
      //--- create buffers
      ((CIndicatorBuffer*)At(0)).Name("TENKANSEN_LINE");
      ((CIndicatorBuffer*)At(1)).Name("KIJUNSEN_LINE");
      ((CIndicatorBuffer*)At(2)).Name("SENKOUSPANA_LINE");
      ((CIndicatorBuffer*)At(2)).Offset(kijun_sen);
      ((CIndicatorBuffer*)At(3)).Name("SENKOUSPANB_LINE");
      ((CIndicatorBuffer*)At(3)).Offset(kijun_sen);
      ((CIndicatorBuffer*)At(4)).Name("CHIKOUSPAN_LINE");
      ((CIndicatorBuffer*)At(4)).Offset(-kijun_sen);
      //--- ok
      return(true);
   }
   //--- error
   return(false);  
   //return true;
}

//+------------------------------------------------------------------+
//| Access to TenkanSen buffer of "Ichimoku Kinko Hyo"               |
//+------------------------------------------------------------------+
double CIchimokuRenko::TenkanSen(const int index) const
{
   return(GetData(MODE_TENKANSEN,index));
}
//+------------------------------------------------------------------+
//| Access to KijunSen buffer of "Ichimoku Kinko Hyo"                |
//+------------------------------------------------------------------+
double CIchimokuRenko::KijunSen(const int index) const
{
   return(GetData(MODE_KIJUNSEN,index));
}
//+------------------------------------------------------------------+
//| Access to SenkouSpanA buffer of "Ichimoku Kinko Hyo"             |
//+------------------------------------------------------------------+
double CIchimokuRenko::SenkouSpanA(const int index) const
{
   return(GetData(MODE_SENKOUSPANA,index));
}
//+------------------------------------------------------------------+
//| Access to SenkouSpanB buffer of "Ichimoku Kinko Hyo"             |
//+------------------------------------------------------------------+
double CIchimokuRenko::SenkouSpanB(const int index) const
{
   return(GetData(MODE_SENKOUSPANB,index));
}
//+------------------------------------------------------------------+
//| Access to ChikouSpan buffer of "Ichimoku Kinko Hyo"              |
//+------------------------------------------------------------------+
double CIchimokuRenko::ChinkouSpan(const int index) const
{
   return(GetData(MODE_CHIKOUSPAN,index));
}

void CIchimokuRenko::Process()
{
    if (medianRenko == NULL)
        return;
        
    if (GET(SignalIndicator) != IchimokuRenkoIndicator) 
    {
        return;
    }
    // medianRenko.Process();    
    //if (!OkToStartBacktest())
    //    return;    
    if (!medianRenko.IsNewBar())
        return;    

    this.Refresh();
                
    int _iterations = 0;
    medianRenko.GetRenkoInfo(CURRENT_UNCOMPLETED_BAR, _iterations);
    
    double TRedValue = TenkanSen(0);
    double KBlueValue = KijunSen(0);
    double ABValue = SenkouSpanA(0);
    double BBValue = SenkouSpanB(0);
    
    if ((TRedValue <= 0) || !MathIsValidNumber(TRedValue))
       return;
    if ((KBlueValue <= 0) || !MathIsValidNumber(KBlueValue))
       return;

    double IshiMax = MathMax(ABValue, BBValue);
    double IshiMin = MathMin(ABValue, BBValue);
    double mediaPrice = (Utils.tick.ask + Utils.tick.bid)/2.0;
    double delta = (Utils.Spread() + Utils.StopLevel() + GET(TrailingIndent) )*Point();
    //signals.deltaCross = delta;
    
    if (Utils.HigherThanAll(KBlueValue, IshiMax, IshiMin))
        Utils.Trade().SetTrend(UPPER);
    else {
       if (Utils.LowerThanAll(KBlueValue, IshiMin, IshiMax))
           Utils.Trade().SetTrend(DOWN);
       else 
           Utils.Trade().SetTrend(LATERAL);
    }
   
    //double IchiMinFlat = 0;          
    //double IchiMaxFlat = 0;    
    //TYPE_TREND IchiMiniTrend = 0;     
    
    //Utils.GetIndicatorMinMax(this, IchiMinFlat, IchiMaxFlat, IchiMiniTrend, MODE_SENKOUSPANB, (int)GET(NumBarsFlatPeriod));
    //double differenceM = MathAbs(IchiMaxFlat - IchiMinFlat)/delta;
    //if ((differenceM <= 1.0) ) 
    //{
    //   Utils.Trade().SetTrend(LATERAL);
       //signals.StatusString = StringFormat("TREND(%s) ABdiff=%g", EnumToString(signals.Trend), differenceM);
    //} else 
    //{
       //signals.StatusString = StringFormat("TREND(%s) ABdiff=%g", EnumToString(signals.Trend),  differenceM);
    //}  

       // MqlRates rates[];
       // ArrayResize(rates, 2);
       // ArraySetAsSeries(rates, true);    
       // CopyRates(signals.Symbol, (ENUM_TIMEFRAMES)m_period, 0, 2, rates);
    //   double cloudDistance = 0;
    //   if (Utils.tick.bid > IshiMax)
    //       cloudDistance = MathAbs((Utils.tick.bid - IshiMax)/Point());
    //   if (Utils.tick.ask < IshiMin)
    //       cloudDistance = MathAbs((IshiMin - Utils.tick.ask)/Point());
           
    //   double defaultCloudDistance = (double)GET(BrickSize); //signals.methods.ATROnIndicator(SL_PERCENTILE);
       
       //bool isUpCandle = (medianRenko.RenkoRatesInfoArray[0].close > medianRenko.RenkoRatesInfoArray[0].open) &&
       //   (medianRenko.RenkoRatesInfoArray[1].close > medianRenko.RenkoRatesInfoArray[1].open); 
       //bool isDownCandle = (medianRenko.RenkoRatesInfoArray[0].close < medianRenko.RenkoRatesInfoArray[0].open) &&
       //   (medianRenko.RenkoRatesInfoArray[1].close < medianRenko.RenkoRatesInfoArray[1].open);
       
       //double TenkanMin = 0;
       //double TenkanMax = 0;
       //TYPE_TREND TenkanTrend = 0;
       //Utils.GetIndicatorMinMax(this, TenkanMin, TenkanMax, TenkanTrend, MODE_TENKANSEN, CANDLE_PATTERN_MAXBARS);
       
       //double KijunMin = 0;
       //double KijunMax = 0;
       //TYPE_TREND KijunTrend = 0;
       //Utils.GetIndicatorMinMax(this, KijunMin, KijunMax, KijunTrend, MODE_KIJUNSEN, CANDLE_PATTERN_MAXBARS);
       
       bool isUpCandle = renkoPatterns.IsBullReversal(medianRenko.RenkoRatesInfoArray,1);
       
       // First Sell Signal             
       if ( GET(AllowBUY) 
            && isUpCandle 
            //&& (cloudDistance < defaultCloudDistance)
            //&& (Utils.tick.bid > TRedValue)
            )
       {
           RaiseMarketSignal(1, StringFormat("BUY On %s", EnumToString((ENUM_INDICATORS)GET(FilterIndicator))));
           return;
       }
       
       bool isDownCandle = renkoPatterns.IsBearReversal(medianRenko.RenkoRatesInfoArray,1);
       // First Buy Signal
       if ( GET(AllowSELL)
             && isDownCandle 
             //&& (cloudDistance < defaultCloudDistance)
             //&& (Utils.tick.ask < TRedValue)
          )
       {
           RaiseMarketSignal(-1, StringFormat("SELL On %s", EnumToString((ENUM_INDICATORS)GET(FilterIndicator))));
           return;
       }
      
}

void CIchimokuRenko::Trail(Order &order, int indent)
{      

    if (!m_bInited)
        return;
    if ( order.Valid() && order.Select() )
    {
       if (!Utils.IsTesting())
         this.Refresh();
       double TRedValue = TenkanSen(0);
       double KBlueValue = KijunSen(0);
       
       if ((TRedValue <= 0) || !MathIsValidNumber(TRedValue))
           return;
       if ((KBlueValue <= 0) || !MathIsValidNumber(KBlueValue))
           return;
       //double ABValue = SenkouSpanA(0);
       //double BBValue = SenkouSpanB(0);

       //double IshiMax = MathMax(ABValue, BBValue);
       //double IshiMin = MathMin(ABValue, BBValue);             
       //double IshiMedium = (IshiMax + IshiMin)/2.0;
       //double mediaPrice = (Utils.tick.ask + Utils.tick.bid)/2.0;
       double bid = Utils.Bid();
       double ask = Utils.Ask();
       double Pt = Point();
       double Spread = MathMax(Utils.Spread() * Pt, ask - bid);
       Utils.Trade().SetTrailDelta(Spread + indent*Pt); //(Utils.Spread() + indent + Utils.StopLevelPoints())*Pt);
       
       //order.stopLoss = Utils.OrderStopLoss();
       //order.takeProfit = Utils.OrderTakeProfit();    
       order.openPrice = Utils.OrderOpenPrice();
       //order.profit = order.RealProfit();
       double startDistance = (GET(CoeffBE) * (double)GET(BrickSize))*Pt;
       //double distance = (startDistance + trlstep)*Point;
                
       double SL = order.StopLoss(false);
       double TP = order.TakeProfit(false);
       double OP = order.openPrice;
       double Profit = order.profit;            
       
      if (order.type == OP_BUY)
      {
         double startLevel = order.openPrice + Spread;
         double startCheck = order.openPrice + startDistance;
         if ( (TRedValue > startLevel) && (ask > startCheck))
         {
            if (TrailLevel(order, ask, bid, SL, TP, TRedValue, startLevel))
               return;
         }
      }
      
      if (order.type==OP_SELL)
      {
         double startLevel = order.openPrice - Spread;
         double startCheck = order.openPrice - startDistance;
         if ( (TRedValue  <  startLevel) && (bid < startCheck))
         {
            if (TrailLevel(order, ask, bid, SL, TP, TRedValue, startLevel))
               return;
         }
      }

   }
}


bool CIchimokuRenko::OkToStartBacktest(void)
{
   #ifdef SHOW_INDICATOR_INPUTS   
      static bool _ok = false;
      if(MQLInfoInteger((int)MQL5_TESTING) && !_ok)
      {
         int _count = 20;//MA1period;
         //if(MA1on && (this.inputs.MA1Filter != FILTER_MODE_OFF))
         //   _count = MA1period;
         //if(MA2on && (this.inputs.MA2Filter != FILTER_MODE_OFF))
         //   _count = MathMax(_count,MA2period);
         //if(MA3on && (this.inputs.MA3Filter != FILTER_MODE_OFF))
         //   _count = MathMax(_count,MA3period);
         //if((ShowChannel == _SuperTrend) && (this.inputs.SuperTrendFilter != FILTER_MODE_OFF))
         //   _count = MathMax(_count,SuperTrendPeriod);
      
         static bool _infoShown = false;
         if(!_infoShown)
         {
            Print("(!) Waiting for "+(string)_count+" bars to complete before starting.");
            _infoShown = true;
         }
         
         MqlRates _temp[];      
         
         if(medianRenko.GetMqlRates(_temp,0,Bars(_Symbol,_Period)))
         {
            int _i = 0;
            int _c = 0;
            
            while(_temp[_i].open > 0)
            {
               _c++;
               _i++;
               if(_i > (ArraySize(_temp) - 1))
                  break;
            }           
            
            if(_c > _count)
            {
               Print("(!) Starting the backtest "+(string)_count+" bars present");
               _ok = true;  
            }
         }
         ArrayFree(_temp);   
         
         if(!_ok)
            return false;
      }
   #endif     
   
   return true;
}
