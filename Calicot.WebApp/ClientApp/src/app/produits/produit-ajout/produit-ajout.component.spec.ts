import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { ProduitAjoutComponent } from './produit-ajout.component';

describe('ProduitAjoutComponent', () => {
  let component: ProduitAjoutComponent;
  let fixture: ComponentFixture<ProduitAjoutComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ProduitAjoutComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ProduitAjoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
