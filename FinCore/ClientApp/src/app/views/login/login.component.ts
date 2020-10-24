
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { first } from 'rxjs/operators';
import { AuthenticationService } from '../../services/authentication.service';
import notify from 'devextreme/ui/notify';

@Component({templateUrl: 'login.component.html'})
export class LoginComponent implements OnInit, OnDestroy {
    loading = false;
    submitted = false;
    returnUrl: string;
    username: string;
    password: string;

    constructor(
        private router: Router,
        private authenticationService: AuthenticationService) {
    }

    ngOnInit() {
      // redirect to home if already logged in
      if (this.authenticationService.currentUserValue) {
          this.router.navigate(['/']);
      }
      this.returnUrl = '/dashboard';
    }

    ngOnDestroy(): void {
    }

    logError(error: any) {
      const message = error;
      notify(message);
      this.loading = false;
    }

    public onSubmit() {
        this.submitted = true;
        this.loading = true;
        this.authenticationService.login(this.username, this.password)
            .pipe(first())
            .subscribe(
                data => this.router.navigate(['/dashboard']),
                error => this.logError(error));
    }
}
