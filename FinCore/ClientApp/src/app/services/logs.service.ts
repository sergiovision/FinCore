import { BaseService } from './base.service';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class LogsService extends BaseService {
  constructor(http: HttpClient) {
      super(http);
  }

}
