import { Component, OnInit, ViewChild } from '@angular/core';
import { SymbolType, Rates, EntitiesEnum, Settings } from '../../../models/Entities';
import { BaseComponent } from '../../../base/base.component';
import CustomStore from 'devextreme/data/custom_store';
import { DxDataGridComponent } from 'devextreme-angular';
import { PropertiesComponent } from '../properties/properties.component';
import { DealsService } from '../../../services/deals.service';

@Component({
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.scss']
})
export class SettingsComponent extends BaseComponent implements OnInit {
  @ViewChild('setContainer') setContainer: DxDataGridComponent;
  @ViewChild(PropertiesComponent) propsContainer: PropertiesComponent;

  popupVisible = false;
  _showProperties = false;
  dataSource: CustomStore;
  expanded = false;
  public masterDetailInfo = { enabled: true, autoExpandAll: this.expanded, template: 'detail' };
  showDisabled: boolean;
  currentObject: any;

  set showProperties(val: boolean) {
    this._showProperties = val;
  }

  get showProperties(): boolean {
    return this._showProperties;
  }

  constructor(public deals: DealsService) {
    super();
    this.showDisabled = true;
  }

  loadData(fullRefresh = false) {
    this.dataSource = new CustomStore({
        key: 'Id',
        load: () => this.deals.loadParentData(EntitiesEnum.Settings, this.showRetired)
            .toPromise()
            .then(data => this.dataSource = data)
            .catch(error => this.logNotifyError(error)),
    });
  }

  ngOnInit() {
    this.loadData();
  }

  public onClickParentCell(e) {
    const id: number = e.columnIndex;
    if (id === 0) {
      const s: Settings = e.data;
      if (s) {
        this.currentObject = s;
        this.propsContainer.setData(s.Id, 'Settings', s);
      }
      return;
    }
  }

  addObjectClick(entityName: string, parentId?: number) {
    const entityType: EntitiesEnum = EntitiesEnum[entityName];
    switch (entityType) {
       case EntitiesEnum.Settings:
        this.currentObject = new Settings().createNew();
       break;
       default:
       return;
    }
    this.propsContainer.setData(undefined, entityName, this.currentObject);
  }

  getCurrentTitle(): string {
    if (this.propsContainer && this.propsContainer.entityName) {
      return `${this.propsContainer.entityName} Properties`;
    }
    return 'Properties';
  }

  SymbolInfo(data): string {
    return SymbolType[data];
  }

  onToolbarPreparing(e) {
    e.toolbarOptions.items.unshift({
      location: 'before',
      widget: 'dxButton',
      options: {
          width: 220,
          text: 'Add Setting',
          onClick: this.addObjectClick.bind(this, 'Settings')
      }
    });
  }

  public syncAll(e) {
    this.subs.sink = this.deals.runJob('SYSTEM', 'TerminalsSyncJob').subscribe(
      data => {
        window.location.reload();
      },
      error => this.logNotifyError(error)
    );
  }

  toDBObj(key: number): any {
    if (this.currentObject) {
      const entries = new Map<string, any>();
      Object.keys(this.currentObject).forEach(element => {
        entries.set(element, this.propsContainer.dataSource[element].value);
      });
      entries.set('Id', key);
      this.currentObject = Object.fromEntries(entries);
      // console.log(this.currentObject);
    }
    return this.currentObject;
  }

  public onClose() {
    this.propsContainer.close();
  }

  onSave() {
    const key = this.propsContainer.objId;
    if (key) {
        this.subs.sink = this.deals.updateStore(this.propsContainer.entityType, key, this.toDBObj(key)).subscribe(
          data => {
            this.propsContainer.updateProperty(false);
            // window.location.reload();
            this.loadData(true);
          }
          , error => this.logConsoleError(error));
    } else {
        console.log(this.propsContainer.dataSource);
        this.subs.sink = this.deals.pushStore(this.propsContainer.entityType, this.toDBObj(key)).subscribe(
        data => {
          console.log('Added successfully: ' + this.propsContainer.entityType.toString() + ', Id=' + data);
          this.currentObject.Id = data;
          // this.dataSource.push(this.currentObject);
          this.propsContainer.parentVisibleChange.emit(false);
          // window.location.reload();
          this.loadData(true);
        }
        , error => this.logConsoleError(error));
    }
  }
}
