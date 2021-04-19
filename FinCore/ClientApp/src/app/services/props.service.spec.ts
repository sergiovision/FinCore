import { ComponentFixture, fakeAsync, flush, flushMicrotasks, TestBed, tick, inject, waitForAsync } from '@angular/core/testing';
import { DebugElement } from '@angular/core';
import { PropsService } from './props.service';
import { AuthenticationService } from './authentication.service';
import { HttpClientModule } from '@angular/common/http';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { EntitiesEnum, DynamicPropertyDefinition } from '../models/Props';
import { TablesModule } from '../views/tables/tables.module';
import { first } from 'rxjs/operators';
import { AssertionError } from 'assert';

describe('PropsService', function() {
  let fixture: ComponentFixture<PropsService>;
  let service: PropsService;
  let authService: AuthenticationService;
  let passed: boolean;
  beforeEach(() => {
    //const propsServiceSpy = jasmine.createSpyObj('PropsService', ['testFunc'])

    TestBed.configureTestingModule({
      imports: [HttpClientModule, HttpClientTestingModule, TablesModule],
      providers: [PropsService, AuthenticationService]
    });
  });

  it('PropsService should be created', inject([PropsService], (service: PropsService) => {
    expect(service).toBeTruthy();
    let res: DynamicPropertyDefinition<any>[] = service.getDefinitionsForEntity('MetaSymbol');
    expect(res.length).toEqual(3);
  }));

  it('PropsService test backend HTTP in dev mode', inject(
    [PropsService, AuthenticationService],
    (service: PropsService, authService: AuthenticationService) => {
      expect(authService).toBeTruthy();
      authService
        .login('admin@sergego.com', '654321X')
        .pipe(first())
        .subscribe(
          dat => {
            service.getInstance(4, 1).subscribe(
              data => {
                expect(data).toContain('Levels');
                passed = true;
              },
              error => {
                const message = JSON.stringify(error.error) + '\n' + error.statusText;
                expect(message).toBe('');
                passed = false;
              }
            );
          },
          error => {
            const message = JSON.stringify(error.error) + '\n' + error.statusText;
            expect(message).toBe('');
            passed = false;
          }
        );
      // expect(passed).toEqual(true);
    }
  ));
});
