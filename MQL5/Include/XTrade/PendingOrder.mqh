//+------------------------------------------------------------------+
//|                                                 PendingOrder.mqh |
//|                                                 Sergei Zhuravlev |
//|                                   http://github.com/sergiovision |
//+------------------------------------------------------------------+
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#define ADD_MINUTES 68

// default lifetime in minutes
#define PENDING_ORDER_LIFETIME 1200

#include <XTrade\GenericTypes.mqh>
#include <XTrade\Orders.mqh>
#include <XTrade\InputTypes.mqh>
#include <ChartObjects\ChartObjectsShapes.mqh>
#include <ChartObjects\ChartObjectsBmpControls.mqh>
#include <ChartObjects\ChartObjectsTxtControls.mqh>

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
class PendingOrder : public Order
{
protected:
   double                defaultPriceShift;
   CChartObjectRectangle rectReward;
   CChartObjectRectangle rectRisk;
   CChartObjectBitmap    OP_arrow;
   datetime              shiftTime;
   CChartObjectLabel     textObj;
public:
   PendingOrder(int typ, int t, ushort nContracts);
   string idRewardRect;
   string idRiskRect;
   string idPending;
   virtual void SetId(long t);
   virtual void SetNContracts(ushort n);
   virtual void ShiftUp();
   virtual void ShiftDown();
   virtual void SetOPLine();
   void rectUpdate(datetime startTime);
   virtual bool isSelected() const { return OP_arrow.Selected(); }
   virtual void doSelect(bool v);
   void InitLoaded();
   string OPLineName() const;
   virtual ~PendingOrder();
   virtual void Delete();   
};

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
PendingOrder::PendingOrder(int typ, int t, ushort nContr)
   :Order(t)
{
   slColor = clrBlue;
   tpColor = clrLightBlue;
   opColor = clrGray;
   SL_LINE_WIDTH = 1;
   TP_LINE_WIDTH = 1;
   symbol = Utils.Symbol;
   //ITrade* parent = Utils.Trade();
   magic = Utils.Service().MagicNumber();
   bDirty = false;
   type = typ;
   SetRole(PendingLimit);
   double ask = Utils.Ask();
   double bid = Utils.Bid();
   SetNContracts(nContr);
   this.expiration = TimeCurrent() + PENDING_ORDER_LIFETIME * 60;
   shiftTime = 5 * ADD_MINUTES * PeriodSeconds() / 60;
   if (type == OP_BUY)
   {
      if (t == -1)
         ticket = PENDING_BUY_TICKET - magic;
      defaultPriceShift = Utils.Trade().DefaultStopLoss()*Point();
      openPrice = ask - defaultPriceShift;
      stopLoss = Utils.Trade().StopLoss(openPrice, type);
      takeProfit = Utils.Trade().TakeProfit(openPrice, type);
   }
   else if (type == OP_SELL)
   {
      if (t == -1)
         ticket = PENDING_SELL_TICKET - magic;
      defaultPriceShift = Utils.Trade().DefaultStopLoss()*Point();
      openPrice = bid + defaultPriceShift;
      stopLoss = Utils.Trade().StopLoss(openPrice, type);
      takeProfit = Utils.Trade().TakeProfit(openPrice, type);
   }
}

void PendingOrder::SetNContracts(ushort n) 
{
   if ((n<=0) || (n>9))
   {
      Utils.Info("NContracts should be in range [1,9]!");
      return;
   }
   this.numberContracts = n;
   ITrade* parent = Utils.Trade();
   lots = parent.ContractsToLots(type, numberContracts);
   textObj.SetString(OBJPROP_TEXT, IntegerToString(this.numberContracts));
}

void PendingOrder::SetId(long t)
{
   Order::SetId(t);
   ITrade* parent = Utils.Trade();
   datetime currentTime = TimeCurrent();
   datetime AddTime = currentTime + shiftTime;   
   
   idPending = StringFormat("OrderPending_%d", ticket);
   idRewardRect = StringFormat("rewardRect_%d", ticket);
   idRiskRect = StringFormat("riskRect_%d", ticket);

   OP_arrow.Create(parent.ChartId(), idPending, parent.SubWindow(), AddTime, openPrice);   
   textObj.Create(parent.ChartId(), idPending + "text", parent.SubWindow(), AddTime, openPrice); 
   textObj.SetString(OBJPROP_TEXT, IntegerToString(this.numberContracts));
   textObj.SetString(OBJPROP_FONT,"Arial Black");

   
   if (type == OP_BUY)
   {
      textObj.SetInteger(OBJPROP_COLOR, clrGreen);
      OP_arrow.BmpFile("/Images/buy.bmp");
   }
   
   if (type == OP_SELL)
   {
      OP_arrow.BmpFile("/Images/sell.bmp");
      textObj.SetInteger(OBJPROP_COLOR, clrRed);
   }
   
   rectUpdate(AddTime);
   
   OP_arrow.Selectable(true);
   OP_arrow.Selected(true);
}

string PendingOrder::OPLineName() const { return StringFormat("OPLINE_%s_%s:%d", TypeToString(), symbol, ticket); }

void PendingOrder::doSelect(bool v) 
{ 
   // setStopLoss(stopLoss);
   updateTP(true);
   updateSL(true);
   InitLoaded();
   // setTakeProfit(takeProfit);
   OP_arrow.Selected(v);
}   

void PendingOrder::Delete() 
{
   SetRole(ShouldBeClosed);
}

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
void PendingOrder::InitLoaded(void)
{
   datetime startTime = 0;
   OP_arrow.GetInteger(OBJPROP_TIME,0,startTime);

   OP_arrow.SetDouble(OBJPROP_PRICE, openPrice);
   rectReward.Delete();
   rectRisk.Delete();
   rectUpdate(startTime);
}

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
PendingOrder::~PendingOrder()
{
   OP_arrow.Delete();
   rectReward.Delete();
   rectRisk.Delete();   
   if (Utils.Trade().AllowVStops())
   {
      string name = OPLineName();
      Utils.ObjDelete(name);
   }
}

void PendingOrder::SetOPLine(void)
{
   if (Utils.Trade().AllowVStops() && (ticket != -1))
   {
      string name = OPLineName();
      if (!Utils.ObjExist(name))
         Utils.HLineCreate(name,0,openPrice,opColor,SL_LINE_STYLE,TP_LINE_WIDTH,false,false,false,0,name);
      else
         Utils.HLineMove(name, openPrice);
   }

}

void PendingOrder::ShiftUp()
{
   datetime startTime = 0;
   OP_arrow.GetInteger(OBJPROP_TIME,0,startTime);

   double shift = MathMax(GET(PendingOrderStep), Utils.Spread())*Point();
   openPrice += shift;
   OP_arrow.SetDouble(OBJPROP_PRICE, openPrice);
   textObj.SetDouble(OBJPROP_PRICE, openPrice);

   stopLoss += shift;
   setStopLoss(stopLoss);
   takeProfit += shift;
   setTakeProfit(takeProfit);
   
   rectReward.Delete();
   rectRisk.Delete();
   rectUpdate(startTime);
   ChartRedraw(Utils.Trade().ChartId());   
}

void PendingOrder::ShiftDown()
{
   datetime startTime = 0;
   OP_arrow.GetInteger(OBJPROP_TIME, 0, startTime);

   double shift = MathMax(GET(PendingOrderStep), Utils.Spread())*Point();
   openPrice -=  shift;
   OP_arrow.SetDouble(OBJPROP_PRICE, openPrice);
   textObj.SetDouble(OBJPROP_PRICE, openPrice);

   stopLoss -= shift;
   setStopLoss(stopLoss);
   takeProfit -= shift;
   setTakeProfit(takeProfit);

   rectReward.Delete();
   rectRisk.Delete();
   rectUpdate(startTime);
   ChartRedraw(Utils.Trade().ChartId());
}

void PendingOrder::rectUpdate(datetime AddTime)
{
   double ask = Utils.Ask();
   double bid = Utils.Bid();
   ITrade* parent = Utils.Trade();
   if (type == OP_BUY)
   {
      if (openPrice < bid)
         role = PendingLimit;
      if (openPrice > ask)
         role = PendingStop;          
      
      datetime  endTime = AddTime + shiftTime; 

      rectReward.Create(parent.ChartId(), idRewardRect,parent.SubWindow(),AddTime,openPrice,endTime,takeProfit);
      rectReward.Color(LightGreen);
      rectReward.Background(true);
   
      rectRisk.Create(parent.ChartId(),idRiskRect,parent.SubWindow(),AddTime,stopLoss,endTime,openPrice);
      rectRisk.Color(LightPink);
      rectRisk.Background(true);
      ObjectSetInteger(parent.ChartId(),idPending, OBJPROP_BACK, 1);
   }
   
   if (type == OP_SELL)
   {      
      if (openPrice > ask)
         role = PendingLimit;
      if (openPrice < bid)
         role = PendingStop;

      datetime  endTime = AddTime + shiftTime;

      rectReward.Create(parent.ChartId(),idRewardRect,parent.SubWindow(),AddTime,openPrice,endTime,takeProfit);
      rectReward.Color(LightGreen);
      rectReward.Background(true);

      rectRisk.Create(parent.ChartId(),idRiskRect,parent.SubWindow(),AddTime,stopLoss,endTime,openPrice);
      rectRisk.Color(LightPink);
      rectRisk.Background(true);
      ObjectSetInteger(parent.ChartId(),idPending, OBJPROP_BACK, 1);
   }
   SetOPLine();

}

