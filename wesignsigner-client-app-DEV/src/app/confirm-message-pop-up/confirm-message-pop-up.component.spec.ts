import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConfirmMessagePopUpComponent } from './confirm-message-pop-up.component';

describe('ConfirmMessagePopUpComponent', () => {
  let component: ConfirmMessagePopUpComponent;
  let fixture: ComponentFixture<ConfirmMessagePopUpComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ConfirmMessagePopUpComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ConfirmMessagePopUpComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
