import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { finalize } from 'rxjs';

import { Produto } from '../../../../core/models/produto.model';
import { ProdutosService } from '../../services/produtos.service';

@Component({
  selector: 'app-produtos-list',
  templateUrl: './produtos-list.component.html',
  styleUrls: ['./produtos-list.component.scss'],
  standalone: false
})
export class ProdutosListComponent implements OnInit {
  produtos: Produto[] = [];
  loading = false;
  error = '';

  constructor(
    private readonly produtosService: ProdutosService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadProdutos();
  }

  loadProdutos(): void {
    this.loading = true;
    this.error = '';

    this.produtosService
      .getAll()
      .pipe(
        finalize(() => {
          this.loading = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: (response) => {
          this.produtos = Array.isArray(response) ? [...response] : [];
          console.log('Produtos carregados:', this.produtos);
          this.cdr.detectChanges();
        },
        error: (err: Error) => {
          this.error = err.message;
          this.cdr.detectChanges();
        }
      });
  }
}
