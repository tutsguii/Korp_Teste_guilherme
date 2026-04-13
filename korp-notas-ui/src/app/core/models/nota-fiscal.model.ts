import { ItemNotaFiscal } from './item-nota-fiscal.model';

export interface NotaFiscal {
  id: string;
  numero: number;
  status: number;
  dataCriacao: string;
  correlationId?: string;
  mensagemFalha?: string;
  itens: ItemNotaFiscal[];
}
