import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { SubSink } from 'subsink';
import notify from 'devextreme/ui/notify';

@Component({
  selector: 'app-base',
  template: `NO UI TO BE FOUND HERE!`
})
export class BaseComponent implements OnInit, OnDestroy {
  protected subs: SubSink = new SubSink();

  protected showRetired = false;

  // Overridable hook: subclasses call super.ngOnInit().
  // eslint-disable-next-line @angular-eslint/no-empty-lifecycle-method, @typescript-eslint/no-empty-function
  ngOnInit() {
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  logNotifyError(error: any) {
    const message = JSON.stringify(error);
    console.log(message);
    notify(message);
  }

  logConsoleError(error: any) {
    const message = error;
    console.log(message);
  }

}
