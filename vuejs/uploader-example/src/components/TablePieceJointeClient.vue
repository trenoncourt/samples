<template>
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
      <td>
        <img v-if="file.thumb" :src="file.thumb" width="40" height="auto"/>
        {{file.name}}
        <p>{{file.size | formatSize}}</p>
      </td>
      <td>
        {{ file.groupType }}
      </td>
      <td>
        {{file.description}}
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
</template>

<script>
export default {
  name: 'table-piece-jointe-client',
  props: {
    files: Array,
  },
};
</script>
