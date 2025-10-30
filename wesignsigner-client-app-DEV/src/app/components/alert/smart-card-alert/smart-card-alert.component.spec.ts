import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SmartCardAlertComponent } from './smart-card-alert.component';

describe('SmartCardAlertComponent', () => {
  let component: SmartCardAlertComponent;
  let fixture: ComponentFixture<SmartCardAlertComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SmartCardAlertComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SmartCardAlertComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
