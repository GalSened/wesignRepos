import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { DocumentsGroupComponent } from './documents-group.component';

describe('DocumentsGroupComponent', () => {
  let component: DocumentsGroupComponent;
  let fixture: ComponentFixture<DocumentsGroupComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ DocumentsGroupComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DocumentsGroupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
