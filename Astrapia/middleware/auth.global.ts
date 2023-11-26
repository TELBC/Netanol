import {useAuth} from "~/composables/auth"
import { defineNuxtRouteMiddleware, navigateTo} from "#app"

export default defineNuxtRouteMiddleware((to: any, from: any): any => {
    return;

    /*
    // This is inefficient but whatever
    // Hey btw to whoever reads this currently sitting in school here supposed to learn about the GDPR ;)
    if (to.path === '/login' || from.path === '/login' || from.path === '/')
        return;

    const auth = useAuth()
    if (!auth.value.isAuthenticated)
        return navigateTo('/login')
     */
})
