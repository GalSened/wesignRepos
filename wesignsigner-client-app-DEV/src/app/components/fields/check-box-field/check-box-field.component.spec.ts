import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { CheckBoxFieldComponent } from './check-box-field.component';

describe('CheckBoxFieldComponent', () => {
  let component: CheckBoxFieldComponent;
  let fixture: ComponentFixture<CheckBoxFieldComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ CheckBoxFieldComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CheckBoxFieldComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
