import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { MainSignerComponent } from './main-signer.component';

describe('MainSignerComponent', () => {
  let component: MainSignerComponent;
  let fixture: ComponentFixture<MainSignerComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ MainSignerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MainSignerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
