import http from 'k6/http';
import { sleep } from 'k6';
import * as config from './config.js';

export const options = {
    stages: [
        { duration: '5s', target: 5 },
        { duration: '30s', target: 10 },
        { duration: '5s', target: 5 },
        { duration: '30s', target: 10 },
        { duration: '5s', target: 5 },
        { duration: '30s', target: 10 },
        { duration: '5s', target: 0 },
    ],
    thresholds: {
        http_req_duration: ['p(95)<600'],
    },
};

export default () => {
    http.put(config.API_BULK_UPDATE_ORDER_URL);
    sleep(1);
};