import { Component, OnInit, OnDestroy } from '@angular/core';
import { WalletsService } from '../../../services/wallets.service';
import notify from 'devextreme/ui/notify';
import { TimeStat, SelectMonth, IWebsocketCallback, WsMessage, WsMessageType } from '../../../models/Entities';
import { BaseComponent } from '../../../base/base.component';
import { WebsocketService } from '../../../services/websocket.service';
import CustomStore from 'devextreme/data/custom_store';
import { DeepPartial } from 'devextreme/core';

let currMonth = 0;

@Component({
  templateUrl: 'performance.component.html',
  styleUrls: ['performance.component.scss']
})
export class PerformanceComponent extends BaseComponent implements OnInit, OnDestroy, IWebsocketCallback  {
  dataSource: Array<TimeStat>;
  currentMonth: number;
  store: CustomStore;
  public chartLoaded: boolean;

  public Months: SelectMonth[] = [{
    id: 0,
    name: 'January',
    value: 0
    },
    {
    id: 1,
    name: 'February',
    value: 1
    },
    {
      id: 2,
      name: 'March',
      value: 2
    },
    {
      id: 3,
      name: 'April',
      value: 3
    },
    {
      id: 4,
      name: 'May',
      value: 4
    },
    {
      id: 5,
      name: 'June',
      value: 5
    },
    {
      id: 6,
      name: 'July',
      value: 6
    },
    {
      id: 7,
      name: 'August',
      value: 7
    },
    {
      id: 8,
      name: 'September',
      value: 8
    },
    {
      id: 9,
      name: 'October',
      value: 9
    },
    {
      id: 10,
      name: 'November',
      value: 10
    },
    {
      id: 11,
      name: 'December',
      value: 11
    }
    ];

  get MonthlyGains(): number {
    if (!this.chartLoaded) {
      return 0;
    }
    let result = 0.0;
    this.dataSource.forEach( el => {
      result += el.Gains;
    });
    return result;
  }

  get MonthlyLosses(): number {
    if (!this.chartLoaded) {
      return 0;
    }
    let result = 0.0;
    this.dataSource.forEach(el => {
      result += el.Losses;
    });
    return result;
  }

  get MonthlyResult(): string {
    const result = this.MonthlyGains - this.MonthlyLosses;
    return result.toFixed(2);
  }

  constructor(public wallet: WalletsService, public ws: WebsocketService) {
    super();
    this.chartLoaded = false;
    const now: Date = new Date();
    this.currentMonth = now.getMonth();
  }

  ngOnInit() {
    const now: Date = new Date();
    this.currentMonth = now.getMonth();
    currMonth = this.currentMonth;
    this.loadData();
  }

  ngOnDestroy(): void {
    this.ws.doDisconnect();
    super.ngOnDestroy();
  }

  loadData() {
    this.chartLoaded = false;
    this.dataSource = new Array<TimeStat>();
    this.ws.doConnect(this);

  }

  customizeTooltip(arg) {
    const now = new Date();
    let year: number = now.getFullYear();
    if (now.getMonth() < currMonth) {
        year = year - 1;
    }
    const dateVal: string = new Date(year, currMonth,  arg.argument).toLocaleString('en-us', {  weekday: 'long' });
    const resultString: string = arg.valueText + '$/' + arg.argument + 'th ' + dateVal;
    return {
      text: resultString
    };
  }

  onValueChanged(data) {
    this.currentMonth = data.value;
    currMonth = this.currentMonth;
    this.loadData();
  }

  customizePoint(arg) {
    if (arg.value === 0) {
      return {
        visible: false
      };
    }
    if (arg.series.pane === 'topPane') {
      if (arg.data.InvestingChange > 0) {
        return { color: '#00cc00', hoverStyle: { color: '#00cc00' } };
      }
    }
    if (arg.series.pane === 'bottomPane') {
      if (arg.data.CheckingChange > 0) {
        return { color: '#00cc00', hoverStyle: { color: '#00cc00' } };
      }
    }
  }

  customizeLabel(arg) {
    if (arg.value === 0) {
      return {
        visible: false
      };
    }
    if (arg.series.pane === 'topPane') {
      let value = 0;
      if (arg.data.InvestingValue > 0) {
          value = (arg.data.InvestingChange / arg.data.InvestingValue) * 100;
      }
      if (arg.data.InvestingChange > 0) {
          return {
              visible: true,
              customizeText: function (e: any) {
                return e.valueText + '\n' + value.toFixed(2) + '%';
              },
              backgroundColor: '#00cc00'
          };
      } else  {
        return {
          visible: true,
          customizeText: function (e: any) {
            return e.valueText + '\n' + value.toFixed(2) + '%';
          }
      };

      }
    }
    if (arg.series.pane === 'bottomPane') {
      if (arg.data.CheckingChange > 0) {
          return {
              visible: true,
              backgroundColor: '#00cc00'
          };
      }
    }
  }

  public onOpen(evt: MessageEvent) {
    console.log('connected performance\n');
    this.ws.doSend({ Type: WsMessageType.GetAllPerformance, From: this.wallet.currentUserToken.userName,
      Message: this.currentMonth.toString() });
  }

  public onClose() {
    console.log('disconnected performance\n');
  }

  public onMessage(msg: WsMessage) {
    if (msg) {
      switch (msg.Type) {
        case WsMessageType.GetAllPerformance:
        {
          const data: TimeStat[] = JSON.parse(msg.Message);
          this.chartLoaded = false;
          this.store = new CustomStore({
            load: () => data,
            key: 'X'
          });
        }
        break;
        case WsMessageType.ChartValue:
        {
          const data: TimeStat = JSON.parse(msg.Message);
          const dp_data: DeepPartial<TimeStat> = data as DeepPartial<TimeStat>;;
          if (this.store && data) {
            this.dataSource.push(data);
            this.store.push([
              {
                type: 'insert',
                key: data.X,
                data: dp_data
              }
            ]);
          }
        }
        break;
        case WsMessageType.ChartDone:
          {
            this.ws.doDisconnect();
            this.chartLoaded = true;
          }
        break;
      }
    }
  }

  public onError(evt: MessageEvent) {
    const webS = evt.currentTarget as WebSocket;
    if (webS)
      notify('WebSocket Connection Error: ' + webS.url);
    else
      notify('WebSocket Connection Error: ' + evt.currentTarget);
    this.ws.doDisconnect();
  }

}

