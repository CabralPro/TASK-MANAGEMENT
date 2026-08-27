export interface ITokenStorage {
  getToken(): string | null;
  setToken(token: string): void;
  getUserJson(): string | null;
  setUserJson(userJson: string): void;
  clear(): void;
}
