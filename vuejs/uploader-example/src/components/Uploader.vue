<template>
  <div class="upload-box">
    <label for="uploader">
      <slot>
      </slot>
    </label>
    <file-upload
        :post-action="postAction"
        :multiple="true"
        name="uploader"
        :drop="true"
        :drop-directory="true"
        v-model="files"
        @input-file="inputFile"
        ref="upload">

    </file-upload>
  </div>
</template>

<script>
import FileUpload from 'vue-upload-component'

export default {
  props: {
    postAction: String
  },
  components: {
    FileUpload
  },
  data () {
    return {
      files: []
    }
  },
  methods: {
    inputFile (newFile, oldFile) {
      if (newFile && !oldFile) {
        this.$emit('added', newFile)
      }

      if (!newFile && oldFile) {
        this.$emit('removed', oldFile)
      }
    }
  }
}
</script>

<style scoped>
  .upload-box {
    position: relative;
    padding: 1px;
    margin: 5px;
    width: 100%;
    height: 100%;
    border: 2px dashed darkgrey;
  }
</style>
