import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LogsService} from '../../services/logs.service';

// ng2-ace-editor
import { AceEditorModule } from 'ng2-ace-editor';

// Routing
import { LogsRoutingModule } from './logs-routing.module';

import { LogsComponent } from './logs.component';
import { DxButtonModule } from 'devextreme-angular';
import { WebsocketService } from '../../services/websocket.service';

@NgModule({
  imports: [
    FormsModule,
    LogsRoutingModule,
    AceEditorModule,
    DxButtonModule
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
