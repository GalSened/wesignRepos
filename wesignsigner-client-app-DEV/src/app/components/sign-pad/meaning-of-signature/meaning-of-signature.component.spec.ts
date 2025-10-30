import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MeaningOfSignatureComponent } from './meaning-of-signature.component';

describe('MeaningOfSignatureComponent', () => {
  let component: MeaningOfSignatureComponent;
  let fixture: ComponentFixture<MeaningOfSignatureComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ MeaningOfSignatureComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MeaningOfSignatureComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
