import { SocialAuthService, SocialUser } from '@abacritt/angularx-social-login';
import { HttpClient } from '@angular/common/http';
import { Component, Inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ModalDismissReasons, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { Observable } from 'rxjs';
import { first, map } from 'rxjs/operators';
import { AppService } from '../../app.service';
import { Produit, ProduitVM } from '../../model/produits';
import { environment } from './../../../environments/environment';
import { AuthService } from './../../_services/auth.service';

@Component({
  selector: 'app-produit-detail',
  templateUrl: './produit-detail.component.html',
  styleUrls: ['./produit-detail.component.css']
})
export class ProduitDetailComponent implements OnInit {
  id!: string;
  produit!: ProduitVM;
  response!: string;
  closeResult = '';
  isAuthenticated!: boolean;
  socialAuthService: SocialAuthService;
  socialUser!: SocialUser | null;
  private authenticatePath = environment.usersUrl + 'Authenticate';

  constructor(private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService,
    private appService: AppService,
    private modalService: NgbModal,
    private http: HttpClient,
    private _socialAuthService: SocialAuthService) {
      this.socialAuthService = _socialAuthService;
    }

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.id = params['id'];

      if (!this.id) {
        alert('Invalid action.');
        this.router.navigate(['produits']);
        return;
      }

      this.appService.getProductVMById(this.id)
        .subscribe( data => {
          console.log(data);
          this.produit = data;
          console.log(this.produit);
        });
    });
    const currentUser = this.authService.currentUserValue;
    this.isAuthenticated = currentUser ? true : false;

    if (!this.isAuthenticated) {
      this.socialAuthService.authState.subscribe((user: SocialUser) => {
        // console.log(user);
        if (user) {
          this.http.post<any>(this.authenticatePath, { idToken: user.idToken }).subscribe((authToken: any) => {
            // console.log(authToken);
           });
        }
        this.socialUser = user;
      });
    }
  }

  editProduct(produit: Produit): void {
    this.router.navigate(['produits/produit-edit', produit.id]);
  }

  open(content:any) {
    this.modalService
      .open(content, {
        ariaLabelledBy: 'modal-basic-title',
        size: 'lg',
        windowClass: 'modal-xl',
      })
      .result.then(
        (result) => {
          this.closeResult = `Closed with: ${result}`;
        },
        (reason) => {
          this.closeResult = `Dismissed ${this.getDismissReason(reason)}`;
        }
      );
  }

  private getDismissReason(reason: any): string {
    if (reason === ModalDismissReasons.ESC) {
      return 'by pressing ESC';
    } else if (reason === ModalDismissReasons.BACKDROP_CLICK) {
      return 'by clicking on a backdrop';
    } else {
      return `with: ${reason}`;
    }
  }

}
