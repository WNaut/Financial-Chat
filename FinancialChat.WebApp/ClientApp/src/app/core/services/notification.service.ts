import { Injectable } from '@angular/core';
import {ToastrService} from 'ngx-toastr';
import {BlockUI, NgBlockUI} from 'ng-block-ui';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  @BlockUI() blockUI: NgBlockUI;
  constructor(private toastr: ToastrService) { }

  success(title: string , message: string) {
    this.toastr.success(message, title);
  }

  error(title: string , message: string) {
    this.toastr.error(message, title);
  }

  info(title: string , message: string) {
    this.toastr.info(message, title);
  }

  warning(title: string , message: string) {
    this.toastr.warning(message, title);
  }

  startLoading(message = 'Cargando...') {
    this.blockUI.start(message);
  }

  endLoading() {
    this.blockUI.stop();
  }
}
