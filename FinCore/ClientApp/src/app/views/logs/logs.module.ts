import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LogsService} from '../../services/logs.service';

import { MonacoEditorModule,NgxMonacoEditorConfig } from 'ngx-monaco-editor';

// Routing
import { LogsRoutingModule } from './logs-routing.module';

import { LogsComponent } from './logs.component';

import { DxButtonModule, DxTemplateModule, DxTabPanelModule, DxBoxModule } from 'devextreme-angular';
import { WebsocketService } from '../../services/websocket.service';
import { CommonModule } from '@angular/common';

declare var monaco: any;

export function onMonacoLoad() {

  console.log((window as any).monaco);

  const uri = monaco.Uri.parse('a://b/foo.json');
  monaco.languages.json.jsonDefaults.setDiagnosticsOptions({
    validate: true,
    schemas: [{
      uri: 'http://myserver/foo-schema.json',
      fileMatch: [uri.toString()],
      schema: {
        type: 'object',
        properties: {
          p1: {
            enum: ['v1', 'v2']
          },
          p2: {
            $ref: 'http://myserver/bar-schema.json'
          }
        }
      }
    }, {
      uri: 'http://myserver/bar-schema.json',
      fileMatch: [uri.toString()],
      schema: {
        type: 'object',
        properties: {
          q1: {
            enum: ['x1', 'x2']
          }
        }
      }
    }]
  });

}

const monacoConfig: NgxMonacoEditorConfig = {
  baseUrl: 'assets',
  defaultOptions: {
    scrollBeyondLastLine: false,
  	lineNumbers: 'off',
	  readOnly: true,
  	includeInlayParameterNameHintsWhenArgumentMatchesName: false,
	  includeInlayFunctionParameterTypeHints: false,
	  includeInlayVariableTypeHints: false,
	  includeInlayPropertyDeclarationTypeHints: false,
	  includeInlayFunctionLikeReturnTypeHints: false,
	  includeInlayEnumMemberValueHints: false
 },
  onMonacoLoad
};


@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    LogsRoutingModule,
    MonacoEditorModule.forRoot(monacoConfig),
    DxButtonModule,
    DxTemplateModule,
    DxTabPanelModule,
    DxBoxModule
  ],
  providers: [
    LogsService,
    WebsocketService
  ],
  declarations: [
    LogsComponent
  ]
})
export class LogsModule { }
