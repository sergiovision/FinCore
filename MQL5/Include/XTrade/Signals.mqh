//+------------------------------------------------------------------+
//|                                                 TradeSignals.mqh |
//|                        Copyright 2018, MetaQuotes Software Corp. |
//|                                             https://www.mql5.com |
//+------------------------------------------------------------------+
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <XTrade\GenericTypes.mqh>

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
class Signal : public SerializableEntity
{
public:
    SignalType type;
    int Value;
    long ObjectId;
    long flags;
    long ChartId;
    string Sym;
    // string Data;

    virtual CJAVal* Persistent() {
        obj["Value"] = Value;
        obj["Id"] = (int)type;
        obj["Flags"] = (int)flags;
        obj["ObjectId"] = ObjectId;
        obj["ChartId"] = ChartId;
        obj["Sym"] = Sym;
        //obj["Data"] = Data; //.AddBase(data);
        return &obj;
    }
    
    Signal(string fromJson)
    {
        obj.Deserialize(fromJson);
        if (obj.FindKey("Value"))
           Value = (int)obj["Value"].ToInt();
        type = (SignalType)obj["Id"].ToInt();
        if (obj.FindKey("Flags"))
           flags = obj["Flags"].ToInt();
        if (obj.FindKey("Sym"))
           Sym = obj["Sym"].ToStr();
        if (obj.FindKey("ChartId"))
           ChartId = obj["ChartId"].ToStr();
        if (obj.FindKey("ObjectId"))
           ObjectId = obj["ObjectId"].ToInt();           
        //if (obj.FindKey("Data"))
        //   Data = obj["Data"].ToStr();           
    }
    
    Signal(SignalFlags fl, SignalType id, long objId, long chartID) {
       this.flags = fl;
       this.ObjectId = objId;
       this.type = id;
       this.Value = 0;
       this.ChartId = chartID;
       this.Sym = Utils.Symbol;
    }

    Signal()
    {
       Init();
    }
    
    virtual void Init()
    {
        Value = 0;
        flags = SignalToAuto;
        type = SignalQuiet;    
    }

    virtual void operator=(const Signal &n) 
    {
        Value = n.Value;
        type = n.type;
    }
      
   virtual bool operator==(const Signal &n) const
   {
     if (Value != n.Value)
        return false;
     if (type != n.type)
        return false;
     return true;
   }
   
   virtual string ToString()
   {
      return EnumToString(type) + " : " + Value;
   }
   
   bool isValid() {
      if ( type < SIGNAL_FIRST || type > SIGNAL_LAST )
         return false;
      return true;
   }
   
};


