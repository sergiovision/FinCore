import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { BehaviorSubject, Observable, lastValueFrom } from 'rxjs';
import { map } from 'rxjs/operators';
import { UserToken } from '../models/Entities';
import { BaseService } from './base.service';
import { Router } from '@angular/router';
import { clearStoredUser, getStoredUser, isUserAuthenticated, setStoredUser } from '../helpers/jwt.helper';


@Injectable({ providedIn: 'root' })
export class AuthenticationService extends BaseService {
    private currentUserSubject: BehaviorSubject<UserToken>;
    public currentUser: Observable<UserToken>;

    constructor(http: HttpClient, private router: Router) {
        super(http);
        this.currentUserSubject = new BehaviorSubject<UserToken>(getStoredUser());
        this.currentUser = this.currentUserSubject.asObservable();
    }

    public get currentUserValue(): UserToken {
        return this.currentUserSubject.value;
    }

    /** True when a token is stored and has not expired yet. */
    public get isAuthenticated(): boolean {
        return isUserAuthenticated(this.currentUserValue);
    }


    login(username: string, password: string) {
      const url = this.baseURL + '/api/token';

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
            if (user && user.access_token) {
              // store user details and jwt token in local storage to keep user logged in between page refreshes
              setStoredUser(user);
              this.currentUserSubject.next(user);
            }
            return user;
          }));

    }

    logout() {
        // remove user from local storage to log user out
        clearStoredUser();
        this.currentUserSubject.next(null);
    }

    async loginAndRedirect(username: string, password: string, redirect: string): Promise<void> {
      const user = await lastValueFrom(this.login(username, password), { defaultValue: null });
      if (!user || !user.access_token) {
        throw new Error('Server returned no access token');
      }
      this.router.navigateByUrl(redirect);
    }
}
