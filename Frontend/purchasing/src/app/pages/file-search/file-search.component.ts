import { Component, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FileChunk, FileMaintenanceService } from '../../services/file-maintenance.service';

@Component({
  selector: 'app-file-search',
  standalone: true,
  imports: [DecimalPipe],
  templateUrl: './file-search.component.html',
  styleUrl: './file-search.component.css'
})
export class FileSearchComponent {
  private readonly fileService = inject(FileMaintenanceService);
  protected readonly query = signal('');
  protected readonly results = signal<FileChunk[]>([]);
  protected readonly searching = signal(false);
  protected readonly searched = signal(false);
  protected readonly error = signal('');

  protected updateQuery(event: Event): void {
    this.query.set((event.target as HTMLInputElement).value);
  }

  protected search(): void {
    const query = this.query().trim();
    if (!query) {
      this.error.set('Escribe una consulta para buscar en los archivos.');
      this.searched.set(false);
      return;
    }

    this.searching.set(true);
    this.searched.set(false);
    this.error.set('');
    this.fileService.search({ query, mode: 'Hybrid', top: 3 }).subscribe({
      next: results => {
        this.results.set(results);
        this.searching.set(false);
        this.searched.set(true);
      },
      error: () => {
        this.searching.set(false);
        this.error.set('No fue posible completar la búsqueda. Verifica que el API esté disponible.');
      }
    });
  }

  protected score(score: number): string {
    return score.toFixed(2);
  }

  protected sourceName(fileName: string): string {
    return fileName.replace(/^\d+_/, '');
  }
}
