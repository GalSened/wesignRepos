import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { ShowPropertiesComponent } from './show-properties.component';

describe('ShowPropertiesComponent', () => {
  let component: ShowPropertiesComponent;
  let fixture: ComponentFixture<ShowPropertiesComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ShowPropertiesComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ShowPropertiesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
