import { Component, enableProdMode, Inject, OnInit } from '@angular/core';
import {
  FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ModalDismissReasons, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { FileUploader } from 'ng2-file-upload';
import { first } from 'rxjs/operators';
import { AppService } from '../../app.service';
import { Produit } from '../../model/produits';
import { AuthService } from './../../_services/auth.service';

// const URL = '/api/';

@Component({
  selector: 'app-produit-edit',
  templateUrl: './produit-edit.component.html',
  styleUrls: ['./produit-edit.component.css'],
})
export class ProduitEditComponent implements OnInit {
  id: string = '';
  produit!: Produit;
  editForm!: FormGroup;
  uploader: FileUploader;
  hasBaseDropZoneOver: boolean;
  hasAnotherDropZoneOver: boolean;
  response: string;
  closeResult = '';
  isAuthenticated!: boolean;

  constructor(
    private route: ActivatedRoute,
    private formBuilder: FormBuilder,
    private router: Router,
    private authService: AuthService,
    private appService: AppService,
    private modalService: NgbModal,
    @Inject('BASE_URL') baseUrl: string
  ) {
    this.uploader = new FileUploader({ url: baseUrl + 'api/FileUpload' });

    this.hasBaseDropZoneOver = false;
    this.hasAnotherDropZoneOver = false;
    this.response = '';
    this.uploader.response.subscribe((res) => {
      // console.log(res);
      const obj = JSON.parse(res);
      this.response = res;
      this.editForm.patchValue({
        image: obj.fileName,
      });
      this.produit.image = obj.fileName;
    });
  }

  ngOnInit() {
    // const produitId = window.localStorage.getItem('editProductId');
    this.route.params.subscribe((params) => {
      this.id = params['id'];

      if (!this.id) {
        alert('Invalid action.');
        this.router.navigate(['produits']);
        return;
      }
      this.produit = {
          id: '',
          nom: '',
          description: '',
          prix: 0,
          image: ''
        };
      // this.editForm = this.formBuilder.group({
      //   id: [''],
      //   nom: ['', Validators.required],
      //   description: ['', Validators.required],
      //   prix: ['', Validators.required],
      //   image: ['', Validators.required],
      //   imageUrl: [''],
      //   imageThumbUrl: ['']
      // });
      this.editForm = new FormGroup({
        id: new FormControl(this.produit.id, [Validators.required]),
        nom: new FormControl(this.produit.nom, [Validators.required]),
        description: new FormControl(this.produit.description, [Validators.required]),
        prix: new FormControl(this.produit.prix, [Validators.required]),
        image: new FormControl(this.produit.image, [Validators.required]),
        imageUrl: new FormControl(''),
        imageThumbUrl: new FormControl('')
      });
      this.appService.getProductById(this.id).subscribe((data) => {
        console.log("data retourné par le getProductById ", data);
        // this.produit = {
        //   id: data.id,
        //   nom: data.nom,
        //   description: data.description,
        //   prix: data.prix,
        //   image: data.image
        // };
        this.produit = data;
        this.editForm.setValue(data);
      });
    });
  }

  submit() {
    this.appService
      .updateProduct(this.editForm.value)
      .pipe(first())
      .subscribe({
        next: (data) => { this.router.navigate(['produits']); },
        error: (error) => { alert(error); },
        complete: () => console.info('complete')
       });
  }

  public fileOverBase(e: any): void {
    this.hasBaseDropZoneOver = e;
  }

  public fileOverAnother(e: any): void {
    this.hasAnotherDropZoneOver = e;
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
