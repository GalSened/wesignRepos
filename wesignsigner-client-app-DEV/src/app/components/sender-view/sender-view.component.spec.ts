import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { SenderViewComponent } from './sender-view.component';

describe('SenderViewComponent', () => {
  let component: SenderViewComponent;
  let fixture: ComponentFixture<SenderViewComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ SenderViewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SenderViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
