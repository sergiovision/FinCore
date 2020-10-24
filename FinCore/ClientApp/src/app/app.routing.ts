import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

// Import Containers
import { DefaultLayoutComponent } from './containers';

import { P404Component } from './views/error/404.component';
import { P500Component } from './views/error/500.component';
import { LoginComponent } from './views/login/login.component';
import { LogoutComponent } from './views/logout/logout.component';
import { RegisterComponent } from './views/register/register.component';
import { AuthGuard } from './guards/AuthGuard';
import { DashboardComponent } from './views/dashboard/dashboard.component';
import { DocComponent } from './views/doc/doc.component';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    canActivate: [AuthGuard],
    pathMatch: 'full'
  },
  {
    path: '404',
    component: P404Component,
    data: {
      title: 'Page 404'
    }
  },
  {
    path: '500',
    component: P500Component,
    data: {
      title: 'Page 500'
    }
  },
  {
    path: 'login',
    component: LoginComponent,
    data: {
      title: 'Login Page'
    }
  },
  {
    path: 'logout',
    component: LogoutComponent,
    data: {
      title: 'Logout Page'
    }
  },
  {
    path: 'register',
    component: RegisterComponent,
    data: {
      title: 'Register Page'
    }
  },
  {
    path: '',
    component: DefaultLayoutComponent,
    data: {
      title: 'Home'
    },
    children: [
      {
        path: 'dashboard',
        canActivate: [AuthGuard],
        component: DashboardComponent
      },
      {
        path: 'logs',
        canActivate: [AuthGuard],
        loadChildren: () => import('./views/logs/logs.module').then(m => m.LogsModule)
      },
      {
        path: 'stat',
        canActivate: [AuthGuard],
        loadChildren: () => import('./views/stat/stat.module').then(m => m.StatModule)
      },
      {
        path: 'tables',
        canActivate: [AuthGuard],
        loadChildren: () => import('./views/tables/tables.module').then(m => m.TablesModule)
      },
      {
        path: 'doc',
        canActivate: [AuthGuard],
        component: DocComponent
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
