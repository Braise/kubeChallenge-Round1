import { SocialAuthService, SocialUser } from '@abacritt/angularx-social-login';
import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { AuthService } from '../_services/auth.service';

@Injectable({
  providedIn: 'root'
})

export class AuthGuard implements CanActivate {
  constructor(
    private router: Router,
    private authService: AuthService,
    private socialAuthService: SocialAuthService
  ) {}

  canActivate(
    next: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {

      const currentUser = this.authService.currentUserValue;
      if (currentUser) {
          // logged in so return true
          return true;
      }

      return this.socialAuthService.authState.pipe(
        map((socialUser: SocialUser) => !!socialUser),
        tap((isLoggedIn: boolean) => {
          if (!isLoggedIn) {
            this.router.navigate(['/login'], { queryParams: { returnUrl: state.url}});
          }
          return false;
        })
      );

    // this.router.navigate(['/login'], { queryParams: { returnUrl: state.url}});
    // return false;
  }
}
