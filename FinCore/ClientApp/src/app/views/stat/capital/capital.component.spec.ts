import {async, ComponentFixture, fakeAsync, flush, flushMicrotasks, TestBed, tick, inject} from '@angular/core/testing';
import {DebugElement} from '@angular/core';
import { CapitalComponent } from './capital.component';
import { WalletsService } from '../../../services/wallets.service';
import { HttpClientModule } from '@angular/common/http';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { DxLoadPanelModule, DxChartModule, DxSelectBoxModule } from 'devextreme-angular';


describe('Capital Component', function() {

  let fixture: ComponentFixture<CapitalComponent>;
  let component:CapitalComponent;
  let el: DebugElement;
  let walletsService: any;


  beforeEach(() => {

    const walletsServiceSpy = jasmine.createSpyObj('WalletsService', ['testFunc'])

    TestBed.configureTestingModule({
      imports: [
          RouterTestingModule,
          HttpClientModule,
          HttpClientTestingModule,
          DxChartModule,
          DxSelectBoxModule,
          DxLoadPanelModule
      ],

      providers: [
          {provide: WalletsService, useValue: walletsServiceSpy}
      ],
      declarations: [CapitalComponent]
    });
    fixture = TestBed.createComponent(CapitalComponent);
    component = fixture.componentInstance;
    el = fixture.debugElement;
    walletsService = TestBed.get(WalletsService);
  });

  it('should check component', () => {
    expect(component).toBeTruthy();
    expect(walletsService).toBeTruthy();

  });
});
