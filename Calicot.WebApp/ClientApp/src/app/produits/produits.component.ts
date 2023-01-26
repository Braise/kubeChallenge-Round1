import { SocialAuthService, SocialUser } from '@abacritt/angularx-social-login';
import { HttpClient } from '@angular/common/http';
import { Component, Inject, OnInit } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { Produit } from '../model/produits';
import { AppService } from './../app.service';
import { AuthService } from './../_services/auth.service';

@Component({
  selector: 'app-produits',
  templateUrl: './produits.component.html',
  styleUrls: ['./produits.component.css']
})
export class ProduitsComponent implements OnInit {
  produits: Produit[] = [];
  isAuthenticated: boolean = false;
  socialAuthService: SocialAuthService;
  socialUser: Observable<SocialUser> = new Observable<SocialUser>();

  constructor(private router: Router,
    private authService: AuthService,
    private apiService: AppService,
    private _socialAuthService: SocialAuthService,
    http: HttpClient,
    @Inject('BASE_URL') baseUrl: string) {
      this.socialAuthService = _socialAuthService;
  }

  ngOnInit() {
    this.apiService.getProducts()
      .subscribe( data => {
        console.log(data);
        this.produits = data;
      });

    const currentUser = this.authService.currentUserValue;
    this.isAuthenticated = currentUser ? true : false;

    if (!this.isAuthenticated) {
      this.socialUser = this.socialAuthService.authState.pipe(map((socialUser: SocialUser) => socialUser));
      this.socialUser.subscribe(user => {
          this.isAuthenticated = !!user;
      });
    }
  }

  deleteProduct(produit: Produit): void {
    if (confirm('Vous supprimer le produit ' + produit.nom)) {
      this.apiService.deleteProduct(produit.id)
      .subscribe( data => {
        this.produits = this.produits.filter(u => u !== produit);
      });
    }
  }

  editProduct(produit: Produit): void {
    this.router.navigate(['produits/produit-edit', produit.id]);
  }

  addProduct(): void {
    this.router.navigate(['produits/produit-ajout']);
  }

}



