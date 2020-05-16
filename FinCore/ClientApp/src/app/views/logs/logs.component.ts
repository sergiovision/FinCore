import { AfterViewInit, Component, ViewChild, OnInit, OnDestroy } from '@angular/core';

import 'brace/index';
import 'brace/theme/eclipse';
import 'brace/theme/monokai';
import 'brace/mode/markdown';
import 'brace/mode/javascript';
import 'brace/ext/language_tools.js';
import { LogsService } from '../../services/logs.service';
import notify from 'devextreme/ui/notify';
import { BaseComponent } from '../../base/base.component';
import { WsMessage, WsMessageType, IWebsocketCallback } from '../../models/Entities';
import { environment } from '../../../environments/environment';
import { WebsocketService } from '../../services/websocket.service';

@Component({
  templateUrl: 'logs.component.html',
  providers: [LogsService, WebsocketService]
})
export class LogsComponent extends BaseComponent implements AfterViewInit, OnInit, OnDestroy, IWebsocketCallback {
  // @ViewChild('editor') editor;
  // text: string = defaults.markdown;

  options: any = {
    // maxLines: 1000,
    printMargin: false,
    wrap: true,
    showGutter: false
  };

  public text: string;

  constructor(public logS: LogsService, public ws: WebsocketService) {
    super();
    this.text = '';
  }

  public doConnect() {

    this.ws.doConnect(this);

  }

  ngOnInit() {
    super.ngOnInit();
    this.doConnect();
  }

  public getText(): string {
    return this.text;
  }

  public ClearLogs() {
    this.text = '';
    this.ws.doSend({ Type: WsMessageType.ClearLog, From: this.logS.currentUserToken.userName, Message: '' });
  }

  public RefreshLogs() {
    window.location.reload();
  }

  ngOnDestroy(): void {
    this.ws.doDisconnect();
    super.ngOnDestroy();
  }

  ngAfterViewInit() {
    // this.editor.setMode('markdown');
    // this.editor.setTheme('eclipse');
    // this.editor.setReadOnly(true);
  }

  public writeToScreen(message) {
    console.log(message);
  }

  public onOpen(evt: MessageEvent) {
    this.ws.doSend({ Type: WsMessageType.GetAllText, From: this.logS.currentUserToken.userName, Message: '' });
    this.writeToScreen('connected logs\n');
  }

  public onClose() {
    this.writeToScreen('disconnected logs\n');
  }

  public onMessage(msg: WsMessage) {
    if (msg) {
       switch (msg.Type) {
        case WsMessageType.WriteLog:
          this.text += msg.Message;
          break;
        case WsMessageType.GetAllText:
          this.text = msg.Message;
          break;
       }
    }
  }

  public onError(evt: MessageEvent) {
    notify('Connection Error: ' + evt.data);
    this.ws.doDisconnect();
  }

}
