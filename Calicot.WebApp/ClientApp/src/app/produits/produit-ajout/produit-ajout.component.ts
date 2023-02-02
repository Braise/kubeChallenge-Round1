import { Component, enableProdMode, Inject, OnInit } from '@angular/core';
import {
  FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ModalDismissReasons, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { FileUploader } from 'ng2-file-upload';
import { first } from 'rxjs/operators';
import { AppService } from '../../app.service';
import { environment } from './../../../environments/environment';
import { Produit, ProduitVM } from './../../model/produits';


@Component({
  selector: 'app-produit-ajout',
  templateUrl: './produit-ajout.component.html',
  styleUrls: ['./produit-ajout.component.css']
})
export class ProduitAjoutComponent implements OnInit {
  id!: string;
  produit!: Produit;
  editForm!: FormGroup;

  uploader: FileUploader;
  hasBaseDropZoneOver: boolean;
  hasAnotherDropZoneOver: boolean;
  response: string;
  closeResult = '';

  apiEndPoint = environment.apiEndpoint

  constructor(private route: ActivatedRoute,
              private formBuilder: FormBuilder,
              private router: Router,
              private appService: AppService,
              private modalService: NgbModal,
              @Inject('BASE_URL') baseUrl: string) {
                this.uploader = new FileUploader({ url: this.apiEndPoint + 'api/FileUpload' });

                this.hasBaseDropZoneOver = false;
                this.hasAnotherDropZoneOver = false;
                this.response = '';
                this.uploader.response.subscribe((res) => {
                  const obj = JSON.parse(res);
                  this.response = res;
                  this.editForm.patchValue({
                    image: obj.fileName,
                  });
                  this.produit.image = obj.fileName;
                });
              }

  ngOnInit() {
    this.produit = {
      id: '',
      nom: '',
      description: '',
      prix: 0,
      image: ''
    };
    this.editForm = this.formBuilder.group({
      nom: ['', Validators.required],
      description: ['', Validators.required],
      prix: ['', Validators.required],
      image: ['', Validators.required],
      // imageUrl: new FormControl(''),
      // imageThumbUrl: new FormControl('')
    });
  }

  submit() {
    console.log(this.editForm.value);
    this.appService.createProduct(this.editForm.value)
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
