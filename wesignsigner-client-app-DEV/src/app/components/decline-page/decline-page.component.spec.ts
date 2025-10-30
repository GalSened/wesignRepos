import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { DeclinePageComponent } from './decline-page.component';

describe('DeclinePageComponent', () => {
  let component: DeclinePageComponent;
  let fixture: ComponentFixture<DeclinePageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ DeclinePageComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DeclinePageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
