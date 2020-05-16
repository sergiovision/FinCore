import { BaseService } from './base.service';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import notify from 'devextreme/ui/notify';
import { AccountState, Terminal } from '../models/Entities';

@Injectable()
export class WalletsService extends BaseService {
    constructor(http: HttpClient) { super(http); }

    public getAll() {
      return super.getAll('/api/wallets');
    }

    public genDeployScripts() {
      return super.getAll('/api/experts/GenerateDeployScripts')
      .subscribe(
        data => {
          console.log(data);
          notify(data);
        },
        error => {
            const message = JSON.stringify(error);
            console.log(message);
            notify(message);
        });
    }

    public deployTerminal(id: number) {
      return super.getAll('/api/experts/DeployScript/' + id)
      .subscribe(
        data => {
          console.log(data);
          notify(data);
        },
        error => {
            const message = JSON.stringify(error);
            console.log(message);
            notify(message);
        });
    }

    public updateTerminal(terminal: Terminal) {
      return super.putWithParams('/api/experts/Put', JSON.stringify(terminal));
    }

    public updateAccountState(accState: AccountState) {
       const par = JSON.stringify(accState);
       return super.putWithParams('/api/wallets/Put', par);
    }

    public getRange(fromDate: string, toDate: string) {
       const url = '/api/wallets/GetRange?id=0&fromDate=' + fromDate + '&toDate=' + toDate;
       // notify(url);
       // console.log(url);
       return super.getAll(url);
    }

    public getPerformance(month: number, period: number) {
      const url = '/api/wallets/Performance?month=' + month + '&period=' + period;
      return super.getAll(url);
    }

    public assetsDistribution(type: number) {
      const url = '/api/wallets/AssetsDistribution?type=' + type;
      return super.getAll(url);
    }


}
