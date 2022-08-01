import {Inject, Injectable} from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
} from '@angular/common/http';
import {Observable} from 'rxjs';
import {AuthService} from '../services/auth.service';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService,
              @Inject('BASE_URL') private baseUrl: string) {
  }

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const isApiUrl = request.url.startsWith(this.baseUrl);
    if (this.authService.accessToken && isApiUrl) {
      request = request.clone({
        setHeaders: {authorization: `Bearer ${this.authService.accessToken}`},
      });
    }

    return next.handle(request);
  }
}
