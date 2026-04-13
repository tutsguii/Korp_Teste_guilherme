import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';

import { ProdutosService } from '../../services/produtos.service';

@Component({
  selector: 'app-produto-form',
  templateUrl: './produto-form.component.html',
  styleUrls: ['./produto-form.component.scss'],
  standalone: false
})
export class ProdutoFormComponent {
  private readonly fb = inject(FormBuilder);

  loading = false;
  error = '';

  form = this.fb.group({
    codigo: ['', [Validators.required, Validators.maxLength(50)]],
    descricao: ['', [Validators.required, Validators.maxLength(200)]],
    saldo: [0, [Validators.required, Validators.min(0)]]
  });

  constructor(
    private readonly produtosService: ProdutosService,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef
  ) {}

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.error = '';

    this.produtosService
      .create(this.form.getRawValue() as { codigo: string; descricao: string; saldo: number })
      .pipe(
        finalize(() => {
          this.loading = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: () => {
          this.router.navigate(['/produtos']);
        },
        error: (err: Error) => {
          this.error = err.message;
          this.cdr.detectChanges();
        }
      });
  }
}
