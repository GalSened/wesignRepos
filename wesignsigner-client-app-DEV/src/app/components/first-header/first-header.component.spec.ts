import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { FirstHeaderComponent } from './first-header.component';

describe('HeaderComponent', () => {
  let component: FirstHeaderComponent;
  let fixture: ComponentFixture<FirstHeaderComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ FirstHeaderComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FirstHeaderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
