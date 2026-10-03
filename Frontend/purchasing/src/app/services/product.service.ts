import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  stock: number;
}

export interface ProductInput {
  name: string;
  description: string;
  price: number;
  stock: number;
}

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly api = inject(ApiService);

  list(): Observable<Product[]> {
    return this.api.get<Product[]>('products');
  }

  create(product: ProductInput): Observable<Product> {
    return this.api.post<ProductInput, Product>('products', product);
  }
}
