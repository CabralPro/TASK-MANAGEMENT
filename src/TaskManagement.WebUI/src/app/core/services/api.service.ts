import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  get<T>(path: string): Promise<ApiResponse<T>> {
    return this.request(this.http.get<ApiResponse<T>>(this.buildUrl(path)));
  }

  post<T>(path: string, body: unknown): Promise<ApiResponse<T>> {
    return this.request(this.http.post<ApiResponse<T>>(this.buildUrl(path), body));
  }

  put<T>(path: string, body: unknown): Promise<ApiResponse<T>> {
    return this.request(this.http.put<ApiResponse<T>>(this.buildUrl(path), body));
  }

  delete<T>(path: string): Promise<ApiResponse<T>> {
    return this.request(this.http.delete<ApiResponse<T>>(this.buildUrl(path)));
  }

  private async request<T>(source$: Observable<ApiResponse<T>>): Promise<ApiResponse<T>> {
    try {
      return await firstValueFrom(source$);
    } catch (error) {
      return this.handleError<T>(error);
    }
  }

  private buildUrl(path: string): string {
    return `${this.baseUrl}${path.startsWith('/') ? path : `/${path}`}`;
  }

  private handleError<T>(error: unknown): ApiResponse<T> {
    const httpError = error as HttpErrorResponse;
    const apiError = httpError.error as ApiResponse<T> | null;
    const message =
      apiError?.message ??
      (Array.isArray(apiError?.errors) ? apiError.errors.join(', ') : null) ??
      httpError.message ??
      'An unexpected error occurred';

    return {
      success: false,
      data: null as T,
      message,
      errors: apiError?.errors ?? []
    };
  }
}
