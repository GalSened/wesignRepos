import { TestBed } from '@angular/core/testing';

import { SignPadService } from './sign-pad.service';

describe('SignPadService', () => {
  let service: SignPadService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SignPadService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
