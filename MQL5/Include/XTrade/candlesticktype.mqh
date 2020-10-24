//+------------------------------------------------------------------+
//|                                                 ThriftClient.mqh |
//|                                                 Sergei Zhuravlev |
//|                                   http://github.com/sergiovision |
//+------------------------------------------------------------------+
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <XTrade\GenericTypes.mqh>

//+------------------------------------------------------------------+
//|   TYPE_TREND                                                     |
//+------------------------------------------------------------------+


enum TYPE_CANDLESTICK
{
   CAND_NONE,           //Unknown
   CAND_MARIBOZU,       //Maribozu
   CAND_MARIBOZU_LONG,  //Maribozu long
   CAND_DOJI,           //Doji
   CAND_SPIN_TOP,       //Spins
   CAND_HAMMER,         //Hammer
   CAND_INVERT_HAMMER,  //Inverted Hammer
   CAND_LONG,           //Long
   CAND_SHORT,          //Short
   CAND_STAR            //Star
};
//+------------------------------------------------------------------+
//|   CANDLE_STRUCTURE                                               |
//+------------------------------------------------------------------+
struct CANDLE_STRUCTURE
  {
   double            open,high,low,close; // OHLC
   datetime          time;     //Time
   TYPE_TREND        trend;    //Trend
   bool              bull;     //Bullish candlestick
   double            bodysize; //Size of body
   TYPE_CANDLESTICK  type;     //Type of candlestick
  };
//+------------------------------------------------------------------+
//|   Function of determining of candlestick                         |
//+------------------------------------------------------------------+
bool RecognizeCandle(string symbol,ENUM_TIMEFRAMES period,datetime time,int aver_period,CANDLE_STRUCTURE &res)
  {
   MqlRates rt[];
//--- Get details of the previous candlestick
   if(CopyRates(symbol,period,time, aver_period+1,rt) < aver_period)
   {
      return(false);
   }
   if (ArraySize(rt) <= aver_period)
      return false; 
   res.open=rt[aver_period].open;
   res.high=rt[aver_period].high;
   res.low=rt[aver_period].low;
   res.close=rt[aver_period].close;
   res.time=rt[aver_period].time;
//--- Determine direction of trend
   double aver=0;
   int i = 0;
   for(i=0;i<aver_period;i++)
     {
      aver+=rt[i].close;
     }
   aver=aver/aver_period;

   if(aver<res.close) res.trend=UPPER;
   if(aver>res.close) res.trend=DOWN;
   if(aver==res.close) res.trend=LATERAL;
//--- Determine if it's a bullish or a bearish candlestick
   res.bull=res.open<res.close;
//--- Get the absolute size of body of candlestick
   res.bodysize=MathAbs(res.open-res.close);
//--- Get sizes of shadows
   double shade_low=res.close-res.low;
   double shade_high=res.high-res.open;
   if(res.bull)
     {
      shade_low=res.open-res.low;
      shade_high=res.high-res.close;
     }
   double HL=res.high-res.low;
//--- Calculate average size of body of previous candlesticks
   double sum=0;
   for(i=1; i<=aver_period; i++)
      sum=sum+MathAbs(rt[i].open-rt[i].close);
   sum=sum/aver_period;
//--- Determine type of candlestick   
   res.type=CAND_NONE;
//--- long 
   if(res.bodysize>sum*1.3) res.type=CAND_LONG;
//--- sort 
   if(res.bodysize<sum*0.5) res.type=CAND_SHORT;
//--- doji
   if(res.bodysize<HL*0.03) res.type=CAND_DOJI;
//--- maribozu
   if((shade_low<res.bodysize*0.01 || shade_high<res.bodysize*0.01) && res.bodysize>0)
     {
      if(res.type==CAND_LONG)
         res.type=CAND_MARIBOZU_LONG;
      else
         res.type=CAND_MARIBOZU;
     }
//--- hammer
   if(shade_low>res.bodysize*2 && shade_high<res.bodysize*0.1) res.type=CAND_HAMMER;
//--- invert hammer
   if(shade_low<res.bodysize*0.1 && shade_high>res.bodysize*2) res.type=CAND_INVERT_HAMMER;
//--- spinning top
   if(res.type==CAND_SHORT && shade_low>res.bodysize && shade_high>res.bodysize) res.type=CAND_SPIN_TOP;
   ArrayFree(rt);
   return(true);
  }
//+------------------------------------------------------------------+
