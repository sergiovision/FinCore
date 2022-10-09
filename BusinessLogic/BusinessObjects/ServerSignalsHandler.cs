using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using BusinessLogic.Jobs;
using BusinessLogic.Repo;
using BusinessObjects;
using BusinessObjects.BusinessObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BusinessLogic.BusinessObjects
{
    public class ServerSignalsHandler : ISignalHandler
    {
        private readonly IWebLog log;

        private readonly MainService xtrade;
        //private readonly ITerminalEvents terminals;

        public ServerSignalsHandler()
        {
            xtrade = MainService.thisGlobal;
            //terminals = MainService.thisGlobal.Container.Resolve<ITerminalEvents>();
            log = MainService.thisGlobal.Container.Resolve<IWebLog>();
        }

        public SignalInfo ListenSignal(long flags, long objectId)
        {
            return xtrade.ListenSignal(flags, objectId);
        }

        public void PostSignal(SignalInfo signal, IMessagingServer server)
        {
            if ((SignalFlags) signal.Flags == SignalFlags.Cluster)
            {
                xtrade.PostSignalTo(signal);
                return;
            }

            switch ((EnumSignals) signal.Id)
            {
                case EnumSignals.SIGNAL_POST_LOG:
                {
                    if (string.IsNullOrEmpty(signal.Data))
                        break;
                    MainService.thisGlobal.DoLog(signal);
                }
                    break;
                case EnumSignals.SIGNAL_CHECK_HEALTH:
                    if (Utils.IsDebug())
                        log.Info("CheckHealth: " + signal.Flags);
                    break;
                case EnumSignals.SIGNAL_DEALS_HISTORY:
                {
                    List<DealInfo> deals = Utils.ExtractList<DealInfo>(signal.Data);
                    xtrade.SaveDeals(deals);
                }
                    break;
                case EnumSignals.SIGNAL_CHECK_BALANCE:
                {
                    if (signal.Data == null)
                        break;
                    var jList = Utils.ExtractList<BalanceInfo>(signal.Data);
                    if (Utils.HasAny(jList))
                    {
                        BalanceInfo info = jList.FirstOrDefault();
                        xtrade.UpdateBalance((int)info.Account, info.Balance, info.Equity);
                    }
                } break;
                case EnumSignals.SIGNAL_UPDATE_RATES:
                {
                    try
                    {
                        var jRates = Utils.ExtractList<RatesInfo>(signal.Data);
                        xtrade.UpdateRates(jRates);
                    }
                    catch (Exception e)
                    {
                        log.Info(string.Format($"GetBYNUSDRates Error: {0}", e));
                    }
                }
                    break;
                case EnumSignals.SIGNAL_ACTIVE_ORDERS:
                {
                    List<PositionInfo> positions = Utils.ExtractList<PositionInfo>(signal.Data);
                    var terminals = MainService.thisGlobal.Container.Resolve<ITerminalEvents>();
                    terminals.UpdatePositions(signal.ObjectId, signal.Value, positions);
                } break;
                case EnumSignals.SIGNAL_ADD_ORDERS:
                {
                    List<PositionInfo> orders = Utils.ExtractList<PositionInfo>(signal.Data);
                    if (Utils.HasAny(orders))
                    {
                        var terminals = MainService.thisGlobal.Container.Resolve<ITerminalEvents>();
                        terminals.AddOrders(signal.ObjectId, signal.Value, orders);
                    }
                    
                } break;
                case EnumSignals.SIGNAL_DELETE_ORDERS:
                {
                    List<PositionInfo> orders = Utils.ExtractList<PositionInfo>(signal.Data);
                    if (Utils.HasAny(orders))
                    {
                        var terminals = MainService.thisGlobal.Container.Resolve<ITerminalEvents>();
                        terminals.DeleteOrders(signal.ObjectId, signal.Value, orders);
                    }
                } break;
                case EnumSignals.SIGNAL_UPDATE_ORDERS:
                {
                    List<PositionInfo> orders = Utils.ExtractList<PositionInfo>(signal.Data);
                    if (Utils.HasAny(orders))
                    {
                        var terminals = MainService.thisGlobal.Container.Resolve<ITerminalEvents>();
                        terminals.UpdateOrders(signal.ObjectId, signal.Value, orders);
                    }
                } break;
                case EnumSignals.SIGNAL_WARN_NEWS:
                    break;
                case EnumSignals.SIGNAL_DEINIT_EXPERT:
                {
                    var expert = JsonConvert.DeserializeObject<ExpertInfo>(signal.Data);
                    xtrade.DeInitExpert(expert);
                } break;
                
                case EnumSignals.SIGNAL_DEINIT_TERMINAL:
                {
                    var expert = JsonConvert.DeserializeObject<ExpertInfo>(signal.Data);
                    xtrade.DeInitTerminal(expert);
                }
                    break;
                case EnumSignals.SIGNAL_LEVELS4SYMBOL:
                {
                    var symbol = signal.Sym;
                    var levelsString = xtrade.Levels4Symbol(symbol);
                    var result = xtrade.CreateSignal(SignalFlags.Expert, signal.ObjectId, (EnumSignals) signal.Id,
                        signal.ChartId);
                    result.Sym = symbol;
                    result.SetData(levelsString);
                    var send = JsonConvert.SerializeObject(result);
                    if (server != null)
                        server.MulticastText(send);
                }
                    break;
                case EnumSignals.SIGNAL_SAVELEVELS4SYMBOL:
                {
                    var levels = signal.Data;
                    xtrade.SaveLevels4Symbol(signal.Sym, levels);
                    log.Info($"Levels Saved For Symbol {signal.Sym}: {levels}");
                }
                    break;
                case EnumSignals.SIGNAL_SAVE_OBJECT:
                {
                    var ds = MainService.thisGlobal.Container.Resolve<DataService>();
                    EntitiesEnum entityType = (EntitiesEnum)(short)signal.Value;
                    DynamicProperties props = ds.GetPropertiesInstance((short) entityType, (int)signal.ObjectId);
                    if (props == null)
                    {
                        props = new DynamicProperties()
                        {
                            objId = (int) signal.ObjectId,
                            entityType = (short) entityType,
                            Vals = signal.Data
                        };
                    }
                    else
                    {
                        props.Vals = signal.Data;
                    }
                    bool res = ds.SavePropertiesInstance(props);
                    log.Info($"SaveObject: {signal.Sym}, Entity: {entityType.ToString()}, Obj:{props.objId} success: {res}");
                }
                    break;
                default:
                    if (server != null)
                    {
                        var result = xtrade.SendSignal(signal);
                        if (result != null)
                        {
                            var send = JsonConvert.SerializeObject(result);
                            server.MulticastText(send);
                        }
                    }

                    break;
            }
        }
        
        public void ProcessMessage(WsMessage wsMessage, IMessagingServer server)
        {
            switch (wsMessage.Type)
            {
                case WsMessageType.GetAllText:
                {
                    wsMessage.Message = log.GetAllText();
                    wsMessage.Type = WsMessageType.GetAllText;
                    var send = JsonConvert.SerializeObject(wsMessage);
                    server.MulticastText(send);
                }
                    break;
                case WsMessageType.ClearLog:
                {
                    log.ClearLog();
                }
                    break;
                case WsMessageType.WriteLog:
                {
                    log.Info(wsMessage.Message);
                }
                    break;
                case WsMessageType.GetAllPositions:
                {
                    var message = new WsMessage();
                    message.From = wsMessage.From;
                    message.Type = WsMessageType.GetAllPositions;
                    var terminals = MainService.thisGlobal.Container.Resolve<ITerminalEvents>();
                    message.Message = JsonConvert.SerializeObject(terminals.GetAllPositions());
                    var send = JsonConvert.SerializeObject(message);
                    server.MulticastText(send);
                }
                    break;
                case WsMessageType.GetCryptoPositions:
                {
                    var message = new WsMessage();
                    message.From = wsMessage.From;
                    message.Type = WsMessageType.GetCryptoPositions;
                    var te = MainService.thisGlobal.Container.ResolveNamed<ITerminalEvents>("crypto");
                    message.Message = JsonConvert.SerializeObject(te.GetAllPositions());
                    var send = JsonConvert.SerializeObject(message);
                    server.MulticastText(send);
                }
                    break;

                case WsMessageType.GetAllPerformance:
                {
                    var message = new WsMessage();
                    message.From = wsMessage.From;
                    message.Type = WsMessageType.GetAllPerformance;
                    message.Message = "[]";
                    var ds = MainService.thisGlobal.Container.Resolve<DataService>();
                    var month = int.Parse(wsMessage.Message);
                    ds.StartPerf(month);
                    var send = JsonConvert.SerializeObject(message);
                    server.MulticastText(send);
                }
                    break;
                case WsMessageType.GetAllCapital:
                {
                    var message = new WsMessage();
                    message.From = wsMessage.From;
                    message.Type = WsMessageType.GetAllCapital;
                    message.Message = "[]";
                    var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(wsMessage.Message);
                    var wallet = int.Parse(result["WalletId"]);
                    var dateTimeFrom = DateTime.Parse(result["from"]);
                    var dateTimeTo = DateTime.Parse(result["to"]);
                    xtrade.GetWalletBalanceRangeAsync(wallet, dateTimeFrom, dateTimeTo);
                    var send = JsonConvert.SerializeObject(message);
                    server.MulticastText(send);
                }
                    break;
                case WsMessageType.UpdatePosition:
                {
                    var result = JsonConvert.DeserializeObject<PositionInfo>(wsMessage.Message);
                    if (result != null)
                    {
                        var terminals = MainService.thisGlobal.Container.Resolve<ITerminalEvents>();
                        terminals.UpdatePositionFromClient(result);
                    }
                }
                    break;
                case WsMessageType.UpdateCryptoPosition:
                {
                    var result = JsonConvert.DeserializeObject<PositionInfo>(wsMessage.Message);
                    if (result != null)
                    {
                        var te = MainService.thisGlobal.Container.ResolveNamed<ITerminalEvents>("crypto");
                        te.UpdatePositionFromClient(result);
                    }
                }
                    break;
                case WsMessageType.GetLevels:
                {
                    wsMessage.From = "Server";
                    wsMessage.Message = xtrade.Levels4Symbol(wsMessage.Message);
                    wsMessage.Type = WsMessageType.GetLevels;
                    var send = JsonConvert.SerializeObject(wsMessage);
                    server.MulticastText(send);
                }
                    break;


                default:
                {
                    log.Info("Undefined Message");
                    // Multicast message to all connected sessions
                    // ((WsServer)Server).MulticastText(message);
                }
                    break;
            }
        }
    }
}