import { Component, OnInit, ViewChild, OnDestroy, AfterViewInit } from '@angular/core';
import { UserToken, PositionInfo, TodayStat, IWebsocketCallback, WsMessageType, WsMessage } from '../../models/Entities';
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

  dataSource: CustomStore;
  connectionStarted: boolean;
  popupVisible = false;
  currentUser: UserToken;
  users: UserToken[] = [];
  stat: TodayStat;
  currentObject: any;
  private timerId: any;

  _showProperties = false;

  set showProperties(val: boolean) {
    this._showProperties = val;
  }

  get showProperties(): boolean {
    return this._showProperties;
  }

  constructor(public deals: DealsService, public ws: WebsocketService) {
    super();
    this.currentUser = JSON.parse(localStorage.getItem('currentUser'));
    this.connectionStarted = false;
    this.stat = new TodayStat();
  }

  public onOpen(evt: MessageEvent) {

    this.UpdateDeals();
    this.connectionStarted = true;
    // console.log('connected dashboard\n');
    this.loadData();
  }

  loadData() {
    this.ws.doSend({ Type: WsMessageType.GetAllPositions, From: this.deals.currentUserToken.userName, Message: '' });
  }

  public onClose() {
    this.propsContainer.close();
  }

  allowClosePositions(data): boolean {
    if (typeof data === 'string') {
      // return false;
      // console.log('should be disabled: ' + data);
      if ((data === 'LongInvestment')  || (data === 'ShortInvestment')) {
         return false;
      }
    }
    return true;
  }

  public onMessage(msg: WsMessage) {
    if (msg) {
      switch (msg.Type) {
        case WsMessageType.GetAllPositions:
        {
          // console.log('Insert ' + msg.Message);
          const data = JSON.parse(msg.Message);
          this.dataSource = new CustomStore({
            load: () => data,
            key: 'Ticket'
          });
          this.dgauge.updateData(data);
          this.timerId = setInterval(() => this.dgauge.updateData(data), 20000);
        }
          break;
        case WsMessageType.UpdatePosition:
        {
          const data = JSON.parse(msg.Message);
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
        case WsMessageType.InsertPosition:
        {
            const data = JSON.parse(msg.Message);
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
    notify('Connection Error: ' + evt.data);
    this.ws.doDisconnect();
  }

  ngOnInit() {
    this.ws.doConnect(this);
  }

  getCurrentTitle(): string {
    if (this.propsContainer && this.propsContainer.entityName) {
      return `${this.propsContainer.entityName} Properties`;
    }
    return 'Properties';
  }


  ngOnDestroy(): void {
    clearInterval(this.timerId);
    this.ws.doDisconnect();
    super.ngOnDestroy();
  }

  public UpdateDeals() {
    this.subs.sink = this.deals.getTodayStat().subscribe(
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

    if (( id === 7)) {
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
    this.subs.sink = this.deals.runJob('SYSTEM', 'TerminalsSyncJob').subscribe(
      data => {
        window.location.reload();
      },
      error => this.logNotifyError(error)
    );
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
          width: 120,
          icon: 'refresh',
          onClick: this.syncAll.bind(this)
        }
      },
      {
        location: 'after',
        widget: 'dxButton',
        options: {
          width: 120,
          text: 'Refresh All',
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

        /* this.subs.sink = this.deals.updateStore(this.propsContainer.entityType, key, this.toDBObj(key)).subscribe(
          data => {
          }
          , error => this.logConsoleError(error));
          */
    }
    /*
    else {
        console.log(this.propsContainer.dataSource);
        this.subs.sink = this.deals.pushStore(this.propsContainer.entityType, this.toDBObj(key)).subscribe(
        data => {
          console.log('Added successfully: ' + this.propsContainer.entityType.toString() + ', Id=' + data);
          this.currentObject.Id = data;
          this.propsContainer.parentVisibleChange.emit(false);
        }
        , error => this.logConsoleError(error));
    }
    */

  }

}
