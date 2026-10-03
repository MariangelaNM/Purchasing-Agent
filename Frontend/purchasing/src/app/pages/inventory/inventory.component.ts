import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Product, ProductInput, ProductService } from '../../services/product.service';

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './inventory.component.html',
  styleUrl: './inventory.component.css'
})
export class InventoryComponent {
  private readonly productService = inject(ProductService);
  protected readonly products = signal<Product[]>([]);
  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected readonly message = signal('');
  protected readonly error = signal('');
  protected readonly search = signal('');
  protected readonly form = signal<ProductInput>({ name: '', description: '', price: 0, stock: 0 });
  protected readonly filteredProducts = computed(() => {
    const query = this.search().trim().toLowerCase();
    return query ? this.products().filter(product => product.name.toLowerCase().includes(query)) : this.products();
  });

  constructor() { this.loadProducts(); }

  protected updateField(field: keyof ProductInput, value: string | number): void {
    this.form.update(current => ({ ...current, [field]: value }));
  }

  protected loadProducts(): void {
    this.loading.set(true);
    this.productService.list().subscribe({
      next: products => { this.products.set(products); this.loading.set(false); },
      error: () => { this.loading.set(false); this.error.set('No fue posible consultar el inventario.'); }
    });
  }

  protected saveProduct(): void {
    const product = this.form();
    if (!product.name.trim() || product.price <= 0 || product.stock < 0) {
      this.error.set('Completa el nombre, un precio mayor que cero y un stock válido.');
      return;
    }
    this.saving.set(true);
    this.error.set('');
    this.productService.create({ ...product, name: product.name.trim() }).subscribe({
      next: created => {
        this.products.update(products => [...products, created]);
        this.form.set({ name: '', description: '', price: 0, stock: 0 });
        this.saving.set(false);
        this.message.set('Producto agregado al inventario.');
      },
      error: () => { this.saving.set(false); this.error.set('No fue posible guardar el producto.'); }
    });
  }

  protected updateSearch(value: string): void { this.search.set(value); }
}
