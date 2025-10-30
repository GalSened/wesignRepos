import { ComponentFixture, TestBed } from '@angular/core/testing';

import { IedasSigningFlowAfterFirstAuthComponent } from './iedas-signing-flow-after-first-auth.component';

describe('IedasSigningFlowAfterFirstAuthComponent', () => {
  let component: IedasSigningFlowAfterFirstAuthComponent;
  let fixture: ComponentFixture<IedasSigningFlowAfterFirstAuthComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ IedasSigningFlowAfterFirstAuthComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(IedasSigningFlowAfterFirstAuthComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
