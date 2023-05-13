import { BaseService } from './base.service';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { EntitiesEnum, DynamicProperties } from '../models/Entities';

declare var require: any;

@Injectable()
export class PropsService extends BaseService {
  constructor(http: HttpClient) {
    super(http);
  }

  public override getAll() {
    return super.getAll('/api/props');
  }

  public getInstance(type: EntitiesEnum, objId: number) {
    const etype: number = type.valueOf() as number;
    const url = '/api/props/GetInstance?entityType=' + etype + '&objId=' + objId;
    return super.getAll(url);
  }

  public saveInstance(value: DynamicProperties) {
    const url = '/api/props/SaveInstance';
    return super.putWithParams(url, JSON.stringify(value));
  }

  public getDefinitionsForEntity(entity: string): any {
    const config = require('../../assets/propdefs.json');
    return config[entity];
  }
}
