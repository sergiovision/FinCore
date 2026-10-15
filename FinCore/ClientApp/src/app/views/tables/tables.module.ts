import { ExpertsService } from '../../services/experts.service';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DevExtremeModule,
  DxTextBoxModule,
  DxDateBoxModule,
  DxButtonModule,
  DxValidatorModule,
  DxValidationSummaryModule,
  DxDataGridModule,
  DxTemplateModule,
  DxPopupModule,
  DxNumberBoxModule,
  DxTextAreaModule,
  DxCheckBoxModule,
  DxSelectBoxModule,
  DxColorBoxModule,
  DxPieChartModule} from 'devextreme-angular';

// Routing
import { TablesRoutingModule } from './tables-routing.module';
import { JobsComponent } from './jobs/jobs.component';
import { WalletsService} from '../../services/wallets.service';
import { DealsService} from '../../services/deals.service';
import { WalletsComponent } from './wallets/wallets.component';
import { DealsComponent } from './deals/deals.component';
import { PropsService } from '../../services/props.service';
import { PropertiesModule } from './properties/properties.module';
import { DxFormModule } from 'devextreme-angular';
import { MetasymbolComponent } from './metasymbols/metasymbols.component';
import { RatesComponent } from './rates/rates.component';
import { SettingsComponent } from './settings/settings.component';
import { PersonComponent } from './person/person.component';

@NgModule({
  imports: [
    CommonModule,
    DxNumberBoxModule,
    DevExtremeModule,
    TablesRoutingModule,
    DxSelectBoxModule,
    DxCheckBoxModule,
    DxTextBoxModule,
    DxDateBoxModule,
    DxButtonModule,
    DxValidatorModule,
    DxValidationSummaryModule,
    DxDataGridModule,
    DxTemplateModule,
    DxPopupModule,
    DxTextAreaModule,
    FormsModule,
    DxColorBoxModule,
    DxFormModule,
    DxPieChartModule,
    PropertiesModule
  ],
  declarations: [
    JobsComponent,
    WalletsComponent,
    DealsComponent,
    MetasymbolComponent,
    RatesComponent,
    SettingsComponent,
    PersonComponent
  ],
  providers: [
    WalletsService,
    DealsService,
    ExpertsService,
    PropsService
  ]
})
export class TablesModule { }
