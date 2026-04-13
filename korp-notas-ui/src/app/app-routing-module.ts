import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { NotaDetailComponent } from './features/notas/pages/nota-detail/nota-detail.component';
import { NotaFormComponent } from './features/notas/pages/nota-form/nota-form.component';
import { NotasListComponent } from './features/notas/pages/notas-list/notas-list.component';
import { ProdutoFormComponent } from './features/produtos/pages/produto-form/produto-form.component';
import { ProdutosListComponent } from './features/produtos/pages/produtos-list/produtos-list.component';

const routes: Routes = [
  { path: '', redirectTo: 'produtos', pathMatch: 'full' },
  { path: 'produtos', component: ProdutosListComponent },
  { path: 'produtos/novo', component: ProdutoFormComponent },
  { path: 'notas', component: NotasListComponent },
  { path: 'notas/nova', component: NotaFormComponent },
  { path: 'notas/:id', component: NotaDetailComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
