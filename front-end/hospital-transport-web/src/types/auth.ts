export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  userId: string;
  fullName: string;
  username: string;
  token: string;
}

export interface User {
  userId: string;
  fullName: string;
  username: string;
  token: string;
}