import './assets/main.css' // <-- NAJWAŻNIEJSZY IMPORT STYLÓW
import '@/assets/fonts/fonts.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
    import Toast from 'vue-toastification'
    import 'vue-toastification/dist/index.css' 

import App from './App.vue'
import router from './router'

const app = createApp(App)

app.use(createPinia())
app.use(router)
    const options = {
        timeout: 3000,
    };
    app.use(Toast, options);
app.component('font-awesome-icon', FontAwesomeIcon);

app.mount('#app')