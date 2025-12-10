// Session storage for auth tokens
// Uses sessionStorage for persistence across page refreshes within the same tab
// sessionStorage is cleared when the browser tab/window is closed

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

const STORAGE_KEY = 'auth_state';

function loadState(): AuthState {
  try {
    const stored = sessionStorage.getItem(STORAGE_KEY);
    if (stored) {
      const parsed = JSON.parse(stored);
      return {
        ...parsed,
        accessTokenExpiresAt: parsed.accessTokenExpiresAt
          ? new Date(parsed.accessTokenExpiresAt)
          : null,
      };
    }
  } catch {
    // Ignore errors (e.g., invalid JSON, no sessionStorage)
  }
  return { accessToken: null, accessTokenExpiresAt: null, user: null };
}

function saveState(state: AuthState): void {
  try {
    sessionStorage.setItem(STORAGE_KEY, JSON.stringify(state));
  } catch {
    // Ignore errors (e.g., quota exceeded)
  }
}

let state: AuthState = loadState();

export const authStore = {
  getAccessToken: (): string | null => state.accessToken,

  setAccessToken: (token: string | null, expiresAt?: Date | null): void => {
    state.accessToken = token;
    state.accessTokenExpiresAt = expiresAt ?? null;
    saveState(state);
  },

  getUser: (): User | null => state.user,

  setUser: (user: User | null): void => {
    state.user = user;
    saveState(state);
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
    sessionStorage.removeItem(STORAGE_KEY);
  },
};
