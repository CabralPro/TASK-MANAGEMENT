import { Injectable } from '@angular/core';
import { ITokenStorage } from '../interfaces/token-storage.interface';

const TOKEN_KEY = 'tm_access_token';
const USER_KEY = 'tm_user';

@Injectable({ providedIn: 'root' })
export class TokenStorageService implements ITokenStorage {
  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  setToken(token: string): void {
    localStorage.setItem(TOKEN_KEY, token);
  }

  getUserJson(): string | null {
    return localStorage.getItem(USER_KEY);
  }

  setUserJson(userJson: string): void {
    localStorage.setItem(USER_KEY, userJson);
  }

  clear(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  }
}
