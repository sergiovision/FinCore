import { BaseService } from './base.service';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { UserToken } from '../models/Entities';

@Injectable()
export class PersonService extends BaseService {

    constructor(protected override http: HttpClient) { super(http); }

    create(user: UserToken) {
        return this.http.post(this.baseURL +'/api/persons', user);
    }

    update(user: UserToken) {
        return this.http.put(this.baseURL + '/api/persons/' + user.userName, user);
    }
}
