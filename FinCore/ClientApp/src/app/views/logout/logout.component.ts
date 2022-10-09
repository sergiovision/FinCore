
import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { UntypedFormBuilder, UntypedFormGroup } from '@angular/forms';

import { AuthenticationService } from '../../services/authentication.service';

@Component({templateUrl: 'logout.component.html'})
export class LogoutComponent implements OnInit {
    loginForm: UntypedFormGroup;
    loading = false;
    submitted = false;
    returnUrl: string;

    constructor(
        private formBuilder: UntypedFormBuilder,
        private route: ActivatedRoute,
        private router: Router,
        private authenticationService: AuthenticationService
    ) {

      this.authenticationService.logout();

    }

    ngOnInit() {

    }
}
