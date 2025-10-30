import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { SignPadComponent } from './sign-pad.component';

describe('SignPadComponent', () => {
  let component: SignPadComponent;
  let fixture: ComponentFixture<SignPadComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ SignPadComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SignPadComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
