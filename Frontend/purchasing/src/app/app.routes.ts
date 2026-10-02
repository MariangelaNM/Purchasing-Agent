import { Routes } from '@angular/router';
import { FileMaintenanceComponent } from './pages/file-maintenance/file-maintenance.component';
import { FileSearchComponent } from './pages/file-search/file-search.component';

export const routes: Routes = [
  { path: '', component: FileMaintenanceComponent },
  { path: 'search', component: FileSearchComponent },
  { path: '**', redirectTo: '' }
];
