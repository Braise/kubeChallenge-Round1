import { GoogleLoginProvider, SocialAuthService, SocialLoginModule, SocialUser } from '@abacritt/angularx-social-login';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { promise } from 'protractor';
import { BehaviorSubject, Observable } from 'rxjs';
import { first, map } from 'rxjs/operators';
import { User } from '../model/user';
import { environment } from './../../environments/environment';
import { LoginComponent } from './../login/login.component';


@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private authenticatePathOld = environment.usersUrl + 'AuthenticateOld';
  private authenticatePath = environment.usersUrl + 'authenticate';
  private currentUserSubject: BehaviorSubject<User|null> = new BehaviorSubject<User | null>(null);
  public currentUser: Observable<User|null>;
  user: SocialUser | null;
  hasApiAccess = false;
  public reqHeader!: HttpHeaders;
  public token!: string | undefined | null;

  constructor(private http: HttpClient,
    private socialAuthService: SocialAuthService,
    private router: Router) {
    let stLocalUser = localStorage.getItem('currentUser');
    if(stLocalUser == null) stLocalUser = '';
    console.log('Local User: '+stLocalUser);
    if(stLocalUser != '')
      this.currentUserSubject = new BehaviorSubject<User | null>(JSON.parse(stLocalUser));
    this.currentUser = this.currentUserSubject.asObservable();
    this.user = null;
    this.socialAuthService.authState.subscribe((user: SocialUser) => {
      if (user) {
        this.http.post<any>(this.authenticatePath, { idToken: user.idToken }).subscribe((authToken: any) => {
          this.reqHeader = new HttpHeaders({
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + authToken.authToken
            });
         });
         this.user = user;
         this.token = user.idToken;
      }
    });
  }

  login(data:any): Observable<any> {
    return this.http.post<any>(this.authenticatePathOld, data).pipe(map(user => {
      localStorage.setItem('currentUser', JSON.stringify(user));
      this.currentUserSubject.next(user);
      this.token = this.currentUserSubject?.value?.token;
      return user;
    }));
  }

  logout() {
      // remove user from local storage to log user out
      this.token = '';
      localStorage.removeItem('currentUser');
      this.currentUserSubject.next(null);
  }

  decodeJwtResponse(token: any) {
    let base64Url = token.split('.')[1]
    let base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    let jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));
    return JSON.parse(jsonPayload)
  }

  signedInWithGoogle(token: any, returnUrl: string): any {
    this.user = this.decodeJwtResponse(token.credential);
    this.token = this.user?.idToken;
    console.log(token);
    console.log(this.token);
    console.log("ID: " + this.user?.id);
    console.log("IDTOKEN: " + this.user?.idToken);
    console.log('Full Name: ' + this.user?.name);
    // console.log('Given Name: ' + this.responsePayload.given_name);
    // console.log('Family Name: ' + this.responsePayload.family_name);
    // console.log("Image URL: " +this.responsePayload.picture);
    console.log("Email: " + this.user?.email);
    this.router.navigate([returnUrl])
    .then(() => {
      window.location.reload();
    });
  }

  signInWithGoogle(returnUrl: string): any {
    this.socialAuthService.signIn(GoogleLoginProvider.PROVIDER_ID)
      .then((x: SocialUser) => {
          if (x) {
            console.log('Validate User on ', this.authenticatePath);
            this.http.post<any>(this.authenticatePath, { idToken: x.idToken }).subscribe({
              next: (authToken: any) => {
                // console.log('Valid User', x.idToken, authToken);
                this.user = x;
                this.token = x.idToken;
              },
              error: error => {
                console.log(error);
                alert('That gmail user not authorized, proceed to log off');
                this.token = undefined;
                this.user = null;
                this.socialAuthService.signOut();
                this.router.navigate([returnUrl]);
              }});
          }

          this.user = x;
          this.token = x.idToken;
          // return this.user;
        this.router.navigate([returnUrl]);
      });
  }

  signOut() {
    this.token = undefined;
    this.socialAuthService.signOut();
    this.router.navigate(['/']);
  }

  public get currentUserValue(): User | null {
    return this.currentUserSubject?.value;
  }

  getToken() {
    // console.log('Current token : ', this.token);
    return  this.token;
  }
}
