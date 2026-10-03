import { Routes } from '@angular/router';
import { FileMaintenanceComponent } from './pages/file-maintenance/file-maintenance.component';
import { FileSearchComponent } from './pages/file-search/file-search.component';
import { InventoryComponent } from './pages/inventory/inventory.component';

export const routes: Routes = [
  { path: '', component: FileMaintenanceComponent },
  { path: 'search', component: FileSearchComponent },
  { path: 'inventory', component: InventoryComponent },
  { path: '**', redirectTo: '' }
];
