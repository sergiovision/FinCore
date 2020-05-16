import { Component, OnInit, AfterViewInit } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';

@Component({
  selector: 'app-chart',
  templateUrl: 'chart.component.html',
  styleUrls: ['chart.component.css']
})
export class ChartComponent implements OnInit, AfterViewInit {

  public symbol: string;

  constructor(private route: ActivatedRoute) {
    this.symbol = 'BTCUSD';
  }

  ngOnInit(): void {

    const sym = this.route.snapshot.queryParams['symbol'];
    console.log('Symbol: ' + sym);
    if (sym) {
      this.symbol = sym;
    }
  }

  ngAfterViewInit() {
  }
}
