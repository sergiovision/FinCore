import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { first } from 'rxjs/operators';
import { AuthenticationService } from '../../services/authentication.service';
import notify from 'devextreme/ui/notify';

@Component({
  templateUrl: 'login.component.html'
})
export class LoginComponent implements OnInit {
  loading = false;
  submitted = false;
  returnUrl: string;
  username: string;
  password: string;

  constructor(
    private router: Router,
    private authenticationService: AuthenticationService
  ) {}

  ngOnInit() {
    if (this.authenticationService.currentUserValue) {
      this.router.navigate(['/']);
    }
    this.returnUrl = '/dashboard';
  }

  logError(error: any) {
    const message = error;
    notify(message);
    this.loading = false;
  }

  async onSubmit() {
    try {
      this.submitted = true;
      this.loading = true;
      await this.authenticationService.loginAndRedirect(this.username, this.password, '/dashboard');
    } catch (error) {
      this.logError(error);
    } finally {
      this.loading = false;
    }
  }
}
