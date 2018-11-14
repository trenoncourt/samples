<template>
  <div>
    <uploader post-action="" @added="fileAdded" @removed="fileRemoved">
        <ul v-if="files.length">
            <li v-for="file in files" :key="file.id">
            <span>{{file.name}}</span> -
            <span>{{file.size | formatSize}}</span> -
            <span v-if="file.error">{{file.error}}</span>
            <span v-else-if="file.success">success</span>
            <span v-else-if="file.active">active</span>
            <span v-else-if="file.active">active</span>
            <span v-else></span>
            </li>
        </ul>
        <ul v-else>
            <h6>Drop files anywhere to upload<br/>or</h6>
        </ul>
    </uploader>
  </div>
</template>

<script>
import Uploader from './Uploader.vue'

export default {
  name: 'UploaderExample',
  components: {Uploader},
  props: {},
  data () {
    return {
      files: []
    }
  },
  methods: {
      fileAdded (file) {
        this.files.push(file)
      },
      fileRemoved (file) {
        this.files = this.files.filter(x => x.id !== file.id)
      }
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>

</style>
