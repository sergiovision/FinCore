import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  DxCheckBoxModule,
  DxColorBoxModule,
  DxDateBoxModule,
  DxNumberBoxModule,
  DxSelectBoxModule,
  DxTextAreaModule,
  DxTextBoxModule
} from 'devextreme-angular';
import { PropertiesComponent } from './properties.component';

// Shared by the lazy-loaded TablesModule and the eagerly loaded dashboard/crypto views,
// so that AppModule doesn't need to import TablesModule (and its routes) eagerly.
@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    DxCheckBoxModule,
    DxColorBoxModule,
    DxDateBoxModule,
    DxNumberBoxModule,
    DxSelectBoxModule,
    DxTextAreaModule,
    DxTextBoxModule
  ],
  declarations: [PropertiesComponent],
  exports: [PropertiesComponent]
})
export class PropertiesModule { }
