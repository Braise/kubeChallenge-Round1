import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { ProduitEditComponent } from './produit-edit.component';

describe('ProduitEditComponent', () => {
  let component: ProduitEditComponent;
  let fixture: ComponentFixture<ProduitEditComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ProduitEditComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ProduitEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
