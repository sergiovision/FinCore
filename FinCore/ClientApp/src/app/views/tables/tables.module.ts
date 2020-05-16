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
import { AuthenticationService } from '../../services/authentication.service';
import { PropertiesComponent } from './properties/properties.component';
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
    DxPieChartModule
  ],
  declarations: [
    JobsComponent,
    WalletsComponent,
    DealsComponent,
    PropertiesComponent,
    MetasymbolComponent,
    RatesComponent,
    SettingsComponent,
    PersonComponent
  ],
  providers: [
    WalletsService,
    DealsService,
    ExpertsService,
    PropsService,
    AuthenticationService
  ],
  exports: [PropertiesComponent]
})
export class TablesModule { }
