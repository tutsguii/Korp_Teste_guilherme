import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';

import { NotaFiscal } from '../../../../core/models/nota-fiscal.model';
import { NotasService } from '../../services/notas.service';

@Component({
  selector: 'app-notas-list',
  templateUrl: './notas-list.component.html',
  styleUrls: ['./notas-list.component.scss'],
  standalone: false
})
export class NotasListComponent implements OnInit {
  notas: NotaFiscal[] = [];
  loading = false;
  error = '';

  constructor(
    private readonly notasService: NotasService,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadNotas();
  }

  loadNotas(): void {
    this.loading = true;
    this.error = '';

    this.notasService
      .getAll()
      .pipe(
        finalize(() => {
          this.loading = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: (data) => {
          this.notas = Array.isArray(data) ? [...data] : [];
          console.log('Notas carregadas:', this.notas);
          this.cdr.detectChanges();
        },
        error: (err: Error) => {
          this.error = err.message;
          this.cdr.detectChanges();
        }
      });
  }

  abrirDetalhe(id: string): void {
    this.router.navigate(['/notas', id]);
  }

  getStatusLabel(status: number): string {
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
        return 'Desconhecido';
    }
  }
}
