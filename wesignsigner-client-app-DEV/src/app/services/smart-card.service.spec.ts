import { TestBed } from '@angular/core/testing';

import { SmartCardService } from './smart-card.service';

describe('SmartCardService', () => {
  let service: SmartCardService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SmartCardService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
