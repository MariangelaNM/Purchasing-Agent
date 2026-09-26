import { Component, computed, inject, signal } from '@angular/core';
import { FileMaintenanceService, IndexedFile } from '../../services/file-maintenance.service';

@Component({
  selector: 'app-file-maintenance',
  standalone: true,
  templateUrl: './file-maintenance.component.html',
  styleUrl: './file-maintenance.component.css'
})
export class FileMaintenanceComponent {
  private readonly fileService = inject(FileMaintenanceService);
  protected readonly files = signal<IndexedFile[]>([]);
  protected readonly selectedFiles = signal<File[]>([]);
  protected readonly searchTerm = signal('');
  protected readonly loading = signal(false);
  protected readonly uploading = signal(false);
  protected readonly message = signal('');
  protected readonly error = signal('');
  protected readonly filteredFiles = computed(() => {
    const query = this.searchTerm().trim().toLowerCase();
    return query ? this.files().filter(file => file.sourceFile.toLowerCase().includes(query)) : this.files();
  });

  constructor() {
    this.loadFiles();
  }

  protected onFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFiles.set(Array.from(input.files ?? []));
    this.message.set('');
    this.error.set('');
  }

  protected uploadFiles(): void {
    const files = this.selectedFiles();
    if (!files.length) {
      this.error.set('Selecciona al menos un archivo para continuar.');
      return;
    }
    this.uploading.set(true);
    this.message.set('Procesando archivos e indexando su contenido...');
    this.error.set('');
    this.fileService.upload(files).subscribe({
      next: result => {
        this.uploading.set(false);
        this.selectedFiles.set([]);
        this.message.set(`${result.documentsProcessed} archivo(s) cargado(s) correctamente.`);
        this.loadFiles();
      },
      error: () => {
        this.uploading.set(false);
        this.message.set('');
        this.error.set('No fue posible cargar los archivos. Verifica que el API esté disponible.');
      }
    });
  }

  protected loadFiles(): void {
    this.loading.set(true);
    this.fileService.listFiles().subscribe({
      next: files => { this.files.set(files); this.loading.set(false); },
      error: () => { this.loading.set(false); this.error.set('No fue posible consultar los archivos cargados.'); }
    });
  }

  protected updateSearch(event: Event): void {
    this.searchTerm.set((event.target as HTMLInputElement).value);
  }

  protected clearSelection(): void {
    this.selectedFiles.set([]);
  }

  protected formatFileName(fileName: string): string {
    return fileName.replace(/^\d+_/, '');
  }
}
