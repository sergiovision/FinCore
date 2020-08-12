import { WalletsService } from '../../../services/wallets.service';
import { Component, OnInit, ViewChild } from '@angular/core';
import { AccountState, AccountView, Asset, Wallet, Account, Terminal, AccountType, EntitiesEnum } from '../../../models/Entities';
import { BaseComponent } from '../../../base/base.component';
import { DxDataGridComponent } from 'devextreme-angular';
import { PropertiesComponent } from '../properties/properties.component';
import CustomStore from 'devextreme/data/custom_store';
import notify from 'devextreme/ui/notify';

@Component({
  templateUrl: './wallets.component.html',
  styleUrls: ['./wallets.component.scss']
})
export class WalletsComponent extends BaseComponent implements OnInit {
  @ViewChild('walletsContainer') metasymbolContainer: DxDataGridComponent;
  @ViewChild(PropertiesComponent) propsContainer: PropertiesComponent;
  dataSource: any;
  showDisabled: boolean;
  currentState: AccountState;
  popupVisible = false;
  childDataAccountsMap: object = {};
  childDataTerminalsMap: object = {};

  pieSource: Asset[];
  expanded = false;
  currentObject: any;
  public masterDetailInfo = { enabled: true, autoExpandAll: this.expanded, template: 'detail' };
  _showProperties = false;
  walletSymbolsMap: object = {};

  constructor(public wallets: WalletsService) {
    super();
    this.showDisabled = true;
    this.currentState = new AccountState();
    this.pieSource = undefined;
  }

  loadData(fullRefresh = false) {

   this.subs.sink = this.wallets.getAll()
        .subscribe(
            data => {
              this.dataSource = data;
              if (fullRefresh)  {
                this.dataSource.forEach(element => {
                  this.loadChilds(element.Id);
                });
              }

            },
            error => this.logConsoleError(error));


    /* this.dataSource = new CustomStore({
          key: 'Id',
          load: () => this.wallets.loadParentData(EntitiesEnum.Wallet)
                      .toPromise()
                      .then((data: any) => {
                          this.dataSource = data.filter(item => !item.Retired);
                          if (fullRefresh)  {
                            this.dataSource.forEach(element => {
                              this.loadChilds(element.Id);
                            });
                          }
                      })
                      .catch(error => {
                        const message = JSON.stringify(error.error);
                        console.log(message);
                      })
        });
*/

    this.subs.sink = this.wallets.assetsDistribution(0)
          .subscribe(
          data => {
            this.pieSource = data;
            this.pieSource = this.pieSource.filter((item) => item.SharePercentValue > 1);
          },
          error => this.logConsoleError(error));
  }

  set showProperties(val: boolean) {
    this._showProperties = val;
  }

  get showProperties(): boolean {
    return this._showProperties;
  }

  ngOnInit() {
    this.loadData(true);
  }

  public onClickAccountCell(e) {
    const id: number = e.columnIndex;
    if (id === 0) {
      const account: AccountView = e.data;
      this.currentState = new AccountState();
      this.currentState.Balance = account.Balance;
      this.currentState.AccountId = account.Id;
      this.popupVisible = true;
      return;
    }
    if (id === 1) {
      const acc: Account = e.data;
      if (acc) {
        this.currentObject = acc;
        // console.log(JSON.stringify(acc));
        this.propsContainer.setData(acc.Id, 'Account', acc);
      }
      return;
    }
  }



   public updateAccountState() {
    this.subs.sink = this.wallets.updateAccountState(this.currentState)
       .subscribe(
        data => {
          this.loadData();
        },
        error => this.logNotifyError(error));
       this.popupVisible = false;
       this.loadData(true);
   }

  customizePieLabel(arg) {
    const value = arg.value;
    return value.toFixed(2) + ' (' + arg.percentText + ')';
  }

  getCurrentTitle(): string {
    if (this.propsContainer && this.propsContainer.entityName) {
      return `${this.propsContainer.entityName} Properties`;
    }
    return 'Properties';
  }

  addObjectClick(entityName: string, parentId?: number) {
    const entityType: EntitiesEnum = EntitiesEnum[entityName];
    switch (entityType) {
       case EntitiesEnum.Wallet:
        this.currentObject = new Wallet().createNew(undefined);
        break;
        case EntitiesEnum.Account:
          this.currentObject = new Account().createNew(parentId);
        break;
        default:
          return;
    }
    this.propsContainer.setData(undefined, entityName, this.currentObject);
  }

  public genDeployScripts() {
    this.subs.sink =  this.wallets.genDeployScripts();
  }

  onRowExpanding(event) {
    this.loadChilds(event.key);
  }

  AccountInfo(data): string {
    return AccountType[data];
  }

  loadChilds(parentKey: number) {
    if (parentKey) {
      this.childDataAccountsMap[parentKey] = this.getChildData(EntitiesEnum.Account, parentKey);
      this.childDataTerminalsMap[parentKey] = this.getChildData(EntitiesEnum.Terminal, parentKey);
    }
  }

  getChildData(childEntity: EntitiesEnum, parentId: number): CustomStore {
    return this.wallets.loadChildData(EntitiesEnum.Wallet, childEntity, parentId);
  }

  loadAccountsChildData(parentId: number) {
    return this.childDataAccountsMap[parentId];
  }

  loadTerminalChildData(parentId: number) {
    return this.childDataTerminalsMap[parentId];
  }

  onToolbarPreparing(e) {
    e.toolbarOptions.items.unshift({
        location: 'before',
        widget: 'dxButton',
        options: {
          width: 220,
          text: 'Add Wallet',
          onClick: this.addObjectClick.bind(this, 'Wallet')
        }
    });
  }

  public onClickParentCell(e) {
    const id: number = e.columnIndex;
    if (id === 1) {
      const w: Wallet = e.data;
      if (w) {
        this.currentObject = w;
        this.propsContainer.setData(w.Id, 'Wallet', w);
      }
      return;
    }
  }

  public onClickTerminalsCell(e) {
    const id: number = e.columnIndex;
    if ( id === 1 ) {
        const t: Terminal = e.data;
        if (t) {
          this.currentObject = t;
          // console.log(JSON.stringify(this.propsContainer));
          this.propsContainer.setData(this.currentObject.Id, 'Terminal', t);
        }
        return;
    }

    if (id === 4) {
      const t: Terminal = e.data;
      // console.log(JSON.stringify(t));
      this.subs.sink = this.wallets.deployTerminal(t.Id);
      return;
    }
  }

  onToolbarPreparingAccount(e, parentId: number) {
    e.toolbarOptions.items.unshift({
        location: 'before',
        widget: 'dxButton',
        options: {
            width: 220,
            text: 'Add Account',
            onClick: this.addObjectClick.bind(this, 'Account', parentId)
        }
      });
  }

  onToolbarPreparingTerminal(e, parentId: number) {
    e.toolbarOptions.items.unshift(/* {
        location: 'before',
        widget: 'dxButton',
        options: {
            width: 220,
            text: 'Add Terminal',
            onClick: this.addObjectClick.bind(this, 'Terminal', parentId)
        }
      }, */
      {
        location: 'before',
        widget: 'dxButton',
        options: {
            width: 220,
            text: 'Generate Deploy Scripts',
            onClick: this.genDeployScripts.bind(this)
        }
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
        this.subs.sink = this.wallets.updateStore(this.propsContainer.entityType, key, this.toDBObj(key))
          .subscribe(
          data => {
            this.propsContainer.updateProperty(false);
            if (this.propsContainer.entityType === EntitiesEnum.Terminal) {
                this.subs.sink = this.wallets.updateTerminal(this.currentObject)
                .subscribe(
                 d => { console.log('Terminal updated'); },
                 error => this.logNotifyError(error));
            }
            this.loadData(true);
          }
          , error => this.logConsoleError(error));
    } else {
        // this.propsContainer.updateProperty(true);
        console.log(this.propsContainer.dataSource);
        this.subs.sink = this.wallets.pushStore(this.propsContainer.entityType, this.toDBObj(key)).subscribe(
        data => {
          console.log('Added successfully: ' + this.propsContainer.entityType.toString() + ', Id=' + data);
          this.currentObject.Id = data;
          // this.dataSource.push(this.currentObject);
          this.propsContainer.parentVisibleChange.emit(false);
          if (this.propsContainer.entityType === EntitiesEnum.Account) {
            this.currentState = new AccountState();
            this.currentState.AccountId = data;
            this.currentState.Comment = '';
            this.currentState.Balance = 0;
            this.currentState.Date = new Date();
            this.updateAccountState();
            return;
          }
          // window.location.reload();
          this.loadData(true);
        }
        , error => this.logConsoleError(error));
    }

  }

  public onClose() {
    this.propsContainer.close();
  }


}
