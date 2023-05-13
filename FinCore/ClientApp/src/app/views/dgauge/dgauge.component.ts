import { Component, OnInit, Input, ViewChild } from '@angular/core';
import { PositionInfo } from '../../models/Entities';
import { BaseComponent } from '../../base/base.component';
import { ExpertsService } from '../../services/experts.service';

let gainers: PositionInfo[];
let loosers: PositionInfo[];

@Component({
  selector: 'app-dgauge',
  templateUrl: 'dgauge.component.html',
  styleUrls: ['dgauge.component.scss']
})
export class DGaugeComponent extends BaseComponent implements OnInit {

  @ViewChild('gaugeGainers') gGainers: DGaugeComponent;
  @ViewChild('gaugeLoosers') gLoosers: DGaugeComponent;

  currentAccountType: number;
  loadingVisible: boolean;
  maxGain: number;
  sumGain: number;
  maxLoose: number;
  sumLoose: number;
  GainersProfits: number[];
  LoosersProfits: number[];

  constructor(public experts: ExpertsService) {
    super();
    this.loadingVisible = true;
    this.currentAccountType = 0;
    this.sumGain = 0;
    this.sumLoose = 0;

  }

  loadData() {
    // this.subs.sink = this.experts.GetGlobalProp('POSITIONS')
    //  .subscribe(data => this.updateData(data), error => this.logConsoleError(error));
  }

  updateData(data: PositionInfo[]) {
      // console.log('Update data');
      this.loadingVisible = true;
      gainers = [];
      loosers = [];
      this.GainersProfits = [];
      this.LoosersProfits = [];
      this.maxGain = 0;
      this.maxLoose = 0;
      this.sumGain = 0;
      this.sumLoose = 0;
      Object.entries(data).forEach(item => {
        const pos: any = item[1];
        if (pos.Profit > 0) {
          gainers.push(pos);
          this.maxGain = Math.max(this.maxGain, pos.Profit);
          this.sumGain += pos.Profit;
        }
        if (pos.Profit < 0) {
          loosers.push(pos);
          this.maxLoose = Math.min(this.maxLoose, pos.Profit);
          this.sumLoose += pos.Profit;
        }
      });
      gainers = gainers.sort((a, b) =>  (a.Profit > b.Profit ? 1 : -1));
      loosers = loosers.sort((a, b) =>  (a.Profit > b.Profit ? -1 : 1));
      gainers.forEach(item => {
          this.GainersProfits.push(item.Profit);
      });
      loosers.forEach(item => {
        this.LoosersProfits.push(item.Profit);
      });
      this.loadingVisible = false;
  }

  override ngOnInit() {
    this.loadData();
  }

  titleGainers(): string {
    return 'Gainers: ' + this.sumGain.toFixed(1);
  }

  titleLoosers(): string {
    return 'Loosers: ' + this.sumLoose.toFixed(1);
  }

  customizeTextGainers(arg) {
    // console.log('data ready: ' + this.loadingVisible);
    // console.log(dataSource);
    if (gainers) {
      if (gainers[arg.index]) {
        const sym: string = gainers[arg.index].Symbol;
        return sym + ' ' + arg.value.toFixed(1);
      }
    }
    return arg.valueText;
  }

  customizeTextLoosers(arg) {
    // console.log('data ready: ' + this.loadingVisible);
    // console.log(dataSource);
    if (loosers) {
      if (loosers[arg.index]) {
        const sym: string = loosers[arg.index].Symbol;
        return sym + ' ' + arg.value.toFixed(1);
      }
    }
    return arg.valueText;
  }

}

