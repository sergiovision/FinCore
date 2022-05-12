import { Component, OnInit, ViewChild, AfterViewInit, OnDestroy } from '@angular/core';
import { WalletsService } from '../../../services/wallets.service';
import { DxChartComponent } from 'devextreme-angular';
import { Wallet, SelectYear, IWebsocketCallback, WsMessageType, WsMessage } from '../../../models/Entities';
import { BaseComponent } from '../../../base/base.component';
import { WebsocketService } from '../../../services/websocket.service';
import CustomStore from 'devextreme/data/custom_store';
import notify from 'devextreme/ui/notify';
import { DeepPartial } from 'devextreme/core';

@Component({
  templateUrl: 'capital.component.html',
  styleUrls: ['capital.component.scss']
})

export class CapitalComponent extends BaseComponent implements OnInit, OnDestroy, IWebsocketCallback  {

  @ViewChild('walletChart') walletChart: DxChartComponent;

  store: CustomStore;
  dataSource: Array<Wallet>;
  fromDate: Date;
  toDate: Date;
  currentYearIndex: number;
  chartLoaded: boolean;
  public years: Array<SelectYear>;

  public getYears()  {
    let arr = new Array<SelectYear>();
    var d = new Date(Date.now());
    var cY = d.getFullYear();
    let i = 0;
    for (let index = cY; index >= 2015; index--) {
      const yearX:number = index;
      arr.push( {
        id: i,
        name: yearX.toString(),
        valueFrom: new Date(yearX, 0, 2),
        valueTo: (i===0)?d:new Date(yearX, 11, 31)
      });
      i++;
    }
    return arr;
  }

  constructor(public wallet: WalletsService, public ws: WebsocketService) {
    super();
    this.currentYearIndex = 0;
    this.years = this.getYears();
    this.fromDate = this.years[this.currentYearIndex].valueFrom;
    this.toDate = this.years[this.currentYearIndex].valueTo;
  }

  loadData() {

    this.chartLoaded = false;
    this.dataSource = new Array<Wallet>();
    this.ws.doConnect(this);
  }

  ngOnInit() {
    this.loadData();
  }

  ngOnDestroy(): void {
    this.ws.doDisconnect();
    super.ngOnDestroy();
  }

  customizeTooltip(arg) {
    const price: number = arg.value;
    const d: Date = arg.originalArgument;
    return {
      text: price.toFixed(2) + '$ / ' + d.toDateString()
    };
  }

  public customizeTitle() {
    return 'Capital ' + this.years[this.currentYearIndex].name;
  }

  onValueChanged(data) {
    this.currentYearIndex = data.value;
    this.fromDate = this.years[this.currentYearIndex].valueFrom;
    this.toDate = this.years[this.currentYearIndex].valueTo;
    this.loadData();
  }

  public onOpen(evt: MessageEvent) {
    console.log('connected capital\n');
    const params = {
      WalletId: 0,
      from: this.wallet.transformDate(this.fromDate),
      to: this.wallet.transformDate(this.toDate)
    };
    this.ws.doSend({ Type: WsMessageType.GetAllCapital, From: this.wallet.currentUserToken.userName,
      Message: JSON.stringify(params)});
  }

  public onClose() {
    console.log('disconnected capital\n');
  }

  public onMessage(msg: WsMessage) {
    if (msg) {
      switch (msg.Type) {
        case WsMessageType.GetAllCapital:
        {
          const data: Wallet[] = JSON.parse(msg.Message);
          this.chartLoaded = false;
          this.store = new CustomStore({
            load: () => data,
            key: 'Date'
          });
        }
        break;
        case WsMessageType.ChartValue:
        {
          const data: Wallet = JSON.parse(msg.Message);
          if (this.store && data) {
            this.dataSource.push(data);
            const dp_data: DeepPartial<Wallet> = data as DeepPartial<Wallet>;
            this.store.push([
              {
                type: 'insert',
                key: data.Date,
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
    notify('Connection Error: ' + evt);
    this.ws.doDisconnect();
  }


}
