import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { CustomDropDownComponent } from './custom-drop-down.component';

describe('CustomDropDownComponent', () => {
  let component: CustomDropDownComponent;
  let fixture: ComponentFixture<CustomDropDownComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ CustomDropDownComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CustomDropDownComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
