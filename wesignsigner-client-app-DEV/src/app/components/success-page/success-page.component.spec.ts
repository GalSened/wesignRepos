import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { SuccessPageComponent } from './success-page.component';

describe('SuccessPageComponent', () => {
  let component: SuccessPageComponent;
  let fixture: ComponentFixture<SuccessPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ SuccessPageComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SuccessPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
