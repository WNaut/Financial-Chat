import {Component} from '@angular/core';
import {FormControl, FormGroup, Validators} from '@angular/forms';
import LoginModel from '../../core/models/account/login.model';
import {NotificationService} from '../../core/services/notification.service';
import {ActivatedRoute, Router} from '@angular/router';
import {AuthService} from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: 'login.component.html'
})
export class LoginComponent {
  loginForm = new FormGroup({
    username: new FormControl(''),
    password: new FormControl(''),
  });

  constructor(private authService: AuthService,
              private notificationService: NotificationService,
              private activatedRoute: ActivatedRoute,
              private router: Router) {
  }

  submit() {
    if (this.loginForm.valid) {
      const loginModel = this.loginForm.getRawValue() as LoginModel;

      this.notificationService.startLoading();
      this.authService.login(loginModel)
        .subscribe(() => {
            this.router.navigate(['/']);
          },
          errorMessage => {
            this.notificationService.error('Error', errorMessage);
          })
        .add(() => this.notificationService.endLoading());
    }
  }

  goToRegister() {
    this.router.navigate(['/register']);
  }
}
