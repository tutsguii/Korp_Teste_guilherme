import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { ApiSuccessResponse } from '../../../core/models/api-response.model';
import { Produto } from '../../../core/models/produto.model';

@Injectable({
  providedIn: 'root'
})
export class ProdutosService {
  private readonly baseUrl = `${environment.estoqueApi}/produtos`;

  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<Produto[]> {
    return this.http.get<Produto[] | ApiSuccessResponse<Produto[]>>(this.baseUrl).pipe(
      map((response: Produto[] | ApiSuccessResponse<Produto[]>) =>
        Array.isArray(response) ? response : response.data
      )
    );
  }

  getById(id: string): Observable<Produto> {
    return this.http
      .get<Produto | ApiSuccessResponse<Produto>>(`${this.baseUrl}/${id}`)
      .pipe(
        map((response: Produto | ApiSuccessResponse<Produto>) =>
          'data' in response ? response.data : response
        )
      );
  }

  create(payload: { codigo: string; descricao: string; saldo: number }): Observable<unknown> {
    return this.http.post(this.baseUrl, payload);
  }
}
