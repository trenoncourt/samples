<template>
  <div>
    
<!--
        <div v-show="$refs.upload && $refs.upload.dropActive">
            <h3>Drop files to upload</h3>
        </div>-->

        <file-upload
            class="upload-box"
            :post-action="postAction"
            :multiple="true"
            :drop="true"
            :drop-directory="true"
            v-model="files"
            @input-file="inputFile"
            ref="upload">
            
            <slot></slot>

        </file-upload>

<!--
        <button type="button" v-if="!$refs.upload || !$refs.upload.active" @click.prevent="$refs.upload.active = true">
            Start Upload
        </button>
        <button type="button" v-else @click.prevent="$refs.upload.active = false">
            Stop Upload
        </button>-->

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
  data() {
    return {
      files: []
    }
  },
  methods: {
    inputFile(newFile, oldFile) {
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
    background: #eee;
    padding: 1px;
    width: 100%;
    height: 100%;
    border: 2px dashed #00ADCE;
}
</style>