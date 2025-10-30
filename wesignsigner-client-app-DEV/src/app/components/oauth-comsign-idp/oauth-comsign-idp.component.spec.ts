import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OauthComsignIdpComponent } from './oauth-comsign-idp.component';

describe('OauthComsignIdpComponent', () => {
  let component: OauthComsignIdpComponent;
  let fixture: ComponentFixture<OauthComsignIdpComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ OauthComsignIdpComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(OauthComsignIdpComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
