import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { JobsComponent } from './jobs/jobs.component';
import { WalletsComponent } from './wallets/wallets.component';
import { DealsComponent } from './deals/deals.component';
import { MetasymbolComponent } from './metasymbols/metasymbols.component';
import { RatesComponent } from './rates/rates.component';
import { SettingsComponent } from './settings/settings.component';
import { PersonComponent } from './person/person.component';

const routes: Routes = [
  {
    path: '',
    data: {
      title: 'Tables'
    },
    children: [
      {
        path: 'wallets',
        component: WalletsComponent,
        data: {
          title: 'Wallets'
        }
      },
      {
        path: 'metasymbols',
        component: MetasymbolComponent,
        data: {
          title: 'Metasymbols'
        }
      },
      {
        path: 'rates',
        component: RatesComponent,
        data: {
          title: 'Rates'
        }
      },
      {
        path: 'jobs',
        component: JobsComponent,
        data: {
          title: 'Jobs'
        }
      },
      {
        path: 'deals',
        component: DealsComponent,
        data: {
          title: 'Deals'
        }
      },
      {
        path: 'settings',
        component: SettingsComponent,
        data: {
          title: 'Settings'
        }
      },
      {
        path: 'person',
        component: PersonComponent,
        data: {
          title: 'Logins'
        }
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class TablesRoutingModule {}
