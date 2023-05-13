import { Component, OnInit } from '@angular/core';
import { DealsService } from '../../../services/deals.service';
import { MetaSymbolStat } from '../../../models/Entities';
import { BaseComponent } from '../../../base/base.component';
import { SelectOption} from '../../../models/Entities';

let currOption = 0;

@Component({
  templateUrl: 'symbols.component.html',
  styleUrls: ['symbols.component.scss']
})
export class SymbolsComponent extends BaseComponent implements OnInit  {
  dataSource: MetaSymbolStat[];
  loadingVisible: boolean;
  currentOption: number;

  public SymbolOptions: SelectOption[] = [{
      id: 0,
      name: 'All Latest Symbols',
      value: 0
    },
    {
      id: 1,
      name: 'Selected Symbols',
      value: 1
    }];

  constructor(public deals: DealsService) {
    super();
    this.loadingVisible = true;
    this.currentOption = 1;
  }

  loadData() {
    this.loadingVisible = true;

    const nElements = 18;

    this.subs.sink = this.deals.getStat(nElements, this.currentOption)
      .subscribe(
          data => {
            this.dataSource = data.slice(0, nElements);
            // console.log(this.dataSource);
            this.loadingVisible = false;
          },
          error => this.logConsoleError(error));
  }

  override ngOnInit() {
    this.loadData();
  }

  customizeTooltip(arg) {
    return {
      text: arg.valueText + '$ / ' + arg.argumentText
    };
  }

  onValueChanged(data) {
    this.currentOption = data.value;
    currOption = this.currentOption;
    this.loadData();
  }

}
