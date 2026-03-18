export interface User {
  id: string;
  name: string;
  email: string;
  createdAt?: string;
  avatarUrl?: string;
  timezone?: string;
}

export interface UpdateProfileRequest {
  name?: string;
  email?: string;
  timezone?: string;
  avatarUrl?: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiry: string;
  user: User;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}
