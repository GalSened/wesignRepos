import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { RadioGroupFieldComponent } from './radio-group-field.component';

describe('RadioGroupFieldComponent', () => {
  let component: RadioGroupFieldComponent;
  let fixture: ComponentFixture<RadioGroupFieldComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ RadioGroupFieldComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RadioGroupFieldComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
