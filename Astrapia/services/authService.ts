import RestService from "~/services/restService";

class AuthService {
    public async getStatus(): Promise<boolean> {
        const response = await RestService.get('/api/auth/status');
        return response.status == 204;
    }
}

export default new AuthService();
