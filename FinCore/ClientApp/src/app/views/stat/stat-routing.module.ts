import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { CapitalComponent } from './capital/capital.component';
import { InvestmentsComponent } from './investments/investments.component';
import { PerformanceComponent } from './performance/performance.component';
import { SymbolsComponent } from './symbols/symbols.component';

const routes: Routes = [
  {
    path: '',
    data: {
      title: 'Statistics'
    },
    children: [
      {
        path: 'symbols',
        component: SymbolsComponent,
        data: {
          title: 'Instruments'
        }
      },

      {
        path: 'performance',
        component: PerformanceComponent,
        data: {
          title: 'Performance'
        }
      },
      {
        path: 'capital',
        component: CapitalComponent,
        data: {
          title: 'Capital'
        }
      },
      {
        path: 'investments',
        component: InvestmentsComponent,
        data: {
          title: 'Investments'
        }
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class StatRoutingModule {}
