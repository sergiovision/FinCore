import { Component, OnInit } from '@angular/core';
import notify from 'devextreme/ui/notify';
import { BaseComponent } from '../../../base/base.component';
import { ExpertsService } from '../../../services/experts.service';
import CustomStore from 'devextreme/data/custom_store';

@Component({
  templateUrl: './jobs.component.html',
  styleUrls: ['./jobs.component.scss']
})
export class JobsComponent extends BaseComponent implements OnInit {
  dataSource: any;
  constructor(public experts: ExpertsService) {
    super();
  }
  loadData() {
    /* this.dataSource = new CustomStore({
      key: 'Id',
      load: () => this.experts.loadParentData(EntitiesEnum.Jobs)
                  .toPromise()
                  .then((data: any) => {
                      this.dataSource = data.filter(item => !item.Retired);

                  })
                  .catch(error => this.logNotifyError(error)),
    });*/

      this.subs.sink = this.experts.getAll('/api/jobs')
        .subscribe(
            data => {
              // this.dataSource = query(data).filter(['Disabled', '==', '0']).toArray();
              this.dataSource = data;
            },
            error => this.logConsoleError(error));

  }

  override ngOnInit() {
    this.loadData();
  }

   public getDate(regDate: string) {
      const date = new Date(regDate);
      return date.toLocaleDateString('en-US', {year: 'numeric', month: 'short', day: '2-digit'});
   }

   public onClickCell(e) {
     const id: number = e.columnIndex;
     if (id === 5) {
        const data: any = e.data;
        this.subs.sink = this.experts.runJob(data.Group, data.Name)
        .subscribe(
          // tslint:disable-next-line:no-shadowed-variable
          data => {
            console.log(data);
            // notify(data);
            window.location.reload();
          },
          error => this.logConsoleError(error));


     }
     if (id === 6) {
      const data: any = e.data;
      this.subs.sink = this.experts.stopJob(data.Group, data.Name)
        .subscribe(
          // tslint:disable-next-line:no-shadowed-variable
          data => {
          console.log(data);
          // notify(data);
          window.location.reload();

        },
        error => this.logConsoleError(error));
     }
   }

}
