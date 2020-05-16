import { Component, OnInit, AfterViewInit, ViewChild, ElementRef, Inject, Input } from '@angular/core';
import { DefaultLayoutComponent } from '../../containers';

declare const TradingView: any;

@Component({
  selector: 'app-tview',
  templateUrl: 'tview.component.html',
  styleUrls: ['tview.component.css']
})
export class TviewComponent implements OnInit, AfterViewInit {

  @Input() symbol: string;

  public height: number;

  constructor(@Inject(DefaultLayoutComponent) private parentView: DefaultLayoutComponent) {
    this.height =  this.parentView.mainView.nativeElement.offsetHeight;
    // console.log('MainView height: ' + this.height + ', Symbol: ' + this.symbol);
  }

  ngOnInit(): void {

  }

  ngAfterViewInit() {

    // tslint:disable-next-line: no-unused-expression
    new TradingView.widget({
      'container_id': 'technical-analysis',
      'autosize': true,
      'symbol': this.symbol,
      'interval': '60',
      'timezone': 'exchange',
      'theme': 'Light',
      'style': '1',
      'toolbar_bg': '#f1f3f6',
      'withdateranges': true,
      'hide_side_toolbar': false,
      'allow_symbol_change': true,
      'save_image': false,
      'hideideas': true,
      'studies': [
      'MASimple@tv-basicstudies' ],
      'show_popup_button': true
    });
  }

}
