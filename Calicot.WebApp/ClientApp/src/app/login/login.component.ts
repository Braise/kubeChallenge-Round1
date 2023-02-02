import { GoogleLoginProvider, SocialAuthService, SocialLoginModule, SocialUser } from '@abacritt/angularx-social-login';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validator, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { EventEmitter } from "events";
import { first } from 'rxjs/operators';
import { AuthService } from './../_services/auth.service';

declare global {
  function handleCredentialResponse(response: any): void
}

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  loginForm!: FormGroup;
  loading = false;
  submitted = false;
  returnUrl!: string;
  socialUser!: SocialUser;
  isLoggedin?: boolean;
  error = '';

  constructor(private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private socialAuthService: SocialAuthService,
    private socialLoginModule: SocialLoginModule) {
    // redirect to home if already logged in
    if (this.authService.currentUserValue) {
      this.authService.logout();
      this.router.navigate(['/']);
    }
  }

  ngOnInit() {
    // this.loginForm = this.fb.group({
    //   UserName: ['', [Validators.required]],
    //   Password: ['', [Validators.required]]
    // });

    this.socialAuthService.authState.subscribe((user) => {
      this.socialUser = user;
      this.isLoggedin = user != null;
      console.log('SocialUser:', this.socialUser);
    });

    global.handleCredentialResponse = (response: any) => {
      this.authService.signedInWithGoogle(response, this.returnUrl);
    }

    // get return url from route parameters or default to '/'
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
  }

  // convenience getter for easy access to form fields
  get f() { return this.loginForm.controls; }

  // onSubmit() {
  //   this.submitted = true;

  //   // stop here if form is invalid
  //   if (this.loginForm.invalid) {
  //       return;
  //   }

  //   this.loading = true;
  //   this.authService.login(this.loginForm.value)
  //       .pipe(first())
  //       .subscribe({
  //         next : (data) => {
  //             this.router.navigate([this.returnUrl]);
  //           },
  //         error: (error) => {
  //           this.error = error;
  //           this.loading = false;
  //           }
  //       });
  // }

  loginWithGoogle(): void {
    this.submitted = true;
    this.loading = true;
    this.authService.signInWithGoogle(this.returnUrl);
  }



  getReturlUrl(): string {
    return this.returnUrl;
  }

  // get UserName() { return this.loginForm.get('UserName'); }
  // get Password() { return this.loginForm.get('Password'); }

}
