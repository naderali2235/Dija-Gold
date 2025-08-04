import { NgModule, Optional, SkipSelf } from '@angular/core';
import { CommonModule } from '@angular/common';

// Services
import { AuthService } from './services/auth.service';
import { ApiService, ApiHelperService } from './services/api.service';
import { NotificationService } from './services/notification.service';

// Guards
import { AuthGuard } from './guards/auth.guard';
import { RoleGuard } from './guards/role.guard';

// Ensure core module is only imported once
function throwIfAlreadyLoaded(parentModule: any, moduleName: string) {
  if (parentModule) {
    throw new Error(`${moduleName} has already been loaded. Import Core modules in the AppModule only.`);
  }
}

@NgModule({
  declarations: [],
  imports: [
    CommonModule
  ],
  providers: [
    // Services
    AuthService,
    ApiService,
    ApiHelperService,
    NotificationService,
    
    // Guards
    AuthGuard,
    RoleGuard
  ]
})
export class CoreModule {
  constructor(@Optional() @SkipSelf() parentModule: CoreModule) {
    throwIfAlreadyLoaded(parentModule, 'CoreModule');
  }
}