import {Injector} from '@angular/core';
import {AuthService} from './auth.service';

export const appInitializer = (injector: Injector) =>
  () => new Promise((resolve) => {
    injector.get(AuthService).refreshToken().subscribe().add(resolve);
  });
