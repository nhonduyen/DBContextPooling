import http from 'k6/http';
import { sleep } from 'k6';
import * as config from './config.js';

export default function () {
    http.post(config.API_CREATE_MANY_ORDER_URL);
    http.put(config.API_UPDATE_ORDER_URL);
    sleep(1);
}