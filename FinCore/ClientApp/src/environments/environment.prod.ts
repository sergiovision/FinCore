import { Env } from '../app/models/Entities';

export const environment: Env = {
  production: true,
  baseURL: 'http://localhost:2020',
  wsURL: 'ws://localhost:2021',
  dateFormat: "yyyy-MM-dd'T'HH:mm:ss'Z'",
  shortDateFormat: 'yyyy-MM-dd'
};
