import { Injectable, computed, inject, signal } from '@angular/core';
import { IAuthService } from '../interfaces/auth.service.interface';
import {
  AuthResponse,
  AuthState,
  AuthUser,
  RegisterRequest,
  SignInRequest
} from '../models';
import { TOKEN_STORAGE } from '../tokens/injection.tokens';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class AuthService implements IAuthService {
  private readonly api = inject(ApiService);
  private readonly tokenStorage = inject(TOKEN_STORAGE);

  private readonly authStateSignal = signal<AuthState | null>(this.buildInitialState());

  readonly authState = this.authStateSignal.asReadonly();
  readonly isAuthenticated = computed(() => this.authStateSignal()?.isAuthenticated ?? false);
  readonly currentUserName = computed(() => this.authStateSignal()?.user.name ?? null);

  async signIn(request: SignInRequest): Promise<AuthResponse> {
    const response = await this.api.post<AuthResponse>('/api/v1/auth/sign-in', request);
    this.ensureSuccess(response);
    this.persistSession(response.data);
    return response.data;
  }

  async register(request: RegisterRequest): Promise<AuthResponse> {
    const response = await this.api.post<AuthResponse>('/api/v1/auth/register', request);
    this.ensureSuccess(response);
    return response.data;
  }

  logout(): void {
    this.tokenStorage.clear();
    this.authStateSignal.set(null);
  }

  getToken(): string | null {
    return this.tokenStorage.getToken();
  }

  private persistSession(response: AuthResponse): void {
    if (!response.token) {
      throw new Error('Sign-in succeeded but no token was returned.');
    }

    const user: AuthUser = {
      userId: response.userId,
      name: response.name,
      email: response.email,
      role: response.role
    };

    this.tokenStorage.setToken(response.token);
    this.tokenStorage.setUserJson(JSON.stringify(user));
    this.authStateSignal.set({
      token: response.token,
      user,
      isAuthenticated: true
    });
  }

  private buildInitialState(): AuthState | null {
    const token = this.tokenStorage.getToken();
    const userJson = this.tokenStorage.getUserJson();

    if (!token || !userJson) {
      this.tokenStorage.clear();
      return null;
    }

    try {
      const user = JSON.parse(userJson) as AuthUser;
      return { token, user, isAuthenticated: true };
    } catch {
      this.tokenStorage.clear();
      return null;
    }
  }

  private ensureSuccess<T>(response: { success: boolean; message?: string | null }): void {
    if (!response.success) {
      throw new Error(response.message ?? 'Request failed');
    }
  }
}
