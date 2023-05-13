import { Env } from '../app/models/Entities';

export const environment: Env = {
  production: true,
  baseURL: 'http://www.sergego.com/fincore',
  wsURL: 'ws://www.sergego.com:2021',
  dateFormat: "yyyy-MM-dd'T'HH:mm:ss'Z'",
  shortDateFormat: 'yyyy-MM-dd'
};
