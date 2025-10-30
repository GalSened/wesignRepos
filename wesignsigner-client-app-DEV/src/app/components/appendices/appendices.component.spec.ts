import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { AppendicesComponent } from './appendices.component';

describe('AppendicesComponent', () => {
  let component: AppendicesComponent;
  let fixture: ComponentFixture<AppendicesComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ AppendicesComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AppendicesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
