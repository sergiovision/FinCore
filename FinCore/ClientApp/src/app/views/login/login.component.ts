
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { first } from 'rxjs/operators';

import { AuthenticationService } from '../../services/authentication.service';
import { AlertService } from '../../services/alert.service';
import { SubSink } from 'subsink';

@Component({templateUrl: 'login.component.html'})
export class LoginComponent implements OnInit, OnDestroy {
    protected subs: SubSink = new SubSink();

    loading = false;
    submitted = false;
    returnUrl: string;
    username: string;
    password: string;

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private authenticationService: AuthenticationService,
        private alertService: AlertService
    ) {
        // redirect to home if already logged in
        if (this.authenticationService.currentUserValue) {
            this.router.navigate(['/']);
        }
    }

    ngOnInit() {
        this.returnUrl = '/dashboard';
    }

    ngOnDestroy(): void {
      this.subs.unsubscribe();
    }

    public onSubmit() {
        this.submitted = true;
        this.loading = true;
        this.subs.sink = this.authenticationService.login(this.username, this.password)
            .pipe(first())
            .subscribe(
                data => {
                    this.router.navigate(['/dashboard']);
                },
                error => {
                    const message = JSON.stringify( error.error) + '\n' + error.statusText;
                    this.alertService.error(error);
                    this.loading = false;
                });
    }
}
