import { TestBed } from '@angular/core/testing';

import { SingleLinkService } from './single-link.service';

describe('SingleLinkService', () => {
  let service: SingleLinkService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SingleLinkService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
