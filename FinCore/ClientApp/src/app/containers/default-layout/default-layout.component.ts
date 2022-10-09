import { Component, Input, ViewChild, ElementRef, AfterViewInit, OnInit } from '@angular/core';
import { navItems } from './../../_nav';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  templateUrl: './default-layout.component.html'
})
export class DefaultLayoutComponent implements OnInit {
  public navItems = navItems;
  public sidebarMinimized = true;
  private changes: MutationObserver;
  public element: HTMLElement = document.body;

  @ViewChild('mainview') mainView: ElementRef;

  constructor(private route: ActivatedRoute ) {

    this.changes = new MutationObserver((mutations) => {
      this.sidebarMinimized = document.body.classList.contains('sidebar-minimized');
    });

    this.changes.observe(<Element>this.element, {
      attributes: true
    });

  }

  ngOnInit() {
    // console.log('height---' + this.element.offsetHeight);
  }



}
