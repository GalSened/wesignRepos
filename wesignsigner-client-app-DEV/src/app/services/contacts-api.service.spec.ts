import { TestBed } from '@angular/core/testing';

import { ContactsApiService } from './contacts-api.service';

describe('ContactsApiService', () => {
  let service: ContactsApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ContactsApiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
