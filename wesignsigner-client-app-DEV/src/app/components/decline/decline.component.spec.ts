import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { DeclineComponent } from './decline.component';

describe('DeclineComponent', () => {
  let component: DeclineComponent;
  let fixture: ComponentFixture<DeclineComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ DeclineComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DeclineComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
