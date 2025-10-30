import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { LoadLicenseComponent } from './load-license.component';

describe('LoadLicenseComponent', () => {
  let component: LoadLicenseComponent;
  let fixture: ComponentFixture<LoadLicenseComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ LoadLicenseComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(LoadLicenseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
