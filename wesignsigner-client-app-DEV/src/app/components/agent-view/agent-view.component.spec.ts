import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { AgentViewComponent } from './agent-view.component';

describe('AgentViewComponent', () => {
  let component: AgentViewComponent;
  let fixture: ComponentFixture<AgentViewComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ AgentViewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AgentViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
