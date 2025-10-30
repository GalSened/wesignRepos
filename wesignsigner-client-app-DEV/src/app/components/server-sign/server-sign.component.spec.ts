import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { ServerSignComponent } from './server-sign.component';

describe('ServerSignComponent', () => {
  let component: ServerSignComponent;
  let fixture: ComponentFixture<ServerSignComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ServerSignComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ServerSignComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
