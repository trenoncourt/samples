<template>
  <div>
    <button @click="isComponentAjouterPieceJointe = true">ok</button>
    <app-table-piece-jointe-client v-if="files.length" :files="files"></app-table-piece-jointe-client>
    <b-modal :active.sync="isComponentAjouterPieceJointe"
             has-modal-card
             class="modal">
      <app-ajout-piece-jointe-client
          @filesAdded="filesAdded"></app-ajout-piece-jointe-client>
    </b-modal>

  </div>
</template>

<script>
/* eslint-disable */

import AjoutPieceJointeClient from './AjoutPieceJointeClient';
import TablePieceJointeClient from './TablePieceJointeClient';

export default {
  name: 'UploaderExample',
  components: {AppAjoutPieceJointeClient: AjoutPieceJointeClient, AppTablePieceJointeClient: TablePieceJointeClient},
  props: {},
  data() {
    return {
      isComponentAjouterPieceJointe: false,
      files: [],
      types: [{id: 1, label: 'Divers'}],
    };
  },
  methods: {
    filesAdded(files) {
      this.files.push(...files.filter(f => !this.files.map(ef => ef.name).includes(f.name)));
      console.log(...this.files);
    },
  },
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->

