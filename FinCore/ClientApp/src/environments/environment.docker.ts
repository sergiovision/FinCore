import { Env } from '../app/models/Entities';

export const environment = {
  production: false,
  baseURL: `http://${window.location.hostname}:2020`,
  wsURL: `ws://${window.location.hostname}:2021`,
  dateFormat: "yyyy-MM-dd'T'HH:mm:ss'Z'",
  shortDateFormat: 'yyyy-MM-dd'
};
