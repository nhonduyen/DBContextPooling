import http from 'k6/http';
import { sleep } from 'k6';
import * as config from './config.js';

export default function () {
    http.put(config.API_BULK_UPDATE_ORDER_URL);
    sleep(1);
}