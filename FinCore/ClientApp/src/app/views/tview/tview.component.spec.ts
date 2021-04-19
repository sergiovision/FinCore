import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { TviewComponent } from './tview.component';

describe('TviewComponent', () => {
  let component: TviewComponent;
  let fixture: ComponentFixture<TviewComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ TviewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
