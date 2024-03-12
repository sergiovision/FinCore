import { Env } from '../app/models/Entities';

export const environment: Env = {
  production: true,
  baseURL: 'http://127.0.0.1:2020/fincore',
  wsURL: 'ws://127.0.0.1:2021',
  dateFormat: "yyyy-MM-dd'T'HH:mm:ss'Z'",
  shortDateFormat: 'yyyy-MM-dd'
};

import config from 'devextreme/core/config';
import { licenseKey } from '../devextreme-license';

config({ licenseKey });
