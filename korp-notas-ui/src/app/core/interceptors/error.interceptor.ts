import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let message = 'Erro inesperado.';

      if (Array.isArray(error.error?.errors) && error.error.errors.length) {
        message = error.error.errors.join(' ');
      } else if (error.error?.message) {
        message = error.error.message;
      }

      return throwError(() => new Error(message));
    })
  );
};
