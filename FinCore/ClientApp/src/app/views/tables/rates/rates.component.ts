import { Component, OnInit, ViewChild } from '@angular/core';
import { SymbolType, Rates, EntitiesEnum } from '../../../models/Entities';
import { BaseComponent } from '../../../base/base.component';
import CustomStore from 'devextreme/data/custom_store';
import { DxDataGridComponent } from 'devextreme-angular';
import { PropertiesComponent } from '../properties/properties.component';
import { ExpertsService } from '../../../services/experts.service';

@Component({
  templateUrl: './rates.component.html',
  styleUrls: ['./rates.component.scss']
})
export class RatesComponent extends BaseComponent implements OnInit {
  @ViewChild('ratesContainer') ratesContainer: DxDataGridComponent;
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

  constructor(public experts: ExpertsService) {
    super();
    this.showDisabled = true;
  }

  loadData(fullRefresh = false) {
    this.dataSource = new CustomStore({
        key: 'Id',
        load: () => this.experts.loadParentData(EntitiesEnum.Rates)
            .toPromise()
            .then((data: any) => {
                this.dataSource = data;
            })
            .catch(error => this.logNotifyError(error)),
    });
  }

  ngOnInit() {
    this.loadData();
  }

  public onClickParentCell(e) {
    const id: number = e.columnIndex;
    if (id === 0) {
      const r: Rates = e.data;
      if (r) {
        this.currentObject = r;
        this.propsContainer.setData(r.Id, 'Rates', r);
      }
      return;
    }
  }

  addObjectClick(entityName: string, parentId?: number) {
    const entityType: EntitiesEnum = EntitiesEnum[entityName];
    switch (entityType) {
       case EntitiesEnum.Rates:
        this.currentObject = new Rates().createNew();
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
    e.toolbarOptions.items.unshift(/* {
        location: 'before',
        widget: 'dxButton',
        options: {
            width: 220,
            text: 'Add Rate',
            onClick: this.addObjectClick.bind(this, 'Rates')
        }
      }, */
      {
      location: 'before',
      widget: 'dxButton',
      options: {
        width: 120,
        icon: 'refresh',
        onClick: this.syncAll.bind(this)
      }
    });
  }

  public syncAll(e) {
    this.subs.sink = this.experts.runJob('SYSTEM', 'TerminalsSyncJob').subscribe(
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

  onSave() {
    const key = this.propsContainer.objId;
    if (key) {
        // JSON.stringify(this.propsContainer.dataSource);
        this.subs.sink = this.experts.updateStore(this.propsContainer.entityType, key, this.toDBObj(key)).subscribe(
          data => {
            this.propsContainer.updateProperty(false);
            // window.location.reload();
            this.loadData(true);

          }
          , error => this.logConsoleError(error));
    } else {
        // this.propsContainer.updateProperty(true);
        console.log(this.propsContainer.dataSource);
        this.subs.sink = this.experts.pushStore(this.propsContainer.entityType, this.toDBObj(key)).subscribe(
        data => {
          console.log('Added successfully: ' + this.propsContainer.entityType.toString() + ', Id=' + data);
          this.currentObject.Id = data;
          this.propsContainer.parentVisibleChange.emit(false);
          // window.location.reload();
          this.loadData(true);
        }
        , error => this.logConsoleError(error));
    }
  }

}
