import { HttpClientModule, HTTP_INTERCEPTORS } from "@angular/common/http";
import { NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { BrowserModule } from "@angular/platform-browser";
import { RouterModule } from "@angular/router";
import { NgbModalModule } from "@ng-bootstrap/ng-bootstrap";
import { FileUploadModule } from "ng2-file-upload";

import { AppComponent } from "./app.component";
import { HomeComponent } from "./home/home.component";
import { LoginComponent } from "./login/login.component";
import { MessagesComponent } from "./messages/messages.component";
import { NavMenuComponent } from "./nav-menu/nav-menu.component";
import { ProduitAjoutComponent } from "./produits/produit-ajout/produit-ajout.component";
import { ProduitDetailComponent } from "./produits/produit-detail/produit-detail.component";
import { ProduitEditComponent } from "./produits/produit-edit/produit-edit.component";
import { ProduitsComponent } from "./produits/produits.component";
import { AuthGuard } from "./_helpers/auth.guard";
import { AuthService } from "./_services/auth.service";

import {
  GoogleLoginProvider,
  SocialLoginModule
} from "@abacritt/angularx-social-login";
import { ErrorInterceptor, JwtInterceptor } from "./_helpers";

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    ProduitsComponent,
    ProduitAjoutComponent,
    ProduitDetailComponent,
    ProduitEditComponent,
    MessagesComponent,
    LoginComponent,
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: "ng-cli-universal" }),
    SocialLoginModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    FileUploadModule,
    NgbModalModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'login', component: LoginComponent },
      { path: 'logout', component: LoginComponent },
      { path: "produits", component: ProduitsComponent },
      {
        path: "produits/produit-ajout",
        component: ProduitAjoutComponent,
        canActivate: [AuthGuard],
      },
      {
        path: "produits/produit-detail/:id",
        component: ProduitDetailComponent,
      },
      {
        path: "produits/produit-edit/:id",
        component: ProduitEditComponent,
        canActivate: [AuthGuard],
      },
      {
        path: "messages",
        component: MessagesComponent,
        canActivate: [AuthGuard],
      },
      // otherwise redirect to home
      { path: "**", redirectTo: "" },
    ]),
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },
    {
      provide: "SocialAuthServiceConfig",
      useValue: {
        autoLogin: true, // keeps the user signed in
        providers: [
          {
            id: GoogleLoginProvider.PROVIDER_ID,
            provider: new GoogleLoginProvider(
              "1014581424284-8fnrj0fis18nl0ob7im8lvsj86c9og5f.apps.googleusercontent.com"
            ), // your client id
          },
        ],
      },
    },
    AuthService,
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
