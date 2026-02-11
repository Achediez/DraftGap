// Payload for /auth/login
export interface LoginRequest {
  email: string;
  password: string;
}

// Payload for /auth/register
export interface RegisterRequest {
  email: string;
  password: string;
  riotId: string;
  region: string;
}

// Response from auth endpoints.
export interface AuthResponse {
  token: string;
  email: string;
  riotId: string;
  puuid?: string;
  region?: string;
  isAdmin: boolean;
  expiresAt: string;
}
