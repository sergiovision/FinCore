import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { BaseComponent } from './base/base.component';

@Component({
  selector: 'body',
  template: '<router-outlet></router-outlet>'
})
export class AppComponent extends BaseComponent implements OnInit {

  constructor(private router: Router) {
    super();


  }

  override ngOnInit() {

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
