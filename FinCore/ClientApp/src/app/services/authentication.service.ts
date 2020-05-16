import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { BehaviorSubject, Observable, empty } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import notify from 'devextreme/ui/notify';
import { UserToken } from '../models/Entities';
import { BaseService } from './base.service';


@Injectable({ providedIn: 'root' })
export class AuthenticationService extends BaseService {
    private currentUserSubject: BehaviorSubject<UserToken>;
    public currentUser: Observable<UserToken>;

    constructor(http: HttpClient) {
        super(http);
        this.currentUserSubject = new BehaviorSubject<UserToken>(JSON.parse(localStorage.getItem('currentUser')));
        this.currentUser = this.currentUserSubject.asObservable();
    }

    public get currentUserValue(): UserToken {
        return this.currentUserSubject.value;
    }


    login(username: string, password: string) {
      const url = this.baseURL + '/api/token';
      console.log(url);

       const headers = new HttpHeaders()
        .append('Accept', 'application/json')
        .append('Content-Type', 'application/json; charset=utf-8');
        const loginInfo = {
          username: username,
          password: password
        };
        const body = JSON.stringify(loginInfo);


        return this.http.post<any>(url, body, {headers: headers }  )
        .pipe(map(
          user => {
            console.log('User: ' + user);
            if (user && user.access_token) {
              // store user details and jwt token in local storage to keep user logged in between page refreshes
              localStorage.setItem('currentUser', JSON.stringify(user));
              this.currentUserSubject.next(user);
              console.log('Success login');
            }
          }),
          catchError(error => {
            const message = JSON.stringify(error);
            console.log(message);
            notify(message);
            return empty();
          }));

    }

    logout() {
        // remove user from local storage to log user out
        localStorage.removeItem('currentUser');
        this.currentUserSubject.next(null);
    }
}
