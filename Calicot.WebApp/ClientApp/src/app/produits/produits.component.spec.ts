import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { ProduitsComponent } from './produits.component';

describe('ProduitsComponent', () => {
  let component: ProduitsComponent;
  let fixture: ComponentFixture<ProduitsComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ProduitsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ProduitsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
