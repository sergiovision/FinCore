import { Component, OnInit } from '@angular/core';
import { DealsService } from '../../../services/deals.service';
import { MetaSymbolStat } from '../../../models/Entities';
import { BaseComponent } from '../../../base/base.component';

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

    this.subs.sink = this.deals.getStat(this.currentAccountType)
      .subscribe(
          data => {
            this.dataSource = data;
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
