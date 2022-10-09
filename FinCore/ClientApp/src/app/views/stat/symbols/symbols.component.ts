import { Component, OnInit } from '@angular/core';
import { DealsService } from '../../../services/deals.service';
import { MetaSymbolStat } from '../../../models/Entities';
import { BaseComponent } from '../../../base/base.component';

@Component({
  templateUrl: 'symbols.component.html',
  styleUrls: ['symbols.component.scss']
})
export class SymbolsComponent extends BaseComponent implements OnInit  {
  dataSource: MetaSymbolStat[];
  loadingVisible: boolean;

  constructor(public deals: DealsService) {
    super();
    this.loadingVisible = true;
  }

  loadData() {
    this.loadingVisible = true;

    const nElements = 18;

    this.subs.sink = this.deals.getStat(nElements)
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
    this.loadData();
  }

}
