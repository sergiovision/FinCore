//+------------------------------------------------------------------+
//|                                                 ITradeService.mqh|
//|                                                 Sergei Zhuravlev |
//|                                   http://github.com/sergiovision |
//+------------------------------------------------------------------+
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <XTrade\GenericTypes.mqh>
#include <XTrade\Signals.mqh>
#include <XTrade\CommandsController.mqh>


class SettingsFile;

class ITradeService
{
protected:
   bool   isActive;
   long   magic;
   bool   isMaster;
   // SettingsFile* set;

public:
   string fileName;
   string IniFilePath;
   string EAName;
   string sym;
   Constants constant;
   ushort sep;   
   ushort sepList; 
   bool IsEA;
   CommandsController* controller;

   ITradeService(short Port, string EA)
   {
      sep = StringGetCharacter(constant.PARAMS_SEPARATOR, 0);
      sepList = StringGetCharacter(constant.LIST_SEPARATOR, 0);
      magic = DEFAULT_MAGIC_NUMBER;
      EAName = EA;
      sym = "";
      isActive = false;
      //set = NULL;
      isMaster = false;
      //IsEA = true;
   }
   
   virtual bool Init(bool isEA)
   {
       IsEA = isEA;
       return true;
   }
   
   virtual long MagicNumber() {
      return magic;
   }
   
   virtual bool IsMaster() const {
      return isMaster;
   }
   
   virtual void SaveAllSettings(string strExpertData, string strOrdersData) 
   {
   }

   void SetController(CommandsController* c) 
   {
      controller = c;   
   }

   virtual void CallLoadParams(CJAVal* pars) {
   }
   
   virtual string CallStoreParamsFunc() {
      return "";
   }
   
   virtual string Name() { return EAName; }
   virtual bool CheckActive() { return isActive;}
   virtual bool IsActive() { return isActive; }
   
   virtual Signal* ListenSignal(long flags, long ObjectId) { return NULL; };
   
   
   virtual void Log(string message)
   {
      //Print(message);
   }
   
   virtual void ProcessSignals() {   
      // TODO: Implement local signals QUEUE   
   }
   
   virtual uint DeInit(int Reason)
   {
       return INIT_SUCCEEDED;
   }
   
   //virtual SettingsFile* Settings()
   //{
   //    return NULL;
   //}
   
   virtual string GetProfileString(string lpSection, string lpKey)
   {
      return "";
   }
   
   virtual bool WriteProfileString(string lpSection, string lpKey,string lpValue)
   {
      return false;
   }
   
   virtual string FileName()
   {
      return fileName;
   }

   virtual string FilePath()
   {
      return IniFilePath;
   }
   
   virtual void PostSignal(Signal* s)  {  }
   //{
   //   PostSignalLocally(s);
   //}
   
   virtual Signal* SendSignal(Signal* s) { return NULL; }

   virtual void PostSignalLocally(Signal* signal)
   {
      
   }
   
   virtual void DealsHistory(int days)
   {
      
   }
   
   virtual void UpdateRates(string symbols)
   {
   }
   
   virtual string Levels4Symbol(string symbolx) 
   {
      string result = "";
      return result;
   }
   
   //void NotifyUpdatePositions()
   //{ 
   //   if (Utils.IsTesting())
   //       return;
   //   Signal* signal = new Signal(SignalToServer, SIGNAL_ACTIVE_ORDERS, this.MagicNumber());
   //   PostSignalLocally(signal);
   //}
};

