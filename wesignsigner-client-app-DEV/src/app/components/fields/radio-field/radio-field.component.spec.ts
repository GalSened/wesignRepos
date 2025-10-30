import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { RadioFieldComponent } from './radio-field.component';

describe('RadioFieldComponent', () => {
  let component: RadioFieldComponent;
  let fixture: ComponentFixture<RadioFieldComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ RadioFieldComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RadioFieldComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
