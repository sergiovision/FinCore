import { Env } from '../app/models/Entities';

export const environment: Env = {
  production: true,
  baseURL: `http://${window.location.hostname}:2020/fincore`,
  wsURL: `ws://${window.location.hostname}:2021`,
  dateFormat: "yyyy-MM-dd'T'HH:mm:ss'Z'",
  shortDateFormat: 'yyyy-MM-dd'
};

import config from 'devextreme/core/config';
import { licenseKey } from '../devextreme-license';

config({ licenseKey });
