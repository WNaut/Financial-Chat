import {Component} from '@angular/core';
import {FormControl, FormGroup, Validators} from '@angular/forms';
import {NotificationService} from '../../core/services/notification.service';
import {ActivatedRoute, Router} from '@angular/router';
import {AuthService} from '../../core/services/auth.service';
import {User} from '../../core/models/account/User.model';

@Component({
  selector: 'app-register',
  templateUrl: 'register.component.html'
})
export class RegisterComponent {
  loginForm = new FormGroup({
    username: new FormControl('', [Validators.required]),
    password: new FormControl('', [Validators.required]),
    status: new FormControl(true)
  });

  constructor(private authService: AuthService,
              private notificationService: NotificationService,
              private activatedRoute: ActivatedRoute,
              private router: Router) {
  }

  submit() {
    if (this.loginForm.valid) {
      const loginModel = this.loginForm.getRawValue() as User;

      this.notificationService.startLoading();
      this.authService.register(loginModel)
        .then(() => {
            this.router.navigate(['/login']);
            this.notificationService.success('Success!', 'Registered successfully!');
            this.notificationService.endLoading();
          },
          errorMessage => {
            this.notificationService.error('Error', errorMessage);
            this.notificationService.endLoading();
          });
    }
  }
}
