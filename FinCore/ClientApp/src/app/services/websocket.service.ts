import { BaseService } from './base.service';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { WsMessage, IWebsocketCallback } from '../models/Entities';

@Injectable({ providedIn: 'root' })
export class WebsocketService extends BaseService {

  private websocket: WebSocket;
  constructor(http: HttpClient) {
    super(http);
    // console.log('WebsocketService c-tor');
  }

  public doConnect(obj: IWebsocketCallback): void {
    console.log('Connect WS: ' + environment.wsURL);
    this.websocket = new WebSocket(environment.wsURL);
    this.websocket.onopen = function(evt) {
      obj.onOpen(evt);
      // console.log('onopen');
    }.bind(obj);
    this.websocket.onclose = function(evt) {
      obj.onClose();
      // console.log('onclose');
    }.bind(obj);
    this.websocket.onmessage = function(evt) {
      const msg: WsMessage = JSON.parse(evt.data);
      obj.onMessage(msg);
      // console.log('onmessage');
    }.bind(obj);
    this.websocket.onerror = function(evt) {
      console.log('onerror: ' + evt.data);
      obj.onError(evt);
    }.bind(obj);

  }

  public doSend(message: WsMessage) {
    const str: string = JSON.stringify(message);
    this.websocket.send(str);
  }

  public doDisconnect() {
    this.websocket.close();
  }

}
