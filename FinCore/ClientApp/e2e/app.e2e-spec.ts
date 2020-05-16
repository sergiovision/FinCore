import { CoreUIPage } from './app.po';
import {async, ComponentFixture, fakeAsync, flush, flushMicrotasks, TestBed, tick} from '@angular/core/testing';
import {DebugElement} from '@angular/core';
import 'jasmine';

describe('core-ui App', function() {
  let page: CoreUIPage;

  beforeEach(() => {
    page = new CoreUIPage();
  });

  it('should display footer containing XTrade Web Admin', () => {
    page.navigateTo();
    expect(page.getTitle()).toContain("XTrade Web Admin");
  });
});
