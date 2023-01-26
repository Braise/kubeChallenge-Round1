import { SocialAuthService, SocialUser } from '@abacritt/angularx-social-login';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, Inject, Injectable, OnInit } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { MessageService } from './message.service';
import { ApiResponse } from './model/ApiResponse';
import { Produit, ProduitVM } from './model/produits';
import { AuthService } from './_services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AppService {
  baseUrl = '/api/Produits';

  constructor(private http: HttpClient,
      private messageService: MessageService,
      private authService: AuthService,
      private socialAuthService: SocialAuthService,
      @Inject('BASE_URL') baseUrl: string) {
    this.baseUrl = baseUrl + 'api/Produits/';
  }

  getProducts(): Observable<Produit[]> {
    // return this.http.get<ApiResponse>(this.baseUrl);
    return this.http.get<Produit[]>(this.baseUrl).pipe(
      tap(_ => this.log('fetched products')),
      catchError(this.handleError<Produit[]>('getProducts', []))
    );
    // this.http.get<Produit[]>(this.baseUrl + 'api/Produits').subscribe(result => {
    //   return result;
    // }, error => console.error(error));
  }

  getProductById(id: string): Observable<Produit> {
    return this.http.get<Produit>(this.baseUrl + id).pipe(
      tap(_ => this.log(`fetched product id=${id}`)),
      catchError(this.handleError<Produit>(`getProductById id=${id}`))
    );
  }

  getProductVMById(id: string): Observable<ProduitVM> {
    return this.http.get<ProduitVM>(this.baseUrl + id).pipe(
      tap(_ => this.log(`fetched product id=${id}`)),
      catchError(this.handleError<ProduitVM>(`getProductVMById id=${id}`))
    );
  }

  createProduct(produit: Produit): Observable<Produit> {
    let httpOption = {};
    if (this.authService.reqHeader) {
      httpOption  = {
        headers: this.authService.reqHeader
      };
    } else {
      const token = this.authService.getToken();
      httpOption  = {
        headers: new HttpHeaders({
          'Content-Type':  'application/json',
          'Authorization': token?token:''
        })
      };
    }

    let newProduit: Produit = {
      id: '',
      nom: produit.nom,
      description: produit.description,
      prix: produit.prix,
      image: produit.image
    };

    console.info('Creatin new product:', newProduit);

    // return this.http.post<Produit>(this.baseUrl, produit);
    return this.http.post<Produit>(this.baseUrl, newProduit, httpOption).pipe(
      tap(_ => this.log(`Fetched product id=${newProduit.nom}`)),
      catchError(this.handleError<Produit>(`createProduct`))
    );
  }

  updateProduct(produit: Produit): Observable<Produit> {
    let httpOption = {};
    if (this.authService.reqHeader) {
      httpOption  = {
        headers: this.authService.reqHeader
      };
    } else {
      const token = this.authService.getToken();
      httpOption  = {
        headers: new HttpHeaders({
          'Content-Type':  'application/json',
          'Authorization': token?token:''
        })
      };
    }

    return this.http.put<Produit>(this.baseUrl + produit.id, produit, httpOption);
  }

  deleteProduct(id: string): Observable<Produit> {
    let httpOption = {};
    if (this.authService.reqHeader) {
      httpOption  = {
        headers: this.authService.reqHeader
      };
    } else {
      const token = this.authService.getToken();
      httpOption  = {
        headers: new HttpHeaders({
          'Content-Type':  'application/json',
          'Authorization': token?token:''
        })
      };
    }
    return this.http.delete<Produit>(this.baseUrl + id, httpOption);
  }

  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      // TODO: send the error to remote logging infrastructure
      console.error(error); // log to console instead
      // TODO: better job of transforming error for user consumption
      this.log(`${operation} failed: ${error.message}`);
      // Let the app keep running by returning an empty result.
      return of(result as T);
    };
  }

  /** Log a Service message with the MessageService */
  private log(message: string) {
    this.messageService.add(`AppService: ${message}`);
  }
}
