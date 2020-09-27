import Vue from 'vue';

import Clipboard from 'v-clipboard';
import { MdButton, MdTable, MdContent } from 'vue-material/dist/components';
import 'vue-material/dist/vue-material.min.css';
import 'vue-material/dist/theme/default.css';

import App from './App.vue';

Vue.config.productionTip = false;

Vue.use(Clipboard);
Vue.use(MdButton);
Vue.use(MdTable);
Vue.use(MdContent);

new Vue({
  render: h => h(App),
}).$mount('#app');
