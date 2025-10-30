import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { DocumentPageComponent } from './document-page.component';

describe('DocumentPageComponent', () => {
  let component: DocumentPageComponent;
  let fixture: ComponentFixture<DocumentPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ DocumentPageComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DocumentPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
