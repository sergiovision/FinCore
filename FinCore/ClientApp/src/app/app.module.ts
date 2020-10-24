import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule } from '@angular/core';
import { LocationStrategy, HashLocationStrategy } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';
import { PERFECT_SCROLLBAR_CONFIG } from 'ngx-perfect-scrollbar';
import { PerfectScrollbarConfigInterface } from 'ngx-perfect-scrollbar';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { DevExtremeModule, DxBarGaugeModule, DxBoxModule } from 'devextreme-angular';
import { DatePipe } from '@angular/common';
import {
  DxSelectBoxModule,
  DxCheckBoxModule,
  DxTextBoxModule,
  DxDateBoxModule,
  DxButtonModule,
  DxValidatorModule,
  DxValidationSummaryModule,
  DxDataGridModule,
  DxTemplateModule
} from 'devextreme-angular';

const DEFAULT_PERFECT_SCROLLBAR_CONFIG: PerfectScrollbarConfigInterface = {
  suppressScrollX: true
};

import { AppComponent } from './app.component';

// Import containers
import { DefaultLayoutComponent } from './containers';

import { P404Component } from './views/error/404.component';
import { P500Component } from './views/error/500.component';
import { LoginComponent } from './views/login/login.component';
import { LogoutComponent } from './views/logout/logout.component';
import { RegisterComponent } from './views/register/register.component';

const APP_CONTAINERS = [DefaultLayoutComponent];

import { AppAsideModule, AppBreadcrumbModule, AppHeaderModule, AppFooterModule, AppSidebarModule } from '@coreui/angular';

// Import routing module
import { AppRoutingModule } from './app.routing';

// Import 3rd party components
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { TabsModule } from 'ngx-bootstrap/tabs';

import { AuthenticationService } from './services/authentication.service';
import { PersonService } from './services/person.service';
import { JwtInterceptor } from './helpers/JwtInterceptior';
import { ErrorInterceptor } from './helpers/ErrorInterceptor';
import { AuthGuard } from './guards/AuthGuard';
import { BaseComponent } from './base/base.component';
import { TviewComponent } from './views/tview/tview.component';
import { ChartComponent } from './views/chart/chart.component';
import { DashboardComponent } from './views/dashboard/dashboard.component';
import { DealsService } from './services/deals.service';
import { WebsocketService } from './services/websocket.service';
import { TablesModule } from './views/tables/tables.module';
import { PropsService } from './services/props.service';
import { DocComponent } from './views/doc/doc.component';
import { DGaugeComponent } from './views/dgauge/dgauge.component';
// import { PropertiesComponent } from './views/tables/properties/properties.component';

@NgModule({
  imports: [
    BrowserModule,
    FormsModule,
    ReactiveFormsModule,
    BrowserAnimationsModule,
    AppRoutingModule,
    AppAsideModule,
    AppBreadcrumbModule.forRoot(),
    AppFooterModule,
    AppHeaderModule,
    AppSidebarModule,
    PerfectScrollbarModule,
    BsDropdownModule.forRoot(),
    TabsModule.forRoot(),
    HttpClientModule,
    DevExtremeModule,
    DxSelectBoxModule,
    DxCheckBoxModule,
    DxTextBoxModule,
    DxDateBoxModule,
    DxButtonModule,
    DxValidatorModule,
    DxValidationSummaryModule,
    DxDataGridModule,
    DxTemplateModule,
    TablesModule,
    DxBarGaugeModule,
    DxBoxModule
  ],
  declarations: [
    BaseComponent,
    AppComponent,
    ...APP_CONTAINERS,
    P404Component,
    P500Component,
    LoginComponent,
    LogoutComponent,
    RegisterComponent,
    ChartComponent,
    TviewComponent,
    DocComponent,
    DGaugeComponent,
    DashboardComponent
  ],
  providers: [
    {
      provide: LocationStrategy,
      useClass: HashLocationStrategy
    },
    AuthenticationService,
    JwtInterceptor,
    AuthGuard,
    DatePipe,
    PersonService,
    DealsService,
    WebsocketService,
    PropsService,
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true }
  ],
  exports: [FormsModule, ReactiveFormsModule],
  bootstrap: [AppComponent]
})
export class AppModule {}
