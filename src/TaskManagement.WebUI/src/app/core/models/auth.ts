export interface RegisterRequest {
  userName: string;
  email: string;
  password: string;
}

export interface SignInRequest {
  userName: string;
  password: string;
}

export interface AuthResponse {
  token: string | null;
  userId: string;
  name: string;
  email: string;
  role: string;
}

export interface AuthUser {
  userId: string;
  name: string;
  email: string;
  role: string;
}

export interface AuthState {
  token: string;
  user: AuthUser;
  isAuthenticated: true;
}
