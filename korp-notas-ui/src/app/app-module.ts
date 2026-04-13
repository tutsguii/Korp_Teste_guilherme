import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { AppRoutingModule } from './app-routing-module';
import { AppComponent } from './app';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { NotaDetailComponent } from './features/notas/pages/nota-detail/nota-detail.component';
import { NotaFormComponent } from './features/notas/pages/nota-form/nota-form.component';
import { NotasListComponent } from './features/notas/pages/notas-list/notas-list.component';
import { ProdutoFormComponent } from './features/produtos/pages/produto-form/produto-form.component';
import { ProdutosListComponent } from './features/produtos/pages/produtos-list/produtos-list.component';
import { MaterialModule } from './shared/material/material.module';

@NgModule({
  declarations: [
    AppComponent,
    ProdutosListComponent,
    ProdutoFormComponent,
    NotasListComponent,
    NotaFormComponent,
    NotaDetailComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    ReactiveFormsModule,
    AppRoutingModule,
    MaterialModule
  ],
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(withInterceptors([errorInterceptor]))
  ],
  bootstrap: [AppComponent]
})
export class AppModule {}
