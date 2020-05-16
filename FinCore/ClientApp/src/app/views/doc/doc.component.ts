import { Component, OnInit, Inject } from '@angular/core';
import { BaseComponent } from '../../base/base.component';
import { LogsService } from '../../services/logs.service';
import { DefaultLayoutComponent } from '../../containers';


@Component({
  selector: 'app-doc',
  templateUrl: 'doc.component.html',
  styleUrls: ['doc.component.css']
})
export class DocComponent extends BaseComponent implements OnInit {

  version: string;
  width: number;

  constructor(public logs: LogsService, @Inject(DefaultLayoutComponent) private parentView: DefaultLayoutComponent) {
    super();
    this.width = this.parentView.mainView.nativeElement.offsetWidth * 0.8;
  }

  ngOnInit(): void {
    this.loadData();
  }

  loadData() {
    this.subs.sink = this.logs.GetGlobalProp('VERSION')
    .subscribe(
      data => {
        const str = 'Version: ' + data.version;
        console.log(str);
        this.version = str;
      },
      error => this.logConsoleError(error));
  }

}
