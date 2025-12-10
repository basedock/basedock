import {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
  useRef,
  type ReactNode,
} from 'react';
import { authStore, type User } from '@/lib/auth-store';
import { client } from '@/api/client.gen';

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  refresh: () => Promise<boolean>;
}

const AuthContext = createContext<AuthContextType | null>(null);

// Configure API client for credentials (cookies)
client.setConfig({
  credentials: 'include',
});

// Refresh timer interval (4 minutes, before 5-min expiry)
const REFRESH_INTERVAL_MS = 4 * 60 * 1000;

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(authStore.getUser());
  const [isLoading, setIsLoading] = useState(true);
  const refreshTimerRef = useRef<number | null>(null);
  // Prevent concurrent refresh attempts (React StrictMode can trigger double calls)
  const isRefreshingRef = useRef(false);
  // Ensure initialization only runs once, even with StrictMode double-mounting
  const hasInitializedRef = useRef(false);

  const isAuthenticated = user !== null && authStore.isAuthenticated();

  const clearRefreshTimer = useCallback(() => {
    if (refreshTimerRef.current) {
      clearTimeout(refreshTimerRef.current);
      refreshTimerRef.current = null;
    }
  }, []);

  const refresh = useCallback(async (): Promise<boolean> => {
    // Prevent concurrent refresh attempts (React StrictMode can trigger double calls)
    if (isRefreshingRef.current) {
      return false;
    }
    isRefreshingRef.current = true;

    try {
      const response = await client.post({
        url: '/api/auth/refresh',
      });

      if (response.error || !response.data) {
        // Refresh failed, clear auth state
        authStore.clear();
        setUser(null);
        clearRefreshTimer();
        return false;
      }

      const data = response.data as {
        accessToken: string;
        accessTokenExpiresAt: string;
        user: User;
      };

      // Update auth store
      authStore.setAccessToken(
        data.accessToken,
        new Date(data.accessTokenExpiresAt)
      );
      authStore.setUser(data.user);
      setUser(data.user);

      return true;
    } catch {
      authStore.clear();
      setUser(null);
      clearRefreshTimer();
      return false;
    } finally {
      isRefreshingRef.current = false;
    }
  }, [clearRefreshTimer]);

  const scheduleRefresh = useCallback(() => {
    clearRefreshTimer();

    // Schedule refresh 1 minute before token expires
    const timeUntilExpiry = authStore.getTimeUntilExpiry();
    const refreshDelay = Math.max(0, timeUntilExpiry - 60000);

    if (refreshDelay > 0) {
      refreshTimerRef.current = window.setTimeout(async () => {
        const success = await refresh();
        if (success) {
          scheduleRefresh();
        }
      }, Math.min(refreshDelay, REFRESH_INTERVAL_MS));
    }
  }, [clearRefreshTimer, refresh]);

  const login = useCallback(
    async (email: string, password: string): Promise<void> => {
      const response = await client.post({
        url: '/api/auth/login',
        body: { email, password },
        headers: {
          'Content-Type': 'application/json',
        },
      });

      if (response.error) {
        const errorData = response.error as { detail?: string };
        throw new Error(errorData.detail || 'Login failed');
      }

      const data = response.data as {
        accessToken: string;
        accessTokenExpiresAt: string;
        user: User;
      };

      // Store tokens in memory
      authStore.setAccessToken(
        data.accessToken,
        new Date(data.accessTokenExpiresAt)
      );
      authStore.setUser(data.user);
      setUser(data.user);

      // Schedule token refresh
      scheduleRefresh();
    },
    [scheduleRefresh]
  );

  const logout = useCallback(async (): Promise<void> => {
    try {
      const token = authStore.getAccessToken();
      await client.post({
        url: '/api/auth/logout',
        headers: token
          ? {
            Authorization: `Bearer ${token}`,
          }
          : undefined,
      });
    } catch {
      // Ignore errors during logout
    } finally {
      authStore.clear();
      setUser(null);
      clearRefreshTimer();
    }
  }, [clearRefreshTimer]);

  // Initialize: try to refresh on mount
  useEffect(() => {
    // Only initialize once, even with StrictMode double-mounting
    if (hasInitializedRef.current) {
      return;
    }
    hasInitializedRef.current = true;

    const initialize = async () => {
      setIsLoading(true);
      try {
        // If we have a valid token in sessionStorage, use it
        if (authStore.isAuthenticated()) {
          setUser(authStore.getUser());
          scheduleRefresh();
          setIsLoading(false);
          return;
        }

        // Try to refresh using the cookie
        const success = await refresh();
        if (success) {
          scheduleRefresh();
        }
      } catch {
        authStore.clear();
        setUser(null);
      } finally {
        setIsLoading(false);
      }
    };

    initialize();

    return () => {
      clearRefreshTimer();
    };
  }, [refresh, scheduleRefresh, clearRefreshTimer]);

  // Add auth header to all requests
  useEffect(() => {
    const requestInterceptor = async (request: Request) => {
      const token = authStore.getAccessToken();
      if (token) {
        const headers = new Headers(request.headers);
        headers.set('Authorization', `Bearer ${token}`);

        const init: RequestInit = {
          method: request.method,
          headers,
          body: request.body,
          mode: request.mode,
          credentials: 'include',  // Explicitly set to 'include' instead of copying
          cache: request.cache,
          redirect: request.redirect,
          referrer: request.referrer,
          integrity: request.integrity,
        };

        // Only add duplex if body exists (for POST/PUT requests)
        if (request.body) {
          (init as any).duplex = 'half';
        }

        return new Request(request.url, init);
      }
      return request;
    };

    client.interceptors.request.use(requestInterceptor);

    return () => {
      client.interceptors.request.eject(requestInterceptor);
    };
  }, []);

  const value: AuthContextType = {
    user,
    isAuthenticated,
    isLoading,
    login,
    logout,
    refresh,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextType {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
