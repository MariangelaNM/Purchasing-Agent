import { Routes } from '@angular/router';
import { FileMaintenanceComponent } from './pages/file-maintenance/file-maintenance.component';

export const routes: Routes = [
  { path: '', component: FileMaintenanceComponent },
  { path: '**', redirectTo: '' }
];
