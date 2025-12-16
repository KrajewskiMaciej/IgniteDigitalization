import './assets/main.css'
import '@/assets/fonts/fonts.css'
import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import Toast from 'vue-toastification'
import 'vue-toastification/dist/index.css'
import App from './App.vue'
import router from './router'
import { i18n } from './plugins/i18n/index'
import { primevue } from '@/plugins/primevue'
import ConfirmationService from 'primevue/confirmationservice'
import '@vue-flow/core/dist/style.css'
import '@vue-flow/core/dist/theme-default.css'
import '/node_modules/flag-icons/css/flag-icons.min.css'

const app = createApp(App)

app.use(createPinia())
app.use(router)
const options = {
  timeout: 3000,
}
app.use(Toast, options)
app.component('font-awesome-icon', FontAwesomeIcon)
app.use(i18n)
app.use(primevue)
app.use(ConfirmationService)

app.mount('#app')
