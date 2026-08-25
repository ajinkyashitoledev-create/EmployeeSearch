import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';
import { ProblemDetails } from '../models/problem-details.model';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notifications = inject(NotificationService);
  const auth = inject(AuthService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const isAuthEndpoint = req.url.includes('/auth/login') || req.url.includes('/auth/register');

      if (error.status === 401 && !isAuthEndpoint) {
        notifications.error('Your session has expired. Please log in again.');
        auth.logout();
        return throwError(() => error);
      }

      if (error.status === 403) {
        notifications.error("You don't have permission to do that.");
        return throwError(() => error);
      }

      if (error.status === 0) {
        notifications.error('Could not reach the server. Is the API running?');
        return throwError(() => error);
      }

      notifications.error(extractMessage(error));
      return throwError(() => error);
    })
  );
};

function extractMessage(error: HttpErrorResponse): string {
  const body = error.error as ProblemDetails | { message?: string } | null;

  if (body && typeof body === 'object') {
    if ('errors' in body && body.errors) {
      const firstField = Object.values(body.errors)[0];
      if (firstField?.length) return firstField[0];
    }
    if ('detail' in body && body.detail) return body.detail;
    if ('title' in body && body.title) return body.title;
    if ('message' in body && body.message) return body.message;
  }

  return `Request failed (${error.status}).`;
}
