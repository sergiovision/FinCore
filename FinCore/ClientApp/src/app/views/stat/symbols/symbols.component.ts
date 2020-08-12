import { Component, OnInit } from '@angular/core';
import { DealsService } from '../../../services/deals.service';
import { MetaSymbolStat } from '../../../models/Entities';
import { BaseComponent } from '../../../base/base.component';
// import DevExpress from 'devextreme/bundles/dx.all';

export class SelectAccountType {
  id: number;
  name: string;
  value: number;
}

@Component({
  templateUrl: 'symbols.component.html',
  styleUrls: ['symbols.component.scss']
})
export class SymbolsComponent extends BaseComponent implements OnInit  {
  dataSource: MetaSymbolStat[];
  currentAccountType: number;
  loadingVisible: boolean;

  public AccountType: SelectAccountType[] = [{
    id: 0,
    name: 'Real',
    value: 0
    }, {
    id: 1,
    name: 'Demo',
    value: 1,
    }];


  constructor(public deals: DealsService) {
    super();
    this.loadingVisible = true;
    this.currentAccountType = 0;
  }

  loadData() {
    this.loadingVisible = true;

    const nElements = 18;
    // console.log(DevExpress.devices);
    // const device = DevExpress.devices.current.name;
    // console.log('Device: ' + device);
    // if (device === 'desktop') {
    //  nElements = 20;
    // } else  {
    //  nElements = 10;
    // }

    this.subs.sink = this.deals.getStat(this.currentAccountType)
      .subscribe(
          data => {
            this.dataSource = data.slice(0, nElements);
            // console.log(this.dataSource);
            this.loadingVisible = false;
          },
          error => this.logConsoleError(error));
  }

  ngOnInit() {
    this.loadData();
  }

  customizeTooltip(arg) {
    return {
      text: arg.valueText + '$ / ' + arg.argumentText
    };
  }

  onValueChanged(data) {
    this.currentAccountType = data.value;
    this.loadData();
  }

}
