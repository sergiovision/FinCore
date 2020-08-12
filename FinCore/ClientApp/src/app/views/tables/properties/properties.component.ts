import { Component, OnInit, Input, OnDestroy, Output, EventEmitter } from '@angular/core';
import { BaseComponent } from '../../../base/base.component';
import { PropsService } from '../../../services/props.service';
import notify from 'devextreme/ui/notify';
import { group } from '@angular/animations';
import { DynamicProperties, DynamicProperty, EntitiesEnum, DynamicPropertyDefinition } from '../../../models/Entities';

@Component({
  selector: 'app-properties',
  templateUrl: './properties.component.html',
  styleUrls: ['./properties.component.scss']
})
export class PropertiesComponent extends BaseComponent implements OnInit, OnDestroy {
  @Input() parentVisible = false;
  @Output() parentVisibleChange = new EventEmitter<boolean>();

  public objId: number;
  public defs: {[key: string]: any };
  public responseData: DynamicProperties;
  public dataSource: {[key: string]: DynamicProperty<any> };
  public entityName = '';
  public entityType: EntitiesEnum;

  constructor(public props: PropsService) {
    super();
    this.dataSource = undefined;
    // tslint:disable-next-line: max-line-length
    // this.dataSource = JSON.parse('{"ID":{"type":"integer","name":"ID","group":"System","value":3},"ObjectID":{"type":"integer","name":"ObjectID","group":"System","value":"3"}}');
  }

  ngOnInit() {
    super.ngOnInit();
  }

  get UpdatedProperty(): Date {
     let result = new Date(2100, 1, 1);
     if (this.responseData) {
        result = this.responseData.updated;
     }
     return result;
  }

  set UpdatedProperty(val: Date) {
  }

  setData(val: number, entityName: string, dbData?: any): void {
    this.objId = val;
    this.entityName = entityName;
    this.entityType = EntitiesEnum[entityName];
    this.subs.unsubscribe();
    const props_def = this.props.getDefinitionsForEntity(entityName);
    if (props_def) {
      this.defs = Object.entries(props_def);
      // console.log(props_def);
      const mydefs: Map<string, DynamicPropertyDefinition<any> > = new Map<string, DynamicPropertyDefinition<any> >();
      // tslint:disable-next-line: forin
      for (const key in props_def) {
        props_def[key].forEach( elements => {
          mydefs.set(elements.name, elements);
        });
      }
      if (!this.objId) {
        console.log('No Object provided loading default.');
        this.loadData(undefined, dbData, mydefs);
        return;
      }
      // console.log(this.defs);
      this.subs.sink = this.props.getInstance(this.entityType, this.objId)
        .subscribe(
            data => this.loadData(data, dbData, mydefs)
            , error => this.logConsoleError(error));
    }
  }

  loadData(data: any, dbData: any, mydefs: Map<string, DynamicPropertyDefinition<any> > ) {

      // console.log('Data loaded for MetaSymbol ObjId=', this.objId);
      this.responseData = data;
      let setupData: Map<string, DynamicProperty<any> > = new Map();

      if (this.responseData) {
         setupData = JSON.parse(this.responseData.Vals);
      }

      this.dataSource = {};

      // console.log(setupData);
      mydefs.forEach(el => {
        const resval = setupData[el.name]; // vals.find(e => e.name === element.name);
        if ( resval === undefined ) {
          console.log('property: ' + el.name + ' is not set. Set to default.');
          const newProp: DynamicProperty<any> = {
             value: el.defaultValue,
             type: el.type,
          };
          this.dataSource[el.name] = newProp;
        } else {
          this.dataSource[el.name] = resval;
        }
      });

      if (dbData) {
        const dbEntries = Object.entries(dbData);
        if (dbEntries) {
          // console.log(JSON.stringify(dbEntries));
          dbEntries.forEach(el => {
            // console.log('El: ' + el[0] + ' ' + el[1]);
            const resval = setupData[el[0]];
            if (resval === undefined) {
              // console.log('resval undefined: ' + JSON.stringify(this.dataSource[el[0]]));
              const newProp: DynamicProperty<any> = {
                value: el[1],
                type: 'number',
              };
              this.dataSource[el[0]] = newProp;
            } else {
              // console.log('resval:' + JSON.stringify(resval));
              const newProp: DynamicProperty<any> = {
                value: el[1],
                type: resval.type,
              };
              this.dataSource[el[0]] = newProp;
            }
          });
        }
      }

      if (this.responseData) {
        this.UpdatedProperty = this.responseData.updated;
      }
      this.parentVisibleChange.emit(true);
  }

  onValueChanged(e) {
    // if (!e.value) {
      // if (this.dataSource) {
        // e.value = 'Hello'; // this.dataSource[ this.defs['name'] ].value;
     // e.component.value = 'Hello';
      // }
    // } else {
      // this.dataSource[ def['name'] ].value:0
     //   console.log(e);
      // }
  }

  ngOnDestroy(): void {
    this.dataSource = undefined;
    super.ngOnDestroy();
  }

  public updateProperty(isNew: boolean, skipSave?: boolean) {
    if (!isNew) {
      this.dataSource[ 'Id' ].value = this.responseData.Id;
    }
    if (!skipSave) {
      // this.dataSource[ 'ObjectID' ].value = this.responseData.objId;
      if (this.responseData) {
        this.responseData.Vals = JSON.stringify(this.dataSource);
      }
      this.subs.sink = this.props.saveInstance(this.responseData)
      .subscribe(
      data => {
            // console.log('Data saved: ');
            // console.log(this.responseData);
            this.close();
      },
      error => this.logNotifyError(error));
    } else {
      this.close();
    }

  }

  public close() {
    this.dataSource = undefined;
    this.parentVisibleChange.emit(false);
  }

}
