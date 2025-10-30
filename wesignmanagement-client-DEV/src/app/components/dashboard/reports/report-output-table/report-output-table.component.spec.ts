import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ReportOutputTableComponent } from './report-output-table.component';

describe('ReportOutputTableComponent', () => {
  let component: ReportOutputTableComponent;
  let fixture: ComponentFixture<ReportOutputTableComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ReportOutputTableComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ReportOutputTableComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
