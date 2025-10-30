import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { HtmlTemplateFormComponent } from './html-template-form.component';

describe('HtmlTemplateFormComponent', () => {
  let component: HtmlTemplateFormComponent;
  let fixture: ComponentFixture<HtmlTemplateFormComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ HtmlTemplateFormComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(HtmlTemplateFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
