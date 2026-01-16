import axios from "axios"

const requestInterceptor = (request) => {
    request.withCredentials = true
    return request
};

const api = axios.create({
    baseURL: '/'  // Base URL for all API requests
})
api.interceptors.request.use(requestInterceptor)

export default api