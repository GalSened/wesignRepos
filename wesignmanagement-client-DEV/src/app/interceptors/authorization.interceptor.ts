import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, BehaviorSubject, of } from 'rxjs';
import { catchError, filter, take, switchMap } from 'rxjs/operators';
import { UsersApiService } from '../services/users-api.service';

@Injectable()
export class UnauthorizedInterceptor implements HttpInterceptor {

    private isRefreshing = false;
    private refreshTokenSubject: BehaviorSubject<any> = new BehaviorSubject<any>(null);

    constructor(public authService: UsersApiService) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

        if (this.authService.accessToken) {
            request = this.addToken(request, this.authService.accessToken);
        }

        return next.handle(request).pipe(catchError(error => {
            if (error instanceof HttpErrorResponse) {
                if (error.status === 401) {
                    return this.handle401Error(request, next);
                }
                if ((error.status === 400 && request.headers.has("token-expired")) || error.status === 0  ) {
                    this.authService.logout();                    
                    return next.handle(request);
                }
                
            }
            return throwError(error);
        }));
    }

    private addToken(request: HttpRequest<any>, token: string) {
        return request.clone({
            setHeaders: {
                'Authorization': `Bearer ${token}`
            }
        });
    }

    private handle401Error(request: HttpRequest<any>, next: HttpHandler) {
        if (!this.isRefreshing) {
            this.isRefreshing = true;
            this.refreshTokenSubject.next(null);

            return this.authService.updateToken().pipe(
                switchMap((token: any) => {
                    this.isRefreshing = false;
                    this.refreshTokenSubject.next(token.token);
                    return next.handle(this.addToken(request, token.token));
                }));

        } else {
            this.isRefreshing = false;
            return this.refreshTokenSubject.pipe(
                filter(token => token != null),
                take(1),
                switchMap(jwt => {
                    return next.handle(this.addToken(request, jwt));
                })
            );
        }
    }
}
