// In-memory token storage (NOT localStorage - XSS protection)
// Tokens are stored in memory and lost on page refresh
// This is intentional - user will need to re-authenticate or use refresh token

export interface User {
  id: string;
  email: string;
  displayName: string;
  isAdmin: boolean;
  createdAt: string;
  updatedAt: string | null;
}

interface AuthState {
  accessToken: string | null;
  accessTokenExpiresAt: Date | null;
  user: User | null;
}

const state: AuthState = {
  accessToken: null,
  accessTokenExpiresAt: null,
  user: null,
};

export const authStore = {
  getAccessToken: (): string | null => state.accessToken,

  setAccessToken: (token: string | null, expiresAt?: Date | null): void => {
    state.accessToken = token;
    state.accessTokenExpiresAt = expiresAt ?? null;
  },

  getUser: (): User | null => state.user,

  setUser: (user: User | null): void => {
    state.user = user;
  },

  isAuthenticated: (): boolean => {
    if (!state.accessToken || !state.user) {
      return false;
    }
    // Check if token is expired
    if (state.accessTokenExpiresAt && state.accessTokenExpiresAt < new Date()) {
      return false;
    }
    return true;
  },

  isTokenExpiringSoon: (thresholdMs: number = 60000): boolean => {
    if (!state.accessTokenExpiresAt) {
      return false;
    }
    const timeUntilExpiry = state.accessTokenExpiresAt.getTime() - Date.now();
    return timeUntilExpiry < thresholdMs;
  },

  getTimeUntilExpiry: (): number => {
    if (!state.accessTokenExpiresAt) {
      return 0;
    }
    return Math.max(0, state.accessTokenExpiresAt.getTime() - Date.now());
  },

  clear: (): void => {
    state.accessToken = null;
    state.accessTokenExpiresAt = null;
    state.user = null;
  },
};
