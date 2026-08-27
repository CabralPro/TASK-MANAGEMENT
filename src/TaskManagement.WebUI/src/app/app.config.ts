import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { AUTH_SERVICE } from './core/interfaces/auth.service.interface';
import { TASK_SERVICE } from './core/interfaces/task.service.interface';
import { AuthService } from './core/services/auth.service';
import { TaskService } from './core/services/task.service';
import { TokenStorageService } from './core/services/token-storage.service';
import { TOKEN_STORAGE } from './core/tokens/injection.tokens';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideAnimationsAsync(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    { provide: TOKEN_STORAGE, useExisting: TokenStorageService },
    { provide: AUTH_SERVICE, useExisting: AuthService },
    { provide: TASK_SERVICE, useExisting: TaskService }
  ]
};
