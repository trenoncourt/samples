<template>
  <div class="card modal-card-border" style="width: 60vw;">
    <header class="bandeau-modal card-header">
    </header>
    <div class="card-content">
      <section class="has-text-centered">
        <p class="modal-title has-text-centered">
          Pièces jointes
        </p>
        <uploader ref="upload" post-action="" @added="fileAdded" @removed="fileRemoved"
                  style="padding: 20px;">
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
        <table v-if="files.length" class="table is-mt-20">
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
              <b-select v-model="file.groupType" placeholder="Select a name">
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
                  @click.native="$refs.upload.remove(file)"
                  style="cursor: pointer;"
                  icon="delete"
                  size="is-medium"
                  type="is-danger">
              </b-icon>
            </td>
          </tr>
          </tbody>
        </table>
      </section>
      <br>
      <footer>
        <div class="has-text-centered">
          <button
              class="button btn-modal-annuler is-cancel has-text-centered button-modal"
              @click="$parent.close()">
            Annuler
          </button>
          <button class="button is-addOrEdit has-text-centered button-modal"
                  @click="ajouter">
            Ajouter
          </button>
        </div>
      </footer>
    </div>
  </div>
</template>

<script>
/* eslint-disable */

import Uploader from './Uploader.vue';

export default {
  name: 'ajout-piece-jointe-client',
  data() {
    return {
      files: [],
      types: [{id: 1, label: 'Divers'}],
    };
  },
  components: {Uploader},
  methods: {
    ajouter() {
      this.$emit('filesAdded', this.files);
      this.$parent.close();
    },
    fileAdded(file) {
      this.files.push(file);
    },
    fileRemoved(file) {
      this.files = this.files.filter(x => x.id !== file.id);
    },
  },
};
</script>

<style scoped>
  .is-mt-20 {
    margin-top: 20px;
  }

  .is-bold {
    font-weight: bold;
  }
</style>
