import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { OtpDetailsComponent } from './otp-details.component';

describe('OtpDetailsComponent', () => {
  let component: OtpDetailsComponent;
  let fixture: ComponentFixture<OtpDetailsComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ OtpDetailsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OtpDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
