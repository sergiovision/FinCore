import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthenticationService } from '../../services/authentication.service';
import { environment } from '../../../environments/environment';

const REMEMBER_EMAIL_KEY = 'fincore-remember-email';
const DEFAULT_PATH = '/dashboard';

@Component({
  templateUrl: 'login.component.html',
  styleUrls: ['login.component.scss']
})
export class LoginComponent implements OnInit {
  loading = false;
  returnUrl = DEFAULT_PATH;
  username: string;
  password: string;
  rememberMe = false;
  errorMessage = '';
  // Which server this sign-in goes to, so the user can tell instances apart.
  serverUrl = '';
  serverHost = '';
  buildEnv = environment.production ? 'production' : 'development';

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private authenticationService: AuthenticationService
  ) {}

  ngOnInit() {
    this.returnUrl = this.resolveReturnUrl();
    if (this.authenticationService.isAuthenticated) {
      this.router.navigateByUrl(this.returnUrl);
      return;
    }
    // a stored but expired token would bounce every guarded route back here
    if (this.authenticationService.currentUserValue) {
      this.authenticationService.logout();
    }

    this.loadServerInfo();

    const saved = localStorage.getItem(REMEMBER_EMAIL_KEY);
    if (saved) {
      this.username = saved;
      this.rememberMe = true;
    }
  }

  /** Host of the backend `baseURL` (relative URLs resolve against the page). */
  private loadServerInfo() {
    try {
      const url = new URL(environment.baseURL || '/', window.location.href);
      this.serverUrl = url.origin + url.pathname.replace(/\/+$/, '');
      this.serverHost = url.host;
    } catch {
      this.serverUrl = window.location.origin;
      this.serverHost = window.location.host;
    }
  }

  /**
   * Where to go after sign-in: the `returnUrl` the AuthGuard remembered when it is
   * an in-app path (never `//host` or a full URL, so it can't become an open redirect).
   */
  private resolveReturnUrl(): string {
    const url = this.route.snapshot.queryParamMap.get('returnUrl');
    if (url && url !== '/' && url.startsWith('/') && !url.startsWith('//') && !url.startsWith('/\\')
        && !url.startsWith('/login')) {
      return url;
    }
    return DEFAULT_PATH;
  }

  logError(error: any) {
    const message = typeof error === 'string' ? error : (error?.message || JSON.stringify(error));
    this.errorMessage = `Login failed: ${message}`;
  }

  async onSubmit() {
    try {
      this.loading = true;
      this.errorMessage = '';
      await this.authenticationService.loginAndRedirect(this.username, this.password || '', this.returnUrl);

      if (this.rememberMe) {
        localStorage.setItem(REMEMBER_EMAIL_KEY, this.username);
      } else {
        localStorage.removeItem(REMEMBER_EMAIL_KEY);
      }
    } catch (error) {
      this.logError(error);
    } finally {
      this.loading = false;
    }
  }
}
