import { BaseService } from './base.service';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import notify from 'devextreme/ui/notify';
import { Adviser } from '../models/Entities';
import { Observable } from 'rxjs';

@Injectable()
export class ExpertsService extends BaseService {
    constructor(http: HttpClient) { super(http); }

    public getRange(fromDate: string, toDate: string) {
       const url = '/api/experts/GetRange?id=0&fromDate=' + fromDate + '&toDate=' + toDate;
       // notify(url);
       console.log(url);
       return super.getAll(url);
    }

    public updateAdviserState(adv: Adviser) {
      const url = '/api/experts/UpdateAdviserState';
      // notify(url);
      console.log(url);
      return super.putWithParams(url, JSON.stringify(adv));
    }

}
