import { SocialAuthService } from '@abacritt/angularx-social-login';
import { Component, enableProdMode, Inject, OnInit } from '@angular/core';
import { User } from './../model/user';
import { AuthService } from './../_services/auth.service';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent implements OnInit {
  isExpanded = false;
  isAuthenticated: boolean;
  currentUser!: User | null;
  socialAuthService: SocialAuthService;

  constructor(private authService: AuthService, private _socialAuthService: SocialAuthService) {
    this.socialAuthService = _socialAuthService;
    const currentUser = this.authService.currentUserValue;
    this.isAuthenticated = currentUser ? true : false;
  }

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }

  logout(): void {
    this.authService.signOut();
  }

  ngOnInit() {
    this.currentUser = this.authService.currentUserValue;
    this.isAuthenticated = this.currentUser ? true : false;
  }
}
