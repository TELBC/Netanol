export default defineNuxtConfig({
  devtools: {enabled: false},
  head: {
    titleTemplate: '%s - Astrapia',
    title: 'Astrapia',
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

  // Auto import components
  components: true,

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
        dark: {} // TODO
      }
    }
  },
  build: {transpile: ['@fortawesome/vue-fontawesome']},
  css: [
    '@fortawesome/fontawesome-svg-core/styles.css',
    'v-network-graph/lib/style.css'
  ]
})
