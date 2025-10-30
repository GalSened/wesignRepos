import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { DownloadDocumentComponent } from './download-document.component';

describe('DownloadDocumentComponent', () => {
  let component: DownloadDocumentComponent;
  let fixture: ComponentFixture<DownloadDocumentComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ DownloadDocumentComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DownloadDocumentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
