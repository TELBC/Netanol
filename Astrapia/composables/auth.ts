import {useState} from "#app";
import {Ref} from "@vue/reactivity";
import RestService from "~/services/restService";
import AuthService from "~/services/authService";
import axios from "axios";

interface AuthState {
    isAuthenticated: boolean,
    message: string
}

export const useAuth = (): Ref<AuthState> => {
    return useState<AuthState>('auth', (): AuthState => ({
        isAuthenticated: false,
        message: ''
    }));
}
