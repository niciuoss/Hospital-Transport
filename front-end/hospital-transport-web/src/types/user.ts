export interface UserData {
  id: string;
  fullName: string;
  username: string;
  role: string;
  createdAt: string;
  isActive: boolean;
}

export interface CreateUserRequest {
  fullName: string;
  username: string;
  password: string;
  role: string;
}