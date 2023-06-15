import http from 'k6/http';
import { sleep } from 'k6';
import * as config from './config.js';

export const options = {
    vus: 1,
    duration: '1m',

    thresholds: {
        http_req_duration: ['p(95)<1000'] // we want at least 95% of requests to respond in < 1 second.
    },
};

export default function () {
    http.get(config.API_GET_ORDER_URL);
    http.put(config.API_UPDATE_ORDER_URL);
    sleep(1);
}