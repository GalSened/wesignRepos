import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { ChoiceFieldComponent } from './choice-field.component';

describe('ChoiceFieldComponent', () => {
  let component: ChoiceFieldComponent;
  let fixture: ComponentFixture<ChoiceFieldComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ChoiceFieldComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChoiceFieldComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
