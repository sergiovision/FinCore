import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
// import 'rxjs/add/operator/map';
import { UserToken, Env, JobParam, EntitiesEnum } from '../models/Entities';
import { DatePipe } from '@angular/common';
import CustomStore from 'devextreme/data/custom_store';

@Injectable()
export class BaseService {
  public baseURL: string;
  public formatDate: string;
  public datePipe: DatePipe;
  public env: Env;

  constructor(protected http: HttpClient) {
    this.datePipe = new DatePipe('en-US');
    this.baseURL = environment.baseURL;
    this.formatDate = environment.dateFormat;
  }

  transformDate(date): string {
    return this.datePipe.transform(date, this.formatDate);
  }

  transformShortDate(date): string {
    return this.datePipe.transform(date, environment.shortDateFormat);
  }

  public get currentUserToken(): UserToken {
    return JSON.parse(localStorage.getItem('currentUser'));
  }

  public get authHeaders(): HttpHeaders {
    const user: UserToken = this.currentUserToken;
    return new HttpHeaders()
      .append('Accept', 'application/json; charset=utf-8')
      .append('Content-Type', 'application/json; charset=utf-8')
      .append('Authorization', 'Bearer ' + user.access_token);
  }

  public getAll(apiAction: string) {
    return this.http.get<any>(this.baseURL + apiAction, { headers: this.authHeaders });
  }

  public postWithParams(apiAction: string, body: string) {
    return this.http.post<string>(this.baseURL + apiAction, body, { headers: this.authHeaders });
  }

  public putWithParams(apiAction: string, body: string) {
    return this.http.put<string>(this.baseURL + apiAction, body, { headers: this.authHeaders });
  }

  public loadParentData(entity: EntitiesEnum, showRetired: boolean) {
    const entityNum: number = entity;
    const url = `${this.baseURL}/api/wallets/GetObjects/${entityNum}?showRetired=${showRetired}`;
    return this.http.get<any>(url, { headers: this.authHeaders });
  }

  logConsoleError(error: any) {
    const message = error;
    console.log(message);
  }

  public loadChildData(parentEntity: EntitiesEnum, childEntity: EntitiesEnum, key: number, showRetired: boolean): CustomStore {
    const entityParentNum: number = parentEntity;
    const entityChildNum: number = childEntity;
    const url = `${this.baseURL}/api/wallets/GetChildObjects/${entityParentNum}/${entityChildNum}/${key}?showRetired=${showRetired}`;
    const loadResponse = this.http.get<any>(url, { headers: this.authHeaders });
    const result = new CustomStore({
      key: 'Id',
      load: function(loadOptions: any) {
        return loadResponse
          .toPromise()
          .then((data: any) => {
            return data;
          })
          .catch(error => this.logConsoleError(error));
      }
    });
    return result;
  }

  public pushStore(entity: EntitiesEnum, values: any) {
    const type: number = entity;
    const url = `${this.baseURL}/api/wallets/InsertObject/${type}`;
    return this.http.post<any>(url, values, { headers: this.authHeaders });
  }

  public updateStore(entity: EntitiesEnum, keyId: number, values: any) {
    const type: number = entity;
    // console.log('UpdateObject ' + values);
    const url = `${this.baseURL}/api/wallets/UpdateObject/${type}/${keyId}`;
    // console.log('UpdateObject url: ' + values);
    return this.http.put<any>(url, values, { headers: this.authHeaders });
  }

  public deleteStore(entity: EntitiesEnum, keyId: number) {
    const entityNum: number = entity;
    const url = `${this.baseURL}/api/wallets/DeleteObject/${entityNum}/${keyId}`;
    return this.http.delete<any>(url, { headers: this.authHeaders });
  }

  public getRunning() {
    return this.getAll('/api/jobs/GetRunning');
  }

  public runJob(group: string, name: string) {
    const body: JobParam = new JobParam();
    body.Group = group;
    body.Name = name;
    return this.postWithParams('/api/jobs/Post', JSON.stringify(body));
  }

  public stopJob(group: string, name: string) {
    const body: JobParam = new JobParam();
    body.Group = group;
    body.Name = name;
    return this.postWithParams('/api/jobs/Stop', JSON.stringify(body));
  }

  getVersion() {
    return this.GetGlobalProp('VERSION');
  }

  externChartURL(sym: string): string {
    const url = `https://www.tradingview.com/chart?symbol=${sym}`;
    return url;
  }

  public GetGlobalProp(name: string) {
    const url = '/api/wallets/GetGlobalProp/' + name;
    return this.getAll(url);
  }
}
