import { InjectionToken, Signal } from '@angular/core';
import { AuthResponse, AuthState, RegisterRequest, SignInRequest } from '../models';

export interface IAuthService {
  readonly authState: Signal<AuthState | null>;
  readonly isAuthenticated: Signal<boolean>;
  readonly currentUserName: Signal<string | null>;
  signIn(request: SignInRequest): Promise<AuthResponse>;
  register(request: RegisterRequest): Promise<AuthResponse>;
  logout(): void;
  getToken(): string | null;
}

export const AUTH_SERVICE = new InjectionToken<IAuthService>('AUTH_SERVICE');
