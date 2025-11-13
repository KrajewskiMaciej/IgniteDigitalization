import PrimeVue from 'primevue/config';
import Ripple from 'primevue/ripple';
import Tooltip from 'primevue/tooltip';
import type { Plugin } from 'vue';
// @ts-ignore
import Wind from '@/assets/presets/wind';

export const primevue: Plugin = {
  install(app) {
    app.use(PrimeVue, {
      unstyled: true,
      ripple: true,
      pt: Wind,
    });


    app.directive('ripple', Ripple);
    app.directive('tooltip', Tooltip);
  }
};
