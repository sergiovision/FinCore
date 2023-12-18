import { AfterViewInit, Component, ViewChild, OnInit, OnDestroy, ViewChildren, QueryList } from '@angular/core';
import { LogsService } from '../../services/logs.service';
import notify from 'devextreme/ui/notify';
import { BaseComponent } from '../../base/base.component';
import { WsMessage, WsMessageType, IWebsocketCallback, LogItem } from '../../models/Entities';
import { WebsocketService } from '../../services/websocket.service';
import { EditorComponent, NgxEditorModel } from 'ngx-monaco-editor';
import { fromEvent } from "rxjs";
import { debounceTime, throttleTime } from "rxjs/operators";
import { ElementRef, Renderer2 } from "@angular/core";


import { DxBoxComponent, DxTabPanelComponent } from 'devextreme-angular';


@Component({
  templateUrl: 'logs.component.html',
  styleUrls: ['logs.component.scss'],
  providers: [LogsService, WebsocketService]
})
export class LogsComponent extends BaseComponent implements AfterViewInit, OnInit, OnDestroy, IWebsocketCallback {
  @ViewChild('tabPanel0') tabPanel0: DxTabPanelComponent;
  @ViewChild('boxitem') boxitem: DxBoxComponent;
  @ViewChild('targetElem') targetElement: ElementRef;
  public element: HTMLElement = document.body;
  //public editorHeight:number = 600;
  public editor: any;

  code: string = '';

  // https://microsoft.github.io/monaco-editor/playground.html#creating-the-editor-syntax-highlighting-for-html-elements
  ///look file for options monako-editor/esm/vs/editor/editor.api.d.ts
  options: any = {
    theme: 'vs-dark', language: 'javascript',
    lineNumbers: 'off',
	  readOnly: true,
    verticalHasArrows: true,
		horizontalHasArrows: true,
    contextmenu: false

  };


  public NumberOfLines = 500;
  public selectedIndex  = 0;

  public logz: LogItem[];

  constructor(public logS: LogsService, public ws: WebsocketService, private renderer: Renderer2) {
    super();

    fromEvent(window, "resize")
      .pipe(throttleTime(200), debounceTime(200))
      .subscribe(() => this.setHeight());
  }

  public doConnect() {

    this.ws.doConnect(this);

  }

  ngAfterViewInit() {
    // Target element's parent's height
    const parentHeight = this.targetElement.nativeElement.parentElement.offsetHeight;
    // Applying height to target element
    this.renderer.setStyle(this.targetElement.nativeElement, 'height', `${parentHeight}px`);
  }

  private setHeight() {
    //if (this.element.offsetHeight > 0) {
      //this.editorHeight = this.element.offsetHeight - 200;
      //console.log('setHeight---' + this.editorHeight);
    //}
  }

  override ngOnInit() {
    super.ngOnInit();

    this.subs.sink = this.logS.GetLogList()
    .subscribe(data  => {
        this.logz = data;
        this.doConnect();
      },
        error => this.logConsoleError(error)
      );

      this.setHeight();

  }

  public getText(): string {
    return this.logz[0].DataSource;
  }

  public ClearLogs() {
    this.logz[0].DataSource = '';
    this.ws.doSend({ Type: WsMessageType.ClearLog, From: this.logS.currentUserToken.userName, Message: '' });
    if (this.editor) {
      var model = this.editor.getModel();
      if (model)
         model.setValue(this.getText());
    }

  }

  public RefreshLogs() {
    window.location.reload();
  }

  override ngOnDestroy(): void {
    this.ws.doDisconnect();
    super.ngOnDestroy();
  }

  public writeToScreen(message) {
    console.log(message);
  }

  public onOpen(evt: MessageEvent) {
    this.ws.doSend({ Type: WsMessageType.GetAllText, From: this.logS.currentUserToken.userName, Message: '' });

    this.writeToScreen('connected logs\n');
  }

  onInit(editor) {
    this.editor = editor;
    console.log('OnInit Monaco Editor' + editor);

  }


  public selectTab(e) {
    const logitem = this.logz[this.selectedIndex];
    console.log('Click MainLog tab index: ' + this.selectedIndex + ', name: ' + logitem.Name);

    this.subs.sink = this.logS.GetLogContent(logitem.Name, this.NumberOfLines)
    .subscribe(data  => {
        if (this.selectedIndex > 0) {
          this.logz[this.selectedIndex].DataSource = data;
        }
        //const editorX = this.viewChildren.find((element, index) => index === this.selectedIndex);
        //this.code = data;
        if (this.selectedIndex > 0) {
          // console.log(' data = ' + data );
          //this.code = data;
          //var opt = this.options;
          //opt.theme = this.logz[this.selectedIndex].Theme;
          //this.options = opt;
          //this.options = Object.assign({}, this.options, opt);
          // console.log(' opt = ' + JSON.stringify(opt) );
        } else {
          data = this.getText();
          // console.log(data);
          this.code = data;
        }
        if (this.editor) {
           var model = this.editor.getModel();
           if (model) {
              model.setValue(data);

              var lineCount = model.getLineCount();
              console.log(' OnInit countlines: ' + lineCount);
              this.editor.revealLine(lineCount, 1);

           }
        }
      },
      error => this.logConsoleError(error)
    );
  }

  public onClose() {
    this.writeToScreen('disconnected logs\n');
  }

  public onMessage(msg: WsMessage) {
    if (msg) {
       switch (msg.Type) {
        case WsMessageType.WriteLog: {
          let str = msg.Message;
          if (str.charAt(0) === '"' && str.charAt(str.length - 1) === '"') {
            str  = str.substr(1, str.length - 2);
          }
          str += '\r';
          str = this.logz[0].DataSource + str;
          this.logz[0].DataSource = str;
          }
          break;
        case WsMessageType.GetAllText:
          this.logz[0].DataSource = msg.Message;
          this.tabPanel0.selectedItem = 0;
          this.selectTab(undefined);
          break;
       }
    }
  }

  public onError(evt: MessageEvent) {
    const webS = evt.currentTarget as WebSocket;
    if (webS)
      notify('WebSocket Connection Error: ' + webS.url);
    else
      notify('WebSocket Connection Error: ' + evt.currentTarget);
    this.ws.doDisconnect();
  }

}
