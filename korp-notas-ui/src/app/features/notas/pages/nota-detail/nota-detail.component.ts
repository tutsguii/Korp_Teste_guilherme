import { ChangeDetectorRef, Component, inject, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Subject, finalize, interval, switchMap, takeUntil } from 'rxjs';

import { ItemNotaFiscal } from '../../../../core/models/item-nota-fiscal.model';
import { NotaFiscal } from '../../../../core/models/nota-fiscal.model';
import { Produto } from '../../../../core/models/produto.model';
import { ProdutosService } from '../../../produtos/services/produtos.service';
import { NotasService } from '../../services/notas.service';

@Component({
  selector: 'app-nota-detail',
  templateUrl: './nota-detail.component.html',
  styleUrls: ['./nota-detail.component.scss'],
  standalone: false
})
export class NotaDetailComponent implements OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly destroy$ = new Subject<void>();
  private readonly pollingStop$ = new Subject<void>();

  notaId = '';
  nota?: NotaFiscal;
  produtos: Produto[] = [];
  loading = false;
  loadingFechamento = false;
  error = '';
  successMessage = '';
  editingItemId: string | null = null;

  formItem = this.fb.group({
    produtoId: ['', Validators.required],
    quantidade: [1, [Validators.required, Validators.min(1)]]
  });

  constructor(
    private readonly route: ActivatedRoute,
    private readonly notasService: NotasService,
    private readonly produtosService: ProdutosService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.notaId = this.route.snapshot.paramMap.get('id') || '';
    this.loadProdutos();
    this.loadNota();
  }

  loadNota(): void {
    this.loading = true;
    this.error = '';

    this.notasService
      .getById(this.notaId)
      .pipe(
        finalize(() => {
          this.loading = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: (data) => {
          this.nota = data;
          this.cdr.detectChanges();
        },
        error: (err: Error) => {
          this.error = err.message;
          this.cdr.detectChanges();
        }
      });
  }

  loadProdutos(): void {
    this.produtosService.getAll().subscribe({
      next: (data) => {
        this.produtos = Array.isArray(data) ? [...data] : [];
        this.cdr.detectChanges();
      },
      error: (err: Error) => {
        this.error = err.message;
        this.cdr.detectChanges();
      }
    });
  }

  salvarItem(): void {
    if (this.formItem.invalid) {
      this.formItem.markAllAsTouched();
      return;
    }

    this.error = '';
    this.successMessage = '';

    const payload = this.formItem.getRawValue() as { produtoId: string; quantidade: number };
    const request$ = this.editingItemId
      ? this.notasService.updateItem(this.notaId, this.editingItemId, payload)
      : this.notasService.addItem(this.notaId, payload);

    request$.subscribe({
      next: () => {
        this.successMessage = this.editingItemId
          ? 'Item atualizado com sucesso.'
          : 'Item adicionado com sucesso.';
        this.cancelarEdicao();
        this.loadNota();
      },
      error: (err: Error) => {
        this.error = err.message;
        this.cdr.detectChanges();
      }
    });
  }

  editarItem(item: ItemNotaFiscal): void {
    this.editingItemId = item.id;
    this.formItem.patchValue({
      produtoId: item.produtoId,
      quantidade: item.quantidade
    });
  }

  removerItem(item: ItemNotaFiscal): void {
    this.error = '';
    this.successMessage = '';

    this.notasService.deleteItem(this.notaId, item.id).subscribe({
      next: () => {
        this.successMessage = 'Item removido com sucesso.';
        if (this.editingItemId === item.id) {
          this.cancelarEdicao();
        }
        this.loadNota();
      },
      error: (err: Error) => {
        this.error = err.message;
        this.cdr.detectChanges();
      }
    });
  }

  cancelarEdicao(): void {
    this.editingItemId = null;
    this.formItem.reset({
      produtoId: '',
      quantidade: 1
    });
    this.cdr.detectChanges();
  }

  fecharNota(): void {
    this.loadingFechamento = true;
    this.error = '';
    this.successMessage = '';
    this.pollingStop$.next();

    this.notasService.fecharNota(this.notaId).subscribe({
      next: () => {
        this.successMessage = 'Solicitacao de fechamento enviada.';
        this.startPollingStatus();
      },
      error: (err: Error) => {
        this.error = err.message;
        this.loadingFechamento = false;
        this.cdr.detectChanges();
      }
    });
  }

  startPollingStatus(): void {
    interval(2000)
      .pipe(
        takeUntil(this.pollingStop$),
        takeUntil(this.destroy$),
        switchMap(() => this.notasService.getById(this.notaId))
      )
      .subscribe({
        next: (nota) => {
          this.nota = nota;

          if (nota.status !== 2) {
            this.loadingFechamento = false;
            this.successMessage =
              nota.status === 3
                ? 'Nota fechada com sucesso.'
                : nota.mensagemFalha || 'Processamento finalizado.';
            this.pollingStop$.next();
          }

          this.cdr.detectChanges();
        },
        error: () => {
          this.loadingFechamento = false;
          this.pollingStop$.next();
          this.cdr.detectChanges();
        }
      });
  }

  getStatusLabel(status?: number): string {
    switch (status) {
      case 1:
        return 'Aberta';
      case 2:
        return 'Processando';
      case 3:
        return 'Fechada';
      case 4:
        return 'Erro';
      default:
        return '-';
    }
  }

  getProdutoDescricao(produtoId: string): string {
    const produto = this.produtos.find((x) => x.id === produtoId);
    return produto ? `${produto.codigo} - ${produto.descricao}` : produtoId;
  }

  ngOnDestroy(): void {
    this.pollingStop$.next();
    this.pollingStop$.complete();
    this.destroy$.next();
    this.destroy$.complete();
  }
}
