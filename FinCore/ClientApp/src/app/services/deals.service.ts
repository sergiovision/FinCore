import {
  BaseService
} from './base.service';
import {
  Injectable
} from '@angular/core';
import {
  HttpClient
} from '@angular/common/http';
import notify from 'devextreme/ui/notify';

@Injectable()
export class DealsService extends BaseService {
  constructor(http: HttpClient) {
    super(http);
  }

  public getAll() {
    return super.getAll('/api/deals');
  }

  public getTodayDeals() {
    return super.getAll('/api/deals/GetToday');
  }

  public getTodayStat() {
    return super.getAll('/api/deals/GetTodayStat');
  }

  public getStat(accountType: number) {
    return super.getAll('/api/deals/MetaSymbolStatistics?type=' + accountType);
  }

  public closePosition(account: number, magic: number, ticket: number) {
    const uri: string = '/api/deals/ClosePosition?account=' + account + '&magic=' + magic + '&ticket=' + ticket;
    return super.getAll(uri);
  }

  public refreshAll() {
    return super.getAll('/api/deals/RefreshAll');
  }

}
