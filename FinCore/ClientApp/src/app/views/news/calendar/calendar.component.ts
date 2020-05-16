import {Component, OnInit, ViewChild, ChangeDetectionStrategy, ViewEncapsulation } from '@angular/core';
import { NewsService} from '../../../services/news.service';
import { DxSchedulerComponent } from 'devextreme-angular';

export class Priority {
  text: string;
  id: number;
  color: string;
}
@Component({
  templateUrl: 'calendar.component.html',
  styleUrls: ['../../../../scss/vendors/angular-calendar/angular-calendar.scss']
})
export class CalendarComponent implements OnInit  {
  // @ViewChild(DxSchedulerComponent) scheduler: DxSchedulerComponent;

  public dataSource: any;
  viewDate: Date;
  startDate: Date;
  startHour: number;

  public importanceData: Priority[] = [
    {
        text: 'Low Priority',
        id: 0,
        color: '#1e90ff'
    }, {
        text: 'Medium Priority',
        id: 1,
        color: '#ff9747'
    },
    {
      text: 'High Priority',
      id: 2,
      color: '#ff0000'
    }
];

  constructor(public news: NewsService) {
    this.viewDate = new Date();
    this.startDate = this.viewDate;
    this.viewDate.setDate(this.viewDate.getDate());

    this.startHour = - this.viewDate.getTimezoneOffset() / 60;
    console.log('Minus Timezone offset in hours. Start hour: ' + this.startHour);
  }

  loadData() {
    this.news.getTodayNews(this.viewDate, 'ALL', 0)
      .subscribe(
          data => {
            this.dataSource = data;
          },
          error => {
              const message = JSON.stringify( error.error) + '\n' + error.statusText;
              console.log(message);
          });
}

ngOnInit() {
  this.loadData();
}

onCellClick() {
  //this.startDate = this.scheduler.instance.getStartViewDate();
  //if (this.startDate.toLocaleDateString() !== this.viewDate.toLocaleDateString()) {
  //  this.viewDate = this.startDate;
  //  this.loadData();
  //}
}


}
