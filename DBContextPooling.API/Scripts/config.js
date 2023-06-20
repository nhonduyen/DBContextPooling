const API_BASE_URL = 'https://localhost:7284/';
const API_UPDATE_ORDER_URL = API_BASE_URL + 'api/Orders/UpdateMany?quantity=1000';
const API_BULK_UPDATE_ORDER_URL = API_BASE_URL + 'api/Orders/BulkUpdateMany?quantity=1000';
const API_CREATE_MANY_ORDER_URL = API_BASE_URL + 'api/Orders/CreateMany?quantity=1000';
const API_BULK_CREATE_MANY_ORDER_URL = API_BASE_URL + 'api/Orders/BulkCreateMany?quantity=1000';

const API_GET_ORDER_URL = API_BASE_URL + 'api/Orders/Customer?id=ec99e4f0-058a-44e8-5fb5-08db6da6a694';

export { API_UPDATE_ORDER_URL, API_BULK_UPDATE_ORDER_URL, API_CREATE_MANY_ORDER_URL, API_BULK_CREATE_MANY_ORDER_URL };