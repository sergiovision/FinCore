import { PerformanceComponent } from './performance/performance.component';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DxChartModule, DxSelectBoxModule, DxLoadPanelModule, DxPieChartComponent, DxPieChartModule } from 'devextreme-angular';

// Routing
import { StatRoutingModule } from './stat-routing.module';
import { CapitalComponent } from './capital/capital.component';
import { WalletsService} from '../../services/wallets.service';
import { DealsService} from '../../services/deals.service';
import { SymbolsComponent } from './symbols/symbols.component';
import { WebsocketService } from '../../services/websocket.service';
import { InvestmentsComponent } from './investments/investments.component';

@NgModule({
  imports: [
    StatRoutingModule,
    DxChartModule,
    DxSelectBoxModule,
    DxLoadPanelModule,
    DxPieChartModule
  ],
  providers: [
    WalletsService,
    DealsService,
    WebsocketService
  ],
  declarations: [
      CapitalComponent,
      SymbolsComponent,
      PerformanceComponent,
      InvestmentsComponent
  ]
})
export class StatModule { }
