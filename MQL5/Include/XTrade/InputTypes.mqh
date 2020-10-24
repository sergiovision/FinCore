//+------------------------------------------------------------------+
//|                                                 ThriftClient.mqh |
//|                                                 Sergei Zhuravlev |
//|                                   http://github.com/sergiovision |
//+------------------------------------------------------------------+
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <XTrade\GenericTypes.mqh>
#include <XTrade\IsSession.mqh>

#ifdef THRIFT

#define EXPERT_PARAMS_CLASS class ExpertSettings : public SerializableEntity {  public:
#define END_EXPERT_PARAMS_CLASS  };  ExpertSettings eset; 
#define INPUT_VARIABLE(var_name, var_type) var_type var_name; 
#define GET(parameterx) eset.parameterx
#define STORE_INT_VARIABLE(var_name, save_type, var_string, def_value) if (bSave) SaveVariable(var_string, this.var_name); \
 else  this.var_name = LoadIntVariable(var_string, def_value);
#define STORE_STR_VARIABLE(var_name, save_type, var_string, def_value) if (bSave) SaveVariable(var_string, this.var_name); \
 else  this.var_name = LoadStrVariable(var_string, def_value);
#define STORE_BOOL_VARIABLE(var_name, save_type, var_string, def_value) if (bSave) SaveVariable(var_string, this.var_name); \
 else  this.var_name = LoadBoolVariable(var_string, def_value);
#define STORE_DBL_VARIABLE(var_name, save_type, var_string, def_value) if (bSave) SaveVariable(var_string, this.var_name); \
 else  this.var_name = LoadDblVariable(var_string, def_value);

#define EXPERT_CLASS_STORE  string Save(bool bSave) {
#define EXPERT_CLASS_STORE_END  return obj.Serialize(); }

#else 

#define EXPERT_PARAMS_CLASS  
#define END_EXPERT_PARAMS_CLASS  
#define STORE_INT_VARIABLE(var_name, save_type, var_string, def_value) input save_type var_name = def_value;
#define STORE_STR_VARIABLE(var_name, save_type, var_string, def_value) input save_type var_name = def_value;
#define STORE_BOOL_VARIABLE(var_name, save_type, var_string, def_value) input save_type var_name = def_value;
#define STORE_DBL_VARIABLE(var_name, save_type, var_string, def_value) input save_type var_name = def_value;

#define INPUT_VARIABLE(var_name, var_type) 
#define GET(parameterx) parameterx
#define STORE_VARIABLE(var_name, save_type, var_string) 
#define EXPERT_CLASS_STORE 
#define EXPERT_CLASS_STORE_END 

#endif

EXPERT_PARAMS_CLASS
INPUT_VARIABLE(LotsBUY, double)
INPUT_VARIABLE(LotsSELL, double)
//INPUT_VARIABLE(LotsMIN, double, 0.02)
//--------------------------------------------------------------------
// Renko settings
INPUT_VARIABLE(BrickSize, long)
INPUT_VARIABLE(RenkoType, long)
INPUT_VARIABLE(InfoTextColor, long)
INPUT_VARIABLE(Wicks, bool)
//--------------------------------------------------------------------
INPUT_VARIABLE(PanelSize, long)
INPUT_VARIABLE(RefreshTimeFrame, long)
INPUT_VARIABLE(AllowBUY, bool)
INPUT_VARIABLE(AllowSELL, bool)
INPUT_VARIABLE(BUYBegin, string) 
INPUT_VARIABLE(BUYEnd, string)
INPUT_VARIABLE(SELLBegin, string)
INPUT_VARIABLE(SELLEnd, string)
INPUT_VARIABLE(AllowVirtualStops, bool)
INPUT_VARIABLE(CoeffSL, double)
INPUT_VARIABLE(CoeffTP, double)
INPUT_VARIABLE(CoeffBE, double) // If BE == 0 then no BE. BE should be more than spread
INPUT_VARIABLE(PendingOrderStep, long)
INPUT_VARIABLE(MoreTriesOpenOrder, bool)
INPUT_VARIABLE(AllowMarketOrders, bool)
//--------------------------------------------------------------------
// Indicators 
INPUT_VARIABLE(EnableRenko, bool)
INPUT_VARIABLE(EnableRenkoMA, bool)
INPUT_VARIABLE(FilterIndicator, long)
INPUT_VARIABLE(SignalIndicator, long)
INPUT_VARIABLE(WeightCalculation, long)
INPUT_VARIABLE(NumBarsToAnalyze, long)
// INPUT_VARIABLE(NumBarsFlatPeriod, long)
INPUT_VARIABLE(IshimokuPeriod1, long)
INPUT_VARIABLE(IshimokuPeriod2, long)
INPUT_VARIABLE(IshimokuPeriod3, long)

INPUT_VARIABLE(BandsPeriod, long)
INPUT_VARIABLE(BandsDeviation, double)
INPUT_VARIABLE(EnableBBands, double)
INPUT_VARIABLE(EnableStochastic, bool)
INPUT_VARIABLE(EnableRSI, bool)
INPUT_VARIABLE(EnableHistogram, bool)
INPUT_VARIABLE(EnableFChannel, bool)
INPUT_VARIABLE(EnableTrendForecast, bool)
INPUT_VARIABLE(EnableVolumes, bool)
// Trailing data
//--------------------------------------------------------------------
INPUT_VARIABLE(TrailingType, long)
INPUT_VARIABLE(TrailingIndent, long)
// INPUT_VARIABLE(TrailInLoss, bool) // If true - stoploss should be defined!!!

// News Params
//INPUT_VARIABLE(EnableNews, bool)
//INPUT_VARIABLE(RaiseSignalBeforeEventMinutes, long)
//INPUT_VARIABLE(NewsPeriodMinutes, long)
//INPUT_VARIABLE(MinImportance, long)
//--------------------------------------------------------------------
INPUT_VARIABLE(Slippage, long)

EXPERT_CLASS_STORE
STORE_DBL_VARIABLE(LotsBUY, double, "LotsBUY", 0.02)
STORE_DBL_VARIABLE(LotsSELL, double, "LotsSELL", 0.02)
STORE_INT_VARIABLE(BrickSize, long, "BrickSize", 100)
STORE_INT_VARIABLE(RenkoType, long, "RenkoType", 3)
STORE_INT_VARIABLE(InfoTextColor, long, "InfoTextColor", (long)clrBlack)
STORE_BOOL_VARIABLE(Wicks, bool, "showWicks", true)
STORE_INT_VARIABLE(PanelSize, long, "PanelSize", (long)PanelNormal)
STORE_INT_VARIABLE(RefreshTimeFrame, long, "RefreshTimeFrame", (long)PERIOD_M1)
STORE_BOOL_VARIABLE(AllowBUY, bool, "AllowBUY", true)
STORE_BOOL_VARIABLE(AllowSELL, bool, "AllowSELL", true)
STORE_STR_VARIABLE(BUYBegin, string, "BUYBegin", "") 
STORE_STR_VARIABLE(BUYEnd, string, "BUYEnd", "")
STORE_STR_VARIABLE(SELLBegin, string, "SELLBegin", "")
STORE_STR_VARIABLE(SELLEnd, string, "SELLEnd", "")
STORE_BOOL_VARIABLE(AllowVirtualStops, bool, "AllowVirtualStops", true)
STORE_DBL_VARIABLE(CoeffSL, double, "CoeffSL", 3)
STORE_DBL_VARIABLE(CoeffTP, double, "CoeffTP", 3.5)
STORE_DBL_VARIABLE(CoeffBE, double, "CoeffBE", 1.5) // If BE == 0 then no BE. BE should be more than spread
STORE_INT_VARIABLE(PendingOrderStep, long, "PendingOrderStep", 4)
STORE_BOOL_VARIABLE(MoreTriesOpenOrder, bool, "MoreTriesOpenOrder", false)
STORE_BOOL_VARIABLE(AllowMarketOrders, bool,  "AllowMarketOrders", false)
STORE_BOOL_VARIABLE(EnableRenko, bool, "EnableRenko", false)
STORE_BOOL_VARIABLE(EnableRenkoMA, bool, "EnableRenkoMA", false)
STORE_INT_VARIABLE(FilterIndicator, long, "FilterIndicator", (long)DefaultIndicator)
STORE_INT_VARIABLE(SignalIndicator, long, "SignalIndicator", (long)NoIndicator)
STORE_INT_VARIABLE(WeightCalculation, long, "WeightCalculation", (long)WeightBySignal)
STORE_INT_VARIABLE(NumBarsToAnalyze, long, "NumBarsToAnalyze", 7)
// STORE_INT_VARIABLE(NumBarsFlatPeriod, long, "NumBarsFlatPeriod", (long)ISHIMOKU_PLAIN_NOTRADE)
STORE_INT_VARIABLE(IshimokuPeriod1, long, "IshimokuPeriod1", 7)
STORE_INT_VARIABLE(IshimokuPeriod2, long, "IshimokuPeriod2", 35)
STORE_INT_VARIABLE(IshimokuPeriod3, long, "IshimokuPeriod3", 105)
STORE_INT_VARIABLE(BandsPeriod, long, "BandsPeriod", 20)
STORE_DBL_VARIABLE(BandsDeviation, double, "BandsDeviation", 1.6)
STORE_BOOL_VARIABLE(EnableBBands, bool, "EnableBBands", false)
STORE_BOOL_VARIABLE(EnableStochastic, bool, "EnableStochastic", false)
STORE_BOOL_VARIABLE(EnableRSI, bool, "EnableRSI", false)
STORE_BOOL_VARIABLE(EnableHistogram, bool, "EnableHistogram", false)
STORE_BOOL_VARIABLE(EnableFChannel, bool, "EnableFChannel", false)
STORE_BOOL_VARIABLE(EnableTrendForecast, bool, "EnableTrendForecast", false)
STORE_BOOL_VARIABLE(EnableVolumes, bool, "EnableVolumes", true)
STORE_INT_VARIABLE(TrailingType, long, "TrailingType", (long)TrailingStairs)
STORE_INT_VARIABLE(TrailingIndent, long, "TrailingIndent", 0)
STORE_INT_VARIABLE(Slippage, long,  "Slippage", 10)

EXPERT_CLASS_STORE_END
END_EXPERT_PARAMS_CLASS

int ThriftPORT = Constants::MQL_PORT;
long actualSlippage = GET(Slippage);
int MaxOpenedTrades = 4;

// INPUT_VARIABLE(ThriftPORT, int, 2010)
