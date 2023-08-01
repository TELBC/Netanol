import colors from 'vuetify/es5/util/colors'

export default {
  // Disable server-side rendering: https://go.nuxtjs.dev/ssr-mode
  ssr: false,

  // Global page headers: https://go.nuxtjs.dev/config-head
  head: {
    titleTemplate: '%s - Manager',
    title: 'Manager',
    htmlAttrs: {
      lang: 'en'
    },
    meta: [
      { charset: 'utf-8' },
      { name: 'viewport', content: 'width=device-width, initial-scale=1' },
      { hid: 'description', name: 'description', content: '' },
      { name: 'format-detection', content: 'telephone=no' }
    ],
    link: [
      { rel: 'icon', type: 'image/x-icon', href: '/favicon.ico' }
    ]
  },

  // Global CSS: https://go.nuxtjs.dev/config-css
  css: [
  ],

  // Plugins to run before rendering page: https://go.nuxtjs.dev/config-plugins
  plugins: [
  ],

  // Auto import components: https://go.nuxtjs.dev/config-components
  components: true,

  // Modules for dev and build (recommended): https://go.nuxtjs.dev/config-modules
  buildModules: [
    // https://go.nuxtjs.dev/typescript
    '@nuxt/typescript-build',
    // https://go.nuxtjs.dev/vuetify
    '@nuxtjs/vuetify',
  ],

  // Modules: https://go.nuxtjs.dev/config-modules
  modules: [
  ],

  // Vuetify module configuration: https://go.nuxtjs.dev/config-vuetify
  vuetify: {
    customVariables: ['~/assets/variables.scss'],
    options: {
      customProperties: true
    },
    theme: {
      dark: false,
      themes: {
        light: {
          primarySurface: '#537B87',
          hoverPrimarySurface: '#3E6474',
          onPrimarySurface: '#294D61',
          secondarySurface: '#7EA0A9',
          hoverSecondarySurface: '#7095AB',
          onSecondarySurface: '#618AAD',
          accentSurface: '#C78750',
          backgroundSurface: '#D7DFE7',
          errorSurface: '#EB5050',

          primaryText: '#FFFFFF',
          onPrimaryText: '#BBBBBB',
          secondaryText: '#000000',
          hoverSecondaryText: '#3D3D3D',
          onSecondaryText: '#FFFFFF',
          accentText: '#000000',
          errorText: '#FFFFFF',

          outline: '#424242'
        },
        dark:{}//TODO
      }
    }
  },

  // Build Configuration: https://go.nuxtjs.dev/config-build
  build: {
  }
}
