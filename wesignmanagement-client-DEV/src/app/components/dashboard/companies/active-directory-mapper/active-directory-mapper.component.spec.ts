import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { ActiveDirectoryMapperComponent } from './active-directory-mapper.component';

describe('ActiveDirectoryMapperComponent', () => {
  let component: ActiveDirectoryMapperComponent;
  let fixture: ComponentFixture<ActiveDirectoryMapperComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ActiveDirectoryMapperComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ActiveDirectoryMapperComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
