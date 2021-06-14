export enum EntitiesEnum {
  Undefined = 0,
  Account = 1,
  Settings = 2,
  Adviser = 3,
  MetaSymbol = 4,
  Symbol = 5,
  Terminal = 6,
  Deals = 7,
  Jobs = 8,
  ExpertCluster = 9,
  Wallet = 10,
  AccountState = 11,
  Rates = 12,
  Country = 13,
  Currency = 14, //
  NewsEvent = 15,
  Person = 16,
  Site = 17,
  Position = 18
}

export interface DynamicProperty<T> {
  type: string;
  // name: string;
  // group: string;
  value: T;
}

export interface DynamicProperties {
  Id: number;
  objId: number;
  entityType: EntitiesEnum;
  Vals: string;
  updated: Date;
}


export class DynamicPropertyDefinition<T> {
  type: string;
  name: string;
  group: string;
  control: string;
  defaultValue: T;
  minValue?: T;
  maxValue?: T;
  description: string;

  constructor(x: new () => T) {
      this.type = x.name;
      // this.defaultValue = defaultValue;
  }
}

export class Person {
    Id: number;
    CountryId: number;
    Created: Date;
    Languageid: number;
    Credential: string;
    Regip: string;
    Mail: string;
    Privilege: string;
    Uuid: string;
    Activated: number;
    Retired: boolean;

    createNew(): Person {

      this.Created = new Date();
      this.Languageid = 0;
      this.Credential = 'password';
      this.Regip = '127.0.0.1';
      this.Mail = 'undef@mail.com';
      this.Privilege = 'GUEST';
      this.Uuid = '';
      this.Activated = 0;
      this.Retired = false;
      return this;
    }
}

export class UserToken {
    access_token: string;
    token_type: string;
    expires_in: number;
    userName: string;
}

export interface Dictionary<T> {
    [Key: string]: T;
}

export class Adviser {
    Id: number;
    Name: string;
    TerminalId: number;
    Symbol: string;
    Timeframe: number;
    Running: boolean;
    Retired: boolean;
    IsMaster: boolean;
    MetaSymbolId: number;
}

export class Terminal {
    Id: number;
    AccountNumber: number;
    Broker: string;
    FullPath: string;
    CodeBase: string;
    Retired: boolean;
    Demo: boolean;
    Stopped: boolean;
}

  export class NewsCalendarEvent {
    text: string;
    startDate: Date;
    endDate: Date;
    currency: string;
  }

  export class NewsEventInfo {
    Currency: string;
    Name: string;
    Importance: number;
    RaiseDateTime: string;
  }

  export class Deal {
    Id: number;
    AccountId: number;
    PersonId: number;
    SiteId: number;
    Name: string;
    ShortName: string;
    Retired: boolean;
    Balance: number;
    OpenTime: Date;
    CloseTime: Date;
    Comment: string;
    SwapValue: number;
    Commission: number;
}

  export class PositionInfo {
    set Id(val: number) {
      this.Ticket = val;
    }
    get Id(): number {
      return this.Ticket;
    }
    Ticket: number;
    Account: number;
    AccountName: string;
    Type: number;
    Magic: number;
    Lots: number;
    Symbol: string;
    MetaSymbol: string;
    ProfitStopsPercent: number;
    ProfitBricks: number;
    Profit: number;
    Role: string;
    Openprice: number;
    contractSize: number;
    cur: string;
    Value: number;
    Vsl: number;
    Realsl: number;
    Vtp: number;
    Realtp: number;
    Swap: number;
    Commission: number;
    Expiration: Date;
    // calcValue(): number {
    //   return this.Lots * this.contractSize * this.Openprice + this.Profit;
    // }
  }

  export class MetaSymbolStat {
    MetaId: number;
    Name: string;
    Description: string;
    NumOfTrades: number;
    TotalProfit: number;
    ProfitPerTrade: number;
    Date: Date;
  }

  export class TodayStat {
      TodayGainReal: number;
      TodayGainDemo: number;
      TodayGainRealPercent: number;
      TodayGainDemoPercent: number;
      TodayBalanceReal: number;
      TodayBalanceDemo: number;
      RISK_PER_DAY: number;
      DAILY_MIN_GAIN: number;
      DAILY_LOSS_AFTER_GAIN: number;
      Deals: Deal[];
      Accounts: Account[];
  }

export class Wallet {
    Id: number;
    PersonId: number;
    SiteId: number;
    Name: string;
    Shortname: string;
    Retired: boolean;
    Date: Date;
    Balance: number;

    createNew(parentId: number): Wallet {
      this.PersonId = 1;
      this.SiteId = undefined;
      this.Name = 'wallet1';
      this.Shortname = 'w1';
      this.Retired = false;
      this.Date = new Date();
      return this;
    }

}

export class AccountState {
    Id: number;
    AccountId: number;
    Date: Date;
    Balance: number;
    Comment: string;
    Formula: string;
}

export enum AccountType {
  Checking = 0,
  Investment = 1
}


export class AccountView {
  Id: number;
  Description: string;
  Retired: boolean;
  Balance: number;
  Equity: number;
  TerminalId: number;
  PersonId: number;
  WalletId: number;
  CurrencyId: number;
  CurrencyStr: string;
  Number: number;
  Lastupdate: Date;
  Typ: AccountType;
}

export class Account {
    Id: number;
    Description: string;
    Balance: number;
    Equity: number;
    CurrencyId: number;
    CurrencyStr: string;
    WalletId: number;
    TerminalId: number;
    PersonId: number;
    Number: number;
    Lastupdate: Date;
    Retired: boolean;
    Typ: number;
    DailyProfit: number;
    DailyMaxGain: number;
    StopTrading: boolean;
    StopReason: string;

    createNew(parentId: number): Account {
      this.Description = 'New Account';
      this.Balance = 0;
      this.Equity = 0;
      this.CurrencyId = 1;
      this.CurrencyStr = 'USD';
      this.WalletId = parentId;
      this.TerminalId = -1;
      this.PersonId = 1;
      this.Number = 0;
      this.Lastupdate = new Date();
      this.Retired = false;
      this.Typ = 0;
      this.DailyProfit = 0;
      this.DailyMaxGain = 0;
      this.StopTrading = false;
      return this;
    }

}

export class TimeStat {
  Date: Date;
  X: number;
  Period: number;
  CheckingValue: number;
  InvestingValue: number;
  CheckingChange: number;
  InvestingChange: number;
  Gains: number;
  Losses: number;
}

export class SelectYear {
  id: number;
  name: string;
  valueFrom: Date;
  valueTo: Date;
}

export class SelectMonth {
    id: number;
    name: string;
    value: number;
}

export enum WsMessageType {
    WriteLog = 0,
    ClearLog = 1,
    GetAllText = 2,
    InsertPosition = 3,
    UpdatePosition = 4,
    RemovePosition = 5,
    GetAllPositions = 6,
    GetAllPerformance = 7,
    ChartValue = 8,
    ChartDone = 9,
    GetAllCapital = 10
}

export class WsMessage {
    Type: WsMessageType;
    From: string;
    Message: string;
}

export interface IWebsocketCallback {
    onOpen(evt: MessageEvent): void;

    onClose(): void;

    onMessage(msg: WsMessage): void;

    onError(evt: MessageEvent): void;
}

export interface Env {
  production: boolean;
  baseURL: string;
  wsURL: string;
  dateFormat: string;
  shortDateFormat: string;
}

export interface Asset {
    ID: number;
    Name: string;
    SharePercentValue: number;
    Value: number;
}

export enum SymbolType {
  Currencies = 0,
  Metals = 1,
  Indexes = 2,
  Commodities = 3,
  Cryptos = 4,
  Stocks = 5,
  ETF = 6
}

export class MetaSymbol {
    Id: number;
    Name: string;
    Description: string;
    C1: string;
    C2: string;
    Typ: SymbolType;
    Retired: boolean;

    createNew(): MetaSymbol {
      this.Name = 'Undef';
      this.Description = 'No';
      this.C1 = 'C1';
      this.C2 = 'USD';
      this.Typ = SymbolType.Currencies;
      this.Retired = false;
      return this;
    }

}

export class SymbolS {
    Id: number;
    MetasymbolId: number;
    Name: string;
    Description: string;
    Expiration: Date;
    Retired: boolean;

    createNew(parentId: number): SymbolS {
      this.Name = 'Undef';
      this.MetasymbolId = parentId;
      this.Description = 'No';
      this.Expiration = new Date(2100, 1, 1);
      this.Retired = false;
      return this;
    }

}

export class Rates {
    Id: number;
    MetaSymbol: string;
    Symbol: string;
    C1: string;
    C2: string;
    Ratebid: number;
    Rateask: number;
    Lastupdate: Date;
    Retired: false;
    createNew(): Rates {
      this.MetaSymbol = 'BTCUSD';
      this.Symbol = 'BTCUSD';
      this.C1 = 'BTC';
      this.C2 = 'USD';
      this.Ratebid = 1;
      this.Rateask = 1;
      this.Lastupdate = new Date();
      return this;
    }
}

export class ScheduledJobView {
    ID: number;
    PrevDate: Date;
    NextDate: Date;
    Group: string;
    Name: string;
    Schedule: string;
    IsRunning: boolean;
    Log: string;
}

export class JobParam {
  Group: string;
  Name: string;
}

export class Settings {
  Id: number;
  Propertyname: string;
  Value: string;
  Description: string;

  createNew(): Settings {
    this.Propertyname = 'Unnamed';
    this.Value = '0';
    this.Description = 'Description 1';
    return this;
  }
}

export class LogItem {
  Name: string;
  TabTitle: string;
  DataSource: string;
  TextChangedEvent: string;
  Theme: string;
  Path: string;
}

