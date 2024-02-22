import { Component, Input, OnInit, ViewChild, OnDestroy, AfterViewInit } from '@angular/core';
import {
  UserToken,
  PositionInfo,
  TodayStat,
  IWebsocketCallback,
  WsMessageType,
  WsMessage,
  BalanceInfo,
  ENUM_ORDERROLE
} from '../../models/Entities';
import { DealsService } from '../../services/deals.service';
import { WebsocketService } from '../../services/websocket.service';
import CustomStore from 'devextreme/data/custom_store';
import notify from 'devextreme/ui/notify';
import { BaseComponent } from '../../base/base.component';
import { DxDataGridComponent } from 'devextreme-angular';
import { PropertiesComponent } from '../tables/properties/properties.component';
import { DGaugeComponent } from '../dgauge/dgauge.component';

@Component({
  templateUrl: 'dashboard.component.html',
  styleUrls: ['dashboard.component.scss']
})
export class DashboardComponent extends BaseComponent implements OnInit, OnDestroy, IWebsocketCallback {
  @ViewChild('positionsContainer') positionsContainer: DxDataGridComponent;
  @ViewChild(PropertiesComponent) propsContainer: PropertiesComponent;
  @ViewChild('dgauge') dgauge: DGaugeComponent;
  @Input() sidebarId: string = "sidebar1";

  dataSource: CustomStore;
  connectionStarted: boolean;
  popupVisible = false;
  currentUser: UserToken;
  users: UserToken[] = [];
  stat: TodayStat;
  balances: string;
  currentObject: any;
  private timerId: any;

  _showProperties = false;

  set showProperties(val: boolean) {
    this._showProperties = val;
  }

  get showProperties(): boolean {
    return this._showProperties;
  }


  RoleString(role: any): string {
    switch (role) {
      case ENUM_ORDERROLE.RegularTrail:
      case '0':
          return 'RegularTrail';
      case ENUM_ORDERROLE.PendingSmartOrder:
      case '9':
        return 'PendingSmartOrder';
      case '3':
      case ENUM_ORDERROLE.PendingLimit:
        return 'PendingLimit';
      case '4':
      case ENUM_ORDERROLE.PendingStop:
          return 'PendingStop';
      case ENUM_ORDERROLE.RegularSmartOrder:
            case '10':
              return 'RegularSmartOrder';
      case ENUM_ORDERROLE.PendingPriceAlert:
      case '11':
        return 'PendingPriceAlert';
      default:
          return role.toString();
    }
  }

  constructor(public deals: DealsService, public ws: WebsocketService) {
    super();
    this.currentUser = JSON.parse(localStorage.getItem('currentUser'));
    this.connectionStarted = false;
    this.stat = new TodayStat();
    this.fillBalancesString();
    this.balances = ' loading...';
  }

  public onOpen(evt: MessageEvent) {
    this.UpdateDeals();
    this.connectionStarted = true;
    // console.log('connected dashboard\n');
    this.loadData();
  }

  loadData() {
    if (this.currentUser) {
      this.ws.doSend({ Type: WsMessageType.GetAllPositions, From: this.currentUser.userName, Message: '' });
      this.subs.sink = this.deals.runJob('SYSTEM', 'TerminalsSyncJob').subscribe(
        data => {
          window.location.reload();
        },
        error => this.logNotifyError(error)
      );
  }
  }

  titleGainsPercent(): string {
    if (this.stat.TodayGainRealPercent)
       return this.stat.TodayGainRealPercent.toFixed(2) + '%';
    return ' loading...';
  }

  public onClose() {
    this.propsContainer.close();
  }

  allowClosePositions(data): boolean {
    if (typeof data === 'string') {
      // return false;
      // console.log('should be disabled: ' + data);
      if (data === 'LongInvestment' || data === 'ShortInvestment') {
        return false;
      }
    }
    if (typeof data === 'number') {
      var d = data as ENUM_ORDERROLE;
      if (d === ENUM_ORDERROLE.LongInvestment || data === ENUM_ORDERROLE.ShortInvestment) {
        return false;
      }
    }

    return true;
  }

  public fillBalancesString() {
    if (this.stat.Accounts) {
        this.balances = this.stat.Accounts
            .filter(element => !element.Retired)
            .map(element => `${element.DailyProfitPercent.toFixed(2)}%=${element.Description}`)
            .join(':');
    }
  }


  public onMessage(msg: WsMessage) {
    if (msg) {
      switch (msg.Type) {
        case WsMessageType.GetAllPositions:
          {
            // console.log('Insert ' + msg.Message);
            const data = JSON.parse(msg.Message) as PositionInfo[];
            data.forEach(info => {
               info.RoleString = this.RoleString(info.Role);
            });
            if (data) {
              this.dgauge.updateData(data);
              this.timerId = setInterval(() => this.dgauge.updateData(data), 20000);
            }
            this.dataSource = new CustomStore({
              load: () => data,
              key: 'Ticket'
            });
          }
          break;
        case WsMessageType.UpdatePosition:
          {
            const data = JSON.parse(msg.Message) as PositionInfo;
            data.RoleString = this.RoleString(data.Role);
            // console.log('Update ' + msg.Message);
            if (this.dataSource) {
              this.dataSource.push([
                {
                  type: 'update',
                  key: data.Ticket,
                  data: data
                }
              ]);
            }
          }
          break;
          case WsMessageType.UpdateBalance:
          {
              const balance = JSON.parse(msg.Message) as BalanceInfo;
              if (balance && this.stat.Accounts) {
                var totalBalance = 0;
                this.stat.Accounts.forEach(element => {
                  if (balance.Account === element.Number && balance.Balance > 0) {
                    // console.log('Updated balance for account=' + balance.Account + 'to ' + balance.Balance);
                    element.Balance = balance.Balance;
                    element.Equity = balance.Equity;
                  }
                  if (element.Balance > 0)
                  totalBalance+=element.Balance;
                });
                this.stat.TodayGainRealPercent = this.stat.TodayGainReal/totalBalance*100.0;
                this.fillBalancesString();
              }
          }
          break;
        case WsMessageType.InsertPosition:
          {
            const data = JSON.parse(msg.Message) as PositionInfo;
            data.RoleString = this.RoleString(data.Role);
            console.log('Insert ' + data);
            if (this.dataSource) {
              this.dataSource.push([
                {
                  type: 'insert',
                  data: data
                }
              ]);
            }
          }
          break;
        case WsMessageType.RemovePosition:
          {
            const data = JSON.parse(msg.Message);
            console.log('Remove ' + data);
            if (this.dataSource) {
              this.dataSource.push([
                {
                  type: 'remove',
                  key: data
                }
              ]);
            }
            this.UpdateDeals();
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

  override ngOnInit() {
    this.ws.doConnect(this);
  }

  getCurrentTitle(): string {
    if (this.propsContainer && this.propsContainer.entityName) {
      return `${this.propsContainer.entityName} Properties`;
    }
    return 'Properties';
  }

  override ngOnDestroy(): void {
    clearInterval(this.timerId);
    this.ws.doDisconnect();
    super.ngOnDestroy();
  }

  public UpdateDeals() {
    this.subs.sink = this.deals.getTodayStat(false).subscribe(
      data => {
        this.stat = data;
      },
      error => this.logConsoleError(error)
    );
  }

  public onClickCell(e) {
    const id: number = e.columnIndex;
    if (id === 1) {
      const p: PositionInfo = e.data;
      if (p) {
        this.currentObject = p;
        this.propsContainer.setData(p.Ticket, 'Position', p);
      }
      return;
    }

    if (id === 5) {
      const pos: PositionInfo = e.data;
      if (this.allowClosePositions(pos.Role)) {
        this.subs.sink = this.deals.closePosition(pos.Account, pos.Magic, pos.Ticket).subscribe(
          data => {
            console.log('Position close request sent ticket=' + pos.Ticket);
          },
          error => this.logConsoleError(error)
        );
        return;
      }
    }
  }

  public refreshAll(e) {
    this.subs.sink = this.deals.refreshAll().subscribe(
      data => {
        window.location.reload();
      },
      error => this.logConsoleError(error)
    );
  }

  public syncAll(e) {
    this.loadData();
    //this.subs.sink = this.deals.runJob('SYSTEM', 'TerminalsSyncJob').subscribe(
    //  data => {
    //    window.location.reload();
    //  },
    //  error => this.logNotifyError(error)
    //);
  }

  onToolbarPreparing(e) {
    e.toolbarOptions.items.unshift(
      {
        location: 'before',
        template: 'totalStat'
      },
      {
        location: 'after',
        widget: 'dxButton',
        options: {
          width: 100,
          icon: 'refresh',
          onClick: this.syncAll.bind(this)
        }
      },
      {
        location: 'after',
        widget: 'dxButton',
        options: {
          width: 100,
          text: 'Refresh',
          onClick: this.refreshAll.bind(this)
        }
      }
    );
  }

  toDBObj(key: number): any {
    if (this.currentObject) {
      const entries = new Map<string, any>();
      Object.keys(this.currentObject).forEach(element => {
        entries.set(element, this.propsContainer.dataSource[element].value);
      });
      entries.set('Id', key);
      this.currentObject = Object.fromEntries(entries);
      // console.log(this.currentObject);
    }
    return this.currentObject;
  }

  symbolClick(sym) {
    window.location.href = this.deals.externChartURL(sym);
  }

  onSave() {
    const key = this.propsContainer.objId;
    if (key) {
      const pos = JSON.stringify(this.toDBObj(key));
      // console.log(pos);
      this.ws.doSend({ Type: WsMessageType.UpdatePosition, From: this.deals.currentUserToken.userName, Message: pos });

      this.propsContainer.updateProperty(true, true);
      this.loadData();
    }
  }
}
