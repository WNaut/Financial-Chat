import {BrowserModule} from '@angular/platform-browser';
import {APP_INITIALIZER, Injector, NgModule} from '@angular/core';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {HttpClientModule} from '@angular/common/http';
import {RouterModule} from '@angular/router';
import {AppComponent} from './app.component';
import {BlockUIModule} from 'ng-block-ui';
import {ToastrModule} from 'ngx-toastr';
import {LoginComponent} from './pages/login/login.component';
import {routes} from './app.routing';
import {CoreModule} from './core/core.module';
import {ChatBoxComponent} from './pages/chat-box/chat-box.component';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {RegisterComponent} from './pages/register/register.component';
import {appInitializer} from './core/services/app-initializer';

@NgModule({
  declarations: [
    AppComponent,
    ChatBoxComponent,
    LoginComponent,
    RegisterComponent
  ],
  imports: [
    BrowserModule.withServerTransition({appId: 'ng-cli-universal'}),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot(routes),
    BlockUIModule.forRoot(),
    BrowserAnimationsModule,
    ToastrModule.forRoot(),
    CoreModule,
    ReactiveFormsModule
  ],
  providers: [
    {
      provide: APP_INITIALIZER,
      useFactory: appInitializer,
      multi: true,
      deps: [Injector],
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule {
}
