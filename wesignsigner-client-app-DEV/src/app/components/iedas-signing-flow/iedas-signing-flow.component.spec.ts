import { ComponentFixture, TestBed } from '@angular/core/testing';

import { IedasSigningFlowComponent } from './iedas-signing-flow.component';

describe('IedasSigningFlowComponent', () => {
  let component: IedasSigningFlowComponent;
  let fixture: ComponentFixture<IedasSigningFlowComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ IedasSigningFlowComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(IedasSigningFlowComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
