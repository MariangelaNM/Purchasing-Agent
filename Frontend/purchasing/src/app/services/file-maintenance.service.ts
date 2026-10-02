import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface IngestionResult {
  documentsProcessed: number;
  chunksIndexed: number;
}

export interface IndexedFile {
  sourceFile: string;
  chunksIndexed: number;
}

export interface FileChunk {
  id: string;
  sourceFile: string;
  ordinal: number;
  content: string;
  score: number;
}

export interface FileSearchRequest {
  query: string;
  mode?: string;
  top?: number;
}

export interface FileQuestionRequest {
  question: string;
  mode?: string;
  top?: number;
}

export interface FileAnswer {
  answer: string;
  sources: FileChunk[];
}

@Injectable({ providedIn: 'root' })
export class FileMaintenanceService {
  private readonly api = inject(ApiService);

  upload(files: File[], chunkSize = 1000, overlap = true): Observable<IngestionResult> {
    const formData = new FormData();
    files.forEach(file => formData.append('files', file, file.name));
    formData.append('chunkSize', String(chunkSize));
    formData.append('overlap', String(overlap));

    return this.api.post<FormData, IngestionResult>('documents/ingest-files', formData);
  }

  listFiles(): Observable<IndexedFile[]> {
    return this.api.get<IndexedFile[]>('documents');
  }

  getFileInfo(fileName: string): Observable<IndexedFile> {
    return this.api.get<IndexedFile>(`documents/${encodeURIComponent(fileName)}/info`);
  }

  listChunks(): Observable<FileChunk[]> {
    return this.api.get<FileChunk[]>('documents/chunks');
  }

  search(request: FileSearchRequest): Observable<FileChunk[]> {
    return this.api.post<FileSearchRequest, FileChunk[]>('documents/search', request);
  }

  ask(request: FileQuestionRequest): Observable<FileAnswer> {
    return this.api.post<FileQuestionRequest, FileAnswer>('documents/ask', request);
  }
}
