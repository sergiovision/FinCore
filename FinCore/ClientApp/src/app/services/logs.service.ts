import { BaseService } from './base.service';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class LogsService extends BaseService {
  constructor(http: HttpClient) {
      super(http);
  }

  public GetLogContent(name: string, lines: number) {
    const url = this.baseURL + '/api/wallets/GetLogContent?logName=' + name + '&size=' + lines;
    // console.log(url);
    return this.http.get(url, { headers: this.authHeaders, responseType: 'text'});
  }

  public GetLogList() {
    const url = this.baseURL + '/api/wallets/LogList';
    return this.http.get<any>(url, { headers: this.authHeaders });
  }


}
