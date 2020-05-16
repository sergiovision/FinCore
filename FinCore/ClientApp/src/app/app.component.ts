import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { BaseComponent } from './base/base.component';

@Component({
  // tslint:disable-next-line
  selector: 'body',
  template: '<router-outlet></router-outlet>'
})
export class AppComponent extends BaseComponent implements OnInit {
  // private script: any;

  constructor(private router: Router) {
    super();

    // this.script = document.createElement('script');
    // this.script.src = 'https://s3.tradingview.com/tv.js';

  }

  ngOnInit() {

    this.subs.sink = this.router.events.subscribe((evt: any ) => {
/*
     if (evt.url) {
        if ( evt.url.indexOf('/chart') !== -1) {
          console.log(' app event: ' + evt.url);
          document.body.appendChild(this.script);
        }
      }
      */

      if (!(evt instanceof NavigationEnd)) {
        return;
      }
      window.scrollTo(0, 0);
    });
  }
}
