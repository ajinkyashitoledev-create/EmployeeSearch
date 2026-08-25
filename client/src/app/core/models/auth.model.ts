export type UserRole = 'Admin' | 'User';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface AuthResponse {
  token: string;
  expiresAtUtc: string;
  userId: number;
  username: string;
  role: UserRole;
}

export interface CurrentUser {
  userId: number;
  username: string;
  role: UserRole;
  expiresAtUtc: string;
}
