import { ComponentFixture, TestBed } from '@angular/core/testing';

import { IdentitySucsessComponent } from './identity-sucsess.component';

describe('IdentitySucsessComponent', () => {
  let component: IdentitySucsessComponent;
  let fixture: ComponentFixture<IdentitySucsessComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ IdentitySucsessComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(IdentitySucsessComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
