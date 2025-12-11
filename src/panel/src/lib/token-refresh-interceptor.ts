import { authStore } from './auth-store';

// Queue of requests waiting for token refresh to complete
interface PendingRequest {
  originalRequest: Request;
  originalFetch: typeof fetch;
  resolve: (value: Response) => void;
  reject: (reason: Error) => void;
}

const pendingRequests: PendingRequest[] = [];
let isRefreshing = false;

const isAuthEndpoint = (url: string): boolean => {
  return (
    url.includes('/api/auth/login') ||
    url.includes('/api/auth/refresh') ||
    url.includes('/api/auth/logout')
  );
};

// Called by auth-context to reset refresh flag on logout
export const resetRefreshState = (): void => {
  isRefreshing = false;
  pendingRequests.length = 0;
};

export const createTokenRefreshInterceptor = (
  refreshFn: () => Promise<boolean>
) => {
  return async (
    response: Response,
    request: Request,
    options: any
  ): Promise<Response> => {
    // Only handle 401 errors from non-auth endpoints
    if (response.status !== 401 || isAuthEndpoint(response.url)) {
      return response;
    }

    // If we're already refreshing, queue this request
    if (isRefreshing) {
      return new Promise<Response>((resolve, reject) => {
        pendingRequests.push({
          originalRequest: request,
          originalFetch: options.fetch || globalThis.fetch,
          resolve,
          reject,
        });
      });
    }

    isRefreshing = true;

    try {
      const refreshSuccess = await refreshFn();

      if (refreshSuccess) {
        // Get new token
        const newToken = authStore.getAccessToken();

        // Clone the original request with updated Authorization header
        const headers = new Headers(request.headers);
        headers.set('Authorization', `Bearer ${newToken}`);

        const retryRequest = new Request(request.url, {
          method: request.method,
          headers,
          body: request.body,
          mode: request.mode,
          credentials: 'include',
          cache: request.cache,
          redirect: request.redirect,
          referrer: request.referrer,
          integrity: request.integrity,
        });

        // Retry the request
        const fetchFn = options.fetch || globalThis.fetch;
        const retryResponse = await fetchFn(retryRequest);

        // Process queued requests
        const queued = pendingRequests.splice(0);
        for (const req of queued) {
          try {
            // Update queued request with new token
            const queuedHeaders = new Headers(req.originalRequest.headers);
            queuedHeaders.set('Authorization', `Bearer ${newToken}`);

            const queuedRequest = new Request(req.originalRequest.url, {
              method: req.originalRequest.method,
              headers: queuedHeaders,
              body: req.originalRequest.body,
              mode: req.originalRequest.mode,
              credentials: 'include',
              cache: req.originalRequest.cache,
              redirect: req.originalRequest.redirect,
              referrer: req.originalRequest.referrer,
              integrity: req.originalRequest.integrity,
            });

            const queuedRetry = await req.originalFetch(queuedRequest);
            req.resolve(queuedRetry);
          } catch (error) {
            req.reject(error as Error);
          }
        }

        return retryResponse;
      } else {
        // Refresh failed, return original 401
        // Clear queued requests
        const queued = pendingRequests.splice(0);
        for (const req of queued) {
          req.reject(new Error('Token refresh failed'));
        }

        return response;
      }
    } catch (error) {
      // Refresh threw an error, treat as failure
      const queued = pendingRequests.splice(0);
      for (const req of queued) {
        req.reject(error as Error);
      }

      return response;
    } finally {
      isRefreshing = false;
    }
  };
};
