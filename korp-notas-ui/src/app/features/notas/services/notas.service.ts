import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { ApiSuccessResponse } from '../../../core/models/api-response.model';
import { NotaFiscal } from '../../../core/models/nota-fiscal.model';

@Injectable({
  providedIn: 'root'
})
export class NotasService {
  private readonly baseUrl = `${environment.faturamentoApi}/notas`;

  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<NotaFiscal[]> {
    return this.http.get<NotaFiscal[] | ApiSuccessResponse<NotaFiscal[]>>(this.baseUrl).pipe(
      map((response: NotaFiscal[] | ApiSuccessResponse<NotaFiscal[]>) =>
        Array.isArray(response) ? response : response.data
      )
    );
  }

  getById(id: string): Observable<NotaFiscal> {
    return this.http
      .get<NotaFiscal | ApiSuccessResponse<NotaFiscal>>(`${this.baseUrl}/${id}`)
      .pipe(
        map((response: NotaFiscal | ApiSuccessResponse<NotaFiscal>) =>
          'data' in response ? response.data : response
        )
      );
  }

  create(): Observable<unknown> {
    return this.http.post(this.baseUrl, {});
  }

  addItem(id: string, payload: { produtoId: string; quantidade: number }): Observable<unknown> {
    return this.http.post(`${this.baseUrl}/${id}/itens`, payload);
  }

  updateItem(
    id: string,
    itemId: string,
    payload: { produtoId: string; quantidade: number }
  ): Observable<unknown> {
    return this.http.put(`${this.baseUrl}/${id}/itens/${itemId}`, payload);
  }

  deleteItem(id: string, itemId: string): Observable<unknown> {
    return this.http.delete(`${this.baseUrl}/${id}/itens/${itemId}`);
  }

  fecharNota(id: string): Observable<unknown> {
    return this.http.post(`${this.baseUrl}/${id}/fechamento`, {});
  }
}
