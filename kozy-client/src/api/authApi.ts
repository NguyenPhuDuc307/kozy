import axiosClient from "./axiosClient";

export const authApi = {
    register: (data: { email: string; password: string }) =>
        axiosClient.post("/auth/register", data),
    login: (data: { email: string; password: string }) =>
        axiosClient.post("/auth/login", data),
    getAll: () => axiosClient.get("/auth")
};
