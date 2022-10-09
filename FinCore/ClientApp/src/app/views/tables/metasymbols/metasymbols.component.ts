import { Component, OnInit, ViewChild } from '@angular/core';
import notify from 'devextreme/ui/notify';
import { MetaSymbol, Adviser, SymbolS, SymbolType, EntitiesEnum } from '../../../models/Entities';
import { BaseComponent } from '../../../base/base.component';
import CustomStore from 'devextreme/data/custom_store';
import { DxDataGridComponent } from 'devextreme-angular';
import { PropertiesComponent } from '../properties/properties.component';
import { ExpertsService } from '../../../services/experts.service';

@Component({
  templateUrl: './metasymbols.component.html',
  styleUrls: ['./metasymbols.component.scss']
})
export class MetasymbolComponent extends BaseComponent implements OnInit {
  @ViewChild('metasymbolContainer') metasymbolContainer: DxDataGridComponent;
  @ViewChild(PropertiesComponent) propsContainer: PropertiesComponent;

  popupVisible = false;
  _showProperties = false;

  dataSource: CustomStore;
  childDataSymbolsMap: object = {};
  childDataAdvisersMap: object = {};
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
        load: () => this.experts.loadParentData(EntitiesEnum.MetaSymbol, this.showRetired)
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
    if (id === 1) {
      const ms: MetaSymbol = e.data;
      if (ms) {
        this.currentObject = ms;
        this.propsContainer.setData(ms.Id, 'MetaSymbol', ms);
      }
      return;
    }
  }

  public onClickAdvisersCell(e) {
    const id: number = e.columnIndex;
    // console.log('Click Adv' + id + ' ' + JSON.stringify(e.data));
    if (id === 0) {
      const adv: Adviser = e.data;
      if (adv) {
        this.currentObject = adv;
        this.propsContainer.setData(adv.Id, 'Adviser', adv);
      }
      return;
    }
  }

  public onClickSymbolsCell(e) {
    const id: number = e.columnIndex;
    if (id === 0) {
      const symbol: SymbolS = e.data;
      if (symbol) {
        this.currentObject = symbol;
        this.propsContainer.setData(symbol.Id, 'Symbol', symbol);
      }
      return;
    }
  }


  addObjectClick(entityName: string, parentId?: number) {
    const entityType: EntitiesEnum = EntitiesEnum[entityName];
    switch (entityType) {
       case EntitiesEnum.Adviser:
        this.currentObject = new Adviser();
        break;
        case EntitiesEnum.Symbol:
          this.currentObject = new SymbolS().createNew(parentId);
        break;
        case EntitiesEnum.MetaSymbol:
          this.currentObject = new MetaSymbol().createNew();
        break;
        default:
          return;
    }
    this.propsContainer.setData(undefined, entityName, this.currentObject);
  }

  doChangeRetired(e: any) {
    // console.log('doChangeRetired: ' + e.value);
    this.showRetired = e.value;
    this.loadData();
  }

  getCurrentTitle(): string {
    if (this.propsContainer && this.propsContainer.entityName) {
      return `${this.propsContainer.entityName} Properties`;
    }
    return 'Properties';
  }

  getChildData(childEntity: EntitiesEnum, parentId: number): CustomStore {
    return this.experts.loadChildData(EntitiesEnum.MetaSymbol, childEntity, parentId, this.showRetired);
  }

  loadSymbolsChildData(parentId: number) {
    return this.childDataSymbolsMap[parentId];
  }

  loadAdvisersChildData(parentId: number) {
    return this.childDataAdvisersMap[parentId];
  }

  onRowExpanding(event) {
    // console.log('Row expanding: ' + JSON.stringify(event.key.Id));
    this.loadChilds(event.key);
  }

  loadChilds(parentKey: number) {
    if (parentKey) {
      this.childDataSymbolsMap[parentKey] = this.getChildData(EntitiesEnum.Symbol, parentKey);
      this.childDataAdvisersMap[parentKey] = this.getChildData(EntitiesEnum.Adviser, parentKey);
    }
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
            text: 'Add MetaSymbol',
            onClick: this.addObjectClick.bind(this, 'MetaSymbol')
        }
      },
      {
          location: 'after',
          widget: 'dxCheckBox',
          options: {
              width: 150,
              text: 'Show Retired',
              value: this.showRetired,
              onValueChanged: this.doChangeRetired.bind(this)
          }
      });
      /*,
      {
          location: 'before',
          widget: 'dxButton',
          options: {
              width: 200,
              text: 'Collapse All',
              onClick: this.collapseAllClick.bind(this)
          }
      });
      */
  }

  onToolbarPreparingSymbol(e, parentId: number) {
    e.toolbarOptions.items.unshift({
        location: 'before',
        widget: 'dxButton',
        options: {
            width: 220,
            text: 'Add Symbol',
            onClick: this.addObjectClick.bind(this, 'Symbol', parentId)
        }
      });
  }

  collapseAllClick(e) {
    this.expanded = !this.expanded;
    e.component.option({
        text: this.expanded ? 'Collapse All' : 'Expand All'
    });
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
            /* console.log('Updated successfully: ' + JSON.stringify(this.currentObject));
            this.dataSource.push([
              {
                type: 'update',
                key: key,
                data: this.currentObject
              }
            ]);*/
            this.propsContainer.updateProperty(false);
            // window.location.reload();
            this.loadData(true);
            if (this.propsContainer.entityType === EntitiesEnum.Adviser) {
              this.updateAdviser(this.currentObject);
            }

          }
          , error => this.logConsoleError(error));
    } else {
        // this.propsContainer.updateProperty(true);
        console.log(this.propsContainer.dataSource);
        this.subs.sink = this.experts.pushStore(this.propsContainer.entityType, this.toDBObj(key)).subscribe(
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

  public onClose() {
    this.propsContainer.close();
  }

  public updateAdviser(obj: Adviser) {
    // const strData: string = JSON.stringify(this.adviserState);
    // this.currentAdviser.State = strData;
    this.subs.sink = this.experts.updateAdviserState(obj)
      .subscribe(
      data => {
        console.log('Adviser updated:');
      },
      error => this.logNotifyError(error));
  }


}
