<template>
  <div>
    <uploader post-action="" @added="fileAdded" @removed="fileRemoved" style="padding: 20px;">
      <div class="columns">
        <div class="column">
          <b-icon
              pack="fas"
              icon="copy"
              size="is-large"
              type="is-info">
          </b-icon>
        </div>
      </div>
      <div class="columns">
        <div class="column is-12 is-paddingless is-size-5 is-bold">
          <span class="is-bold">Ajouter vos pièces jointes</span>
        </div>
      </div>
      <div class="columns">
        <div class="column is-12 is-paddingless  is-size-6">
          Au format (PDF, JPG) 10 Mo maximum
        </div>
      </div>

    </uploader>


    <table class="table">
      <thead>
      <tr>
        <th>Nom du fichier</th>
        <th>Type</th>
        <th>Description</th>
        <th></th>
      </tr>
      </thead>
      <tbody>
      <tr v-for="file in files" :key="file.id">
        <td>{{file.name}}</td>
        <td>
          <b-select placeholder="Select a name">
            <option
                v-for="type in types"
                :value="type.id"
                :key="type.id">
              {{ type.label }}
            </option>
          </b-select>
        </td>
        <td>
          <b-input v-model="file.description"></b-input>
        </td>
        <td>
          <b-icon
              style="cursor: pointer;"
              icon="delete"
              size="is-medium"
              type="is-danger">
          </b-icon>
        </td>
      </tr>
      </tbody>
    </table>
    <!--dqs-->
    <!--<ul v-if="files.length">-->
    <!--<li v-for="file in files" :key="file.id">-->
    <!--<span>{{file.name}}</span> - -->
    <!--<span>{{file.size | formatSize}}</span> - -->
    <!--<span v-if="file.error">{{file.error}}</span>-->
    <!--<span v-else-if="file.success">success</span>-->
    <!--<span v-else-if="file.active">active</span>-->
    <!--<span v-else-if="file.active">active</span>-->
    <!--<span v-else></span>-->
    <!--</li>-->
    <!--</ul>-->
    <!--<ul v-else>-->
    <!--<h6>Drop files anywhere to upload<br/>or</h6>-->
    <!--</ul>-->
  </div>
</template>

<script>
import Uploader from './Uploader.vue';

export default {
  name: 'UploaderExample',
  components: {Uploader},
  props: {},
  data() {
    return {
      files: [],
      types: [{id: 1, label: 'Divers'}],
    };
  },
  methods: {
    fileAdded(file) {
      this.files.push(file);
    },
    fileRemoved(file) {
      this.files = this.files.filter(x => x.id !== file.id);
    },
  },
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
  .is-mt-10 {
    margin-top: 10px;
  }

  .is-bold {
    font-weight: bold;
  }
</style>
