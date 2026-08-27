import { InjectionToken } from '@angular/core';
import { ITokenStorage } from '../interfaces/token-storage.interface';

export const TOKEN_STORAGE = new InjectionToken<ITokenStorage>('TOKEN_STORAGE');
