import { HttpClient, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiService } from './api.service';

describe('ApiService', () => {
  let service: ApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ApiService, provideHttpClient(), provideHttpClientTesting()]
    });

    service = TestBed.inject(ApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('get returns successful envelope', async () => {
    const pending = service.get<{ id: string }>('/api/v1/items');

    const req = httpMock.expectOne(`${environment.apiUrl}/api/v1/items`);
    expect(req.request.method).toBe('GET');
    req.flush({ success: true, data: { id: '1' }, message: null, errors: [] });

    await expect(pending).resolves.toEqual({
      success: true,
      data: { id: '1' },
      message: null,
      errors: []
    });
  });

  it('post/put/delete use matching verbs and normalize paths', async () => {
    const postPending = service.post<string>('api/v1/items', { title: 'a' });
    const postReq = httpMock.expectOne(`${environment.apiUrl}/api/v1/items`);
    expect(postReq.request.method).toBe('POST');
    postReq.flush({ success: true, data: 'created', message: null, errors: [] });
    await expect(postPending).resolves.toMatchObject({ success: true, data: 'created' });

    const putPending = service.put<string>('/api/v1/items/1', { title: 'b' });
    const putReq = httpMock.expectOne(`${environment.apiUrl}/api/v1/items/1`);
    expect(putReq.request.method).toBe('PUT');
    putReq.flush({ success: true, data: 'updated', message: null, errors: [] });
    await expect(putPending).resolves.toMatchObject({ success: true, data: 'updated' });

    const deletePending = service.delete<boolean>('/api/v1/items/1');
    const deleteReq = httpMock.expectOne(`${environment.apiUrl}/api/v1/items/1`);
    expect(deleteReq.request.method).toBe('DELETE');
    deleteReq.flush({ success: true, data: true, message: null, errors: [] });
    await expect(deletePending).resolves.toMatchObject({ success: true, data: true });
  });

  it('maps API error envelope message on HTTP failure', async () => {
    const pending = service.get<string>('/api/v1/fail');

    const req = httpMock.expectOne(`${environment.apiUrl}/api/v1/fail`);
    req.flush(
      { success: false, data: null, message: 'Nope', errors: ['a'] },
      { status: 400, statusText: 'Bad Request' }
    );

    await expect(pending).resolves.toEqual({
      success: false,
      data: null,
      message: 'Nope',
      errors: ['a']
    });
  });

  it('joins API error array when message is missing', async () => {
    const pending = service.get<string>('/api/v1/fail-errors');

    const req = httpMock.expectOne(`${environment.apiUrl}/api/v1/fail-errors`);
    req.flush(
      { success: false, data: null, message: null, errors: ['one', 'two'] },
      { status: 422, statusText: 'Unprocessable' }
    );

    await expect(pending).resolves.toMatchObject({
      success: false,
      message: 'one, two',
      errors: ['one', 'two']
    });
  });

  it('falls back to HTTP status text when body has no message', async () => {
    const pending = service.get<string>('/api/v1/network');

    const req = httpMock.expectOne(`${environment.apiUrl}/api/v1/network`);
    req.flush(null, { status: 500, statusText: 'Server Error' });

    const result = await pending;
    expect(result.success).toBe(false);
    expect(result.data).toBeNull();
    expect(result.errors).toEqual([]);
    expect(result.message).toBeTruthy();
  });

  it('uses unexpected fallback when error has no message details', async () => {
    const http = TestBed.inject(HttpClient);
    vi.spyOn(http, 'get').mockReturnValue(
      throwError(() => ({ error: null, message: undefined })) as never
    );

    await expect(service.get<string>('/ignored')).resolves.toEqual({
      success: false,
      data: null,
      message: 'An unexpected error occurred',
      errors: []
    });
  });
});
