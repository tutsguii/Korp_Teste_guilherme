import { ChangeDetectorRef, Component } from '@angular/core';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';

import { NotasService } from '../../services/notas.service';

@Component({
  selector: 'app-nota-form',
  templateUrl: './nota-form.component.html',
  styleUrls: ['./nota-form.component.scss'],
  standalone: false
})
export class NotaFormComponent {
  loading = false;
  error = '';

  constructor(
    private readonly notasService: NotasService,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef
  ) {}

  salvar(): void {
    this.loading = true;
    this.error = '';

    this.notasService
      .create()
      .pipe(
        finalize(() => {
          this.loading = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: () => {
          this.router.navigate(['/notas']);
        },
        error: (err: Error) => {
          this.error = err.message;
          this.cdr.detectChanges();
        }
      });
  }
}
