import { UserToken } from '../models/Entities';

/** localStorage key holding the logged-in user's token payload. */
export const CURRENT_USER_STORAGE_KEY = 'currentUser';

/** Safely retrieves and parses the current user from localStorage. */
export function getStoredUser(): UserToken | null {
  try {
    const raw = localStorage.getItem(CURRENT_USER_STORAGE_KEY);
    if (!raw) return null;
    return JSON.parse(raw) as UserToken;
  } catch {
    return null;
  }
}

export function setStoredUser(user: UserToken): void {
  localStorage.setItem(CURRENT_USER_STORAGE_KEY, JSON.stringify(user));
}

export function clearStoredUser(): void {
  localStorage.removeItem(CURRENT_USER_STORAGE_KEY);
}

/** Safely decodes a JWT payload (URL-safe base64, UTF-8). */
export function parseJwtPayload<T = any>(token: string): T | null {
  if (!token || typeof token !== 'string') return null;
  const parts = token.split('.');
  if (parts.length < 2) return null;

  try {
    let base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
    while (base64.length % 4 !== 0) {
      base64 += '=';
    }
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    return JSON.parse(jsonPayload) as T;
  } catch {
    return null;
  }
}

/**
 * Checks if a JWT token is expired. Returns true if expired or invalid:
 * every server token (AuthController.GenerateToken) carries `exp`.
 */
export function isTokenExpired(token: string): boolean {
  const payload = parseJwtPayload<{ exp?: number }>(token);
  if (!payload || !payload.exp) {
    return true;
  }
  return payload.exp * 1000 < Date.now();
}

/** Whether the user has a valid, non-expired token. */
export function isUserAuthenticated(user?: UserToken | null): boolean {
  const u = user || getStoredUser();
  if (!u || !u.access_token) return false;
  return !isTokenExpired(u.access_token);
}
