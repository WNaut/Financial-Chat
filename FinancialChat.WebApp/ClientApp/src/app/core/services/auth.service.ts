import {Inject, Injectable} from '@angular/core';
import {Router} from '@angular/router';
import {HttpClient} from '@angular/common/http';
import {BehaviorSubject, Observable, of, Subscription} from 'rxjs';
import {delay, finalize, map, tap} from 'rxjs/operators';
import {ApplicationUser} from '../models/account/application-user';
import LoginModel from '../models/account/login.model';
import {TokenResponseModel} from '../models/account/tokenResponse.model';
import BaseResponseModel from '../models/base-response.model';
import {User} from '../models/account/User.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  _user = new BehaviorSubject<ApplicationUser>(null);
  currentUser$: Observable<ApplicationUser> = this._user.asObservable();
  private readonly apiUrl = `${this.baseUrl}api/account`;
  private timer: Subscription;

  constructor(private router: Router, private http: HttpClient,
              @Inject('BASE_URL') private baseUrl: string) {
  }

  get accessToken(): string {
    return localStorage.getItem('access_token');
  }

  login(model: LoginModel) {
    return this.http.post<BaseResponseModel<TokenResponseModel>>(`${this.apiUrl}/login`, model)
      .pipe(
        map((result) => {
          const tokenResponse = result.data;

          this.setUserState(tokenResponse);
          this.setLocalStorage(tokenResponse);
          this.startTokenTimer();
          return tokenResponse;
        })
      );
  }

  register(user: User) {
    return this.http.post<BaseResponseModel<null>>(`${this.apiUrl}/register`, user).toPromise();
  }

  logout() {
    return this.http
      .post(`${this.apiUrl}/logout`, {})
      .pipe(
        finalize(() => {
          this.clearLocalStorage();
          this._user.next(null);
          this.stopTokenTimer();
          this.router.navigate(['login']);
        })
      )
      .toPromise();
  }

  refreshToken() {
    const refreshToken = localStorage.getItem('refresh_token');
    if (!refreshToken) {
      this.clearLocalStorage();
      return of(null);
    }

    return this.http.post<BaseResponseModel<TokenResponseModel>>(`${this.apiUrl}/refresh-token`, {refreshToken})
      .pipe(
        map(({data: tokenResponse}) => {
          this.setUserState(tokenResponse);
          this.setLocalStorage(tokenResponse);
          this.startTokenTimer();
          return tokenResponse;
        })
      );
  }

  setUserState(applicationUser: ApplicationUser) {
    this._user.next({
      userId: applicationUser.userId,
      username: applicationUser.username
    });
  }

  setLocalStorage(tokenResponse: TokenResponseModel) {
    localStorage.setItem('access_token', tokenResponse.accessToken);
    localStorage.setItem('refresh_token', tokenResponse.refreshToken);
  }

  clearLocalStorage() {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
  }

  private storageEventListener(event: StorageEvent) {
    if (event.storageArea === localStorage) {
      if (event.key === 'logout-event') {
        this.stopTokenTimer();
        this._user.next(null);
      }
    }
  }

  private startTokenTimer() {
    const timeout = this.getTokenRemainingTime();
    this.timer = of(true)
      .pipe(
        delay(timeout),
        tap(() => this.refreshToken().subscribe())
      )
      .subscribe();
  }

  private getTokenRemainingTime() {
    if (!this.accessToken) {
      return 0;
    }
    const jwtToken = JSON.parse(atob(this.accessToken.split('.')[1]));
    const expires = new Date(jwtToken.exp * 1000);
    return expires.getTime() - Date.now();
  }

  private stopTokenTimer() {
    this.timer.unsubscribe();
  }
}
