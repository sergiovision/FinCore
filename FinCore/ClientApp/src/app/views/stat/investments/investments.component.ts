import { Component, OnInit } from '@angular/core';
import { DealsService } from '../../../services/deals.service';
import { PositionInfo } from '../../../models/Entities';
import { BaseComponent } from '../../../base/base.component';
import { element } from 'protractor';
import { ExpertsService } from '../../../services/experts.service';

@Component({
  templateUrl: 'investments.component.html',
  styleUrls: ['investments.component.scss']
})
export class InvestmentsComponent extends BaseComponent implements OnInit  {
  dataSource: PositionInfo[];
  currentAccountType: number;
  loadingVisible: boolean;
  totalSum: number;

  constructor(public experts: ExpertsService) {
    super();
    this.loadingVisible = true;
    this.currentAccountType = 0;
  }

  loadData() {
    this.loadingVisible = true;

    this.subs.sink = this.experts.GetGlobalProp('POSITIONS')
      .subscribe(
          data => {
            this.dataSource = [];
            this.totalSum = 0;
            Object.entries(data).forEach(item => {
              const pos: any = item[1];
              const val: PositionInfo = pos;
              if (val.Role === 'LongInvestment' || val.Role === 'ShortInvestment') {
                // val.Value = val.Lots * val.Openprice * val.contractSize + val.Profit;
                this.totalSum += val.Value;
                this.dataSource.push(pos);
              }
            });
            this.loadingVisible = false;
          },
          error => this.logConsoleError(error));
  }

  calcTitle(): string {
    const t = 'Total Positions $USD';
    if ((this.totalSum) && (typeof this.totalSum === 'number')) {
      return t + ': ' + this.totalSum.toFixed(2);
    }
    return t;
  }

  ngOnInit() {
    this.loadData();
  }

  customizePieLabel(arg) {
    const value = arg.value;
    return value.toFixed(2) + ' (' + arg.percentText + ')';
  }

}
