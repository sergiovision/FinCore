using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Autofac;
using BusinessObjects.BusinessObjects;

namespace BusinessObjects
{
    public interface IMainService : IDataService
    {
        ILifetimeScope Container { get; }

        
        void Init(ILifetimeScope container);

        void Dispose();

        bool InitScheduler(bool bServerMode);

        void RunJobNow(string group, string name);

        void StopJobNow(string group, string name);

        string GetJobProp(string group, string name, string prop);

        void SetJobCronSchedule(string group, string name, string cron);

        public List<DealInfo> GetDeals();

        List<ScheduledJobInfo> GetAllJobsList();

        Dictionary<string, ScheduledJobInfo> GetRunningJobs();

        DateTime? GetJobNextTime(string group, string name);

        DateTime? GetJobPrevTime(string group, string name);

        void PauseScheduler();

        void ResumeScheduler();

        TimeZoneInfo GetBrokerTimeZone();

        bool IsDebug();

        ExpertInfo InitExpert(ExpertInfo expert);

        ExpertInfo InitTerminal(ExpertInfo expert);

        // void SaveExpert(ExpertInfo expert);

        void DeInitExpert(ExpertInfo expert);

        void DeInitTerminal(ExpertInfo expert);
        
        int DeleteHistoryOrders(string filePath);

        void DeployToTerminals(string sourceFolder);

        string DeployToAccount(int id);

        List<Wallet> GetWalletBalanceRange(int WalletId, DateTime from, DateTime to);

        void GetWalletBalanceRangeAsync(int WalletId, DateTime from, DateTime to);

        bool UpdateAccountState(AccountState accState);

        SignalInfo ListenSignal(long ReciverObj, long flags);

        void PostSignalTo(SignalInfo signal);

        SignalInfo SendSignal(SignalInfo expert);

        void SubscribeToSignals(long objectId);

        SignalInfo CreateSignal(SignalFlags flags, long ObjectId, EnumSignals Id, long chartId);

        ConcurrentDictionary<string, Rates> GetRates(bool IsReread);

        void UpdateRates(List<RatesInfo> rates);

        string GetRatesList();

        Tuple<Type, Type> EnumToType(EntitiesEnum entities);

        Dictionary<EntitiesEnum, Tuple<Type, Type>> GetTypes();

        string Levels4Symbol(string strSymbol);

        string GetLogContent(string logName, long size);

        object LogList();
    }
}
