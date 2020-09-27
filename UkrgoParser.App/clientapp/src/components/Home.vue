<template>
  <div>
    <div class="toolbar">
      <md-button @click="process()" class="md-raised md-primary">Обновить</md-button>
      <md-button @click="savePhoneNumbers()" class="md-raised md-accent">Сохранить</md-button>
    </div>
    <md-table v-if="phoneNumbers && phoneNumbers.length > 0">
      <md-table-row>
        <md-table-head>Действия</md-table-head>
        <md-table-head>Номер</md-table-head>
        <md-table-head>Ссылка</md-table-head>
      </md-table-row>
      <md-table-row v-for="phoneNumber of phoneNumbers" :key="phoneNumber.number">
        <md-table-cell class="toolbar">
          <md-button v-clipboard="phoneNumber.number" class="md-dense md-raised md-primary">Копировать</md-button>
          <md-button @click="addPhoneNumber(phoneNumber.number)" class="md-dense md-raised md-accent">Добавить в ЧС</md-button>
        </md-table-cell>
        <md-table-cell>{{phoneNumber.number}}</md-table-cell>
        <md-table-cell>
          <a :href="phoneNumber.postLink" target="_blank">{{phoneNumber.postLink.replace('http://kharkov.ukrgo.com/', '')}}</a>
        </md-table-cell>
      </md-table-row>
    </md-table>
    <md-content v-if="error" class="md-accent">
      {{error.message}}
    </md-content>
  </div>
</template>

<script>
import axios from 'axios';

export default {
  name: 'Home',
  data() {
    return {
      phoneNumbers: [],
      error: null
    }
  },
  created() {
    if (localStorage.getItem('phoneNumbers')) {
      try {
        this.phoneNumbers = JSON.parse(localStorage.getItem('phoneNumbers'));
      } catch(e) {
        localStorage.removeItem('phoneNumbers');
      }
    }
  },
  methods: {
    async process() {
      this.phoneNumbers = [];
      const postLinks = await this.getPostLinks();
      for (const postLink of postLinks) {
        await this.sleep(300);
        const phoneNumber = await this.getPhoneNumber(postLink);
        if (!phoneNumber) { continue; }
        const validNumber = await this.checkNumber(phoneNumber);
        if (validNumber && !this.phoneNumbers.some(p => p.number === phoneNumber)) {
          this.phoneNumbers.push({ number: phoneNumber, postLink });
        }
      }
    },
    savePhoneNumbers() {
      const parsedPhoneNumbers = JSON.stringify(this.phoneNumbers);
      localStorage.setItem('phoneNumbers', parsedPhoneNumbers);
    },
    async addPhoneNumber(phoneNumber) {
      await this.addNumber(phoneNumber);
      const index = this.phoneNumbers.findIndex(p => p.number === phoneNumber);
      this.phoneNumbers.splice(index, 1);
    },
    sleep(ms) {
      return new Promise(resolve => setTimeout(resolve, ms));
    },
    async getPostLinks() {
      const response = await axios.get(`api/browser/GetPostLinks`, {
        params: {
          url: 'http://kharkov.ukrgo.com/view_subsection.php?id_subsection=146'
        }
      });
      return response.data;
    },
    async getPhoneNumber(postLink) {
      const response = await axios.get(`api/browser/GetPhoneNumber`, {
        params: {
          postLink
        }
      });
      return response.data;
    },
    async checkNumber(phoneNumber) {
      const response = await axios.get(`api/phone/CheckNumber`, {
        params: {
          phoneNumber
        }
      });
      return response.data;
    },
    async addNumber(phoneNumber) {
      await axios.post(`api/phone/AddNumber`, {
        phoneNumber: phoneNumber.toString()
      });
    }
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
h3 {
  margin: 40px 0 0;
}
a {
  color: #42b983;
}
.toolbar {
  white-space: nowrap;
}
.toolbar button {
  margin-right: 5px;
}
</style>
