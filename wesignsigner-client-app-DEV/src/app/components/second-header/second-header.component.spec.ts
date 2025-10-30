import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { SecondHeaderComponent } from './second-header.component';

describe('SecondHeaderComponent', () => {
  let component: SecondHeaderComponent;
  let fixture: ComponentFixture<SecondHeaderComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ SecondHeaderComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SecondHeaderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
