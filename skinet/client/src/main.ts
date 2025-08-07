import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
// Update the import to match the correct exported member from app.component
import { AppComponent } from './app/app.component';
bootstrapApplication(AppComponent, appConfig)
  .catch((err) => console.error(err));
 
