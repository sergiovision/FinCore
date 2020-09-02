import { AfterViewInit, Component, ViewChild, OnInit, OnDestroy, ViewChildren, QueryList } from '@angular/core';

import 'brace/index';
import 'brace/theme/eclipse';
import 'brace/theme/monokai';
import 'brace/mode/markdown';
import 'brace/mode/javascript';
import 'brace/ext/language_tools.js';
import { LogsService } from '../../services/logs.service';
import notify from 'devextreme/ui/notify';
import { BaseComponent } from '../../base/base.component';
import { WsMessage, WsMessageType, IWebsocketCallback, LogItem } from '../../models/Entities';
import { environment } from '../../../environments/environment';
import { WebsocketService } from '../../services/websocket.service';
import { AceEditorComponent } from 'ng2-ace-editor';
import { DxTabPanelComponent } from 'devextreme-angular';
import 'brace/theme/terminal';
import 'brace/theme/solarized_dark';
import 'brace/theme/vibrant_ink';
import 'brace/theme/github';
import 'brace/theme/solarized_light';
import 'brace/mode/yaml';


@Component({
  templateUrl: 'logs.component.html',
  styleUrls: ['logs.component.scss'],
  providers: [LogsService, WebsocketService]
})
export class LogsComponent extends BaseComponent implements AfterViewInit, OnInit, OnDestroy, IWebsocketCallback {
  @ViewChild('tabPanel0') tabPanel0: DxTabPanelComponent;
  @ViewChildren('editor') viewChildren: QueryList<AceEditorComponent>;


  // https://ace.c9.io/#nav=higlighter
  // tester https://ace.c9.io/build/kitchen-sink.html
  // https://codepen.io/ryancat/pen/mMyvpx/?css-preprocessor=sass
  options: any = {
    // maxLines: 1000,
    printMargin: false,
    theme: 'ace/theme/solarized_dark',
    mode: 'ace/mode/yaml',
    wrap: true,
    readOnly: true,
    showGutter: false,
    highlightActiveLine: true,
    cursorStyle: 'ace',
    animatedScroll: true,
    showLineNumbers: false,
    showInvisibles: false,
    newLineMode: 'auto'
  };

  public NumberOfLines = 500;
  public selectedIndex  = 0;

  public logz: LogItem[];

  constructor(public logS: LogsService, public ws: WebsocketService) {
    super();
    // this.text = '';
  }

  public doConnect() {

    this.ws.doConnect(this);

  }

  ngOnInit() {
    super.ngOnInit();

    this.subs.sink = this.logS.GetLogList()
    .subscribe(data  => {
        this.logz = data;
        this.doConnect();
      },
        error => this.logConsoleError(error)
      );
  }

  public getText(): string {
    return this.logz[0].DataSource;
  }

  public ClearLogs() {
    this.logz[0].DataSource = '';
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
  }

  public writeToScreen(message) {
    console.log(message);
  }

  public onOpen(evt: MessageEvent) {
    this.ws.doSend({ Type: WsMessageType.GetAllText, From: this.logS.currentUserToken.userName, Message: '' });

    this.writeToScreen('connected logs\n');
  }

  public selectTab(e) {
    const logitem = this.logz[this.selectedIndex];
    console.log('Click MainLog tab index: ' + this.selectedIndex + ', name: ' + logitem.Name);

    this.subs.sink = this.logS.GetLogContent(logitem.Name, this.NumberOfLines)
    .subscribe(data  => {
        if (this.selectedIndex > 0) {
          this.logz[this.selectedIndex].DataSource = data;
        }

        const editorX = this.viewChildren.find((element, index) => index === this.selectedIndex);
        // console.log(' viewChildren.count = ' + this.viewChildren.length + ', item: ' + editorX);
        const editor = editorX.getEditor();
        if (this.selectedIndex > 0) {
          editorX.setText(data);
        }
        let NumOfLines = this.NumberOfLines;
        if (this.selectedIndex === 0)   {
          NumOfLines = editor.session.doc.getAllLines().length;
        } else {
          NumOfLines = this.NumberOfLines;
        }
        if (editor) {
          this.options.theme = this.logz[this.selectedIndex].Theme;
          editorX.setOptions(this.options);
          console.log('NumOfLines ' + NumOfLines);
          editor.resize(true);
          editor.scrollToLine(NumOfLines, true, true, function () {});
          editor.gotoLine(NumOfLines, 10, true);
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
          // tslint:disable-next-line: no-shadowed-variable
          let str = msg.Message;
          // const str = msg.Message.replace(/^"(.+(?="$))"$/, '$1');
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
    notify('Connection Error: ' + evt.data);
    this.ws.doDisconnect();
  }

}
