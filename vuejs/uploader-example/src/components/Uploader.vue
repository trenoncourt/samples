<template>
  <div>
    <label :for="name">
      <slot>
      </slot>
    </label>
    <file-upload
        :multiple="multiple"
        :name="name"
        :drop="true"
        v-model="files"
        @input-file="inputFile"
        @input-filter="inputFilter"
        ref="upload">
    </file-upload>
  </div>
</template>

<script>
import FileUpload from 'vue-upload-component';
import ImageCompressor from '@xkeshi/image-compressor';

export default {
  props: {
    background: {
      type: String,
      default: '',
    },
    multiple: {
      type: Boolean,
      default: false,
    },
    name: {
      type: String,
      default: 'uploader',
    },
  },
  components: {
    FileUpload,
  },
  data() {
    return {
      files: [],
    };
  },
  methods: {
    inputFilter(newFile, oldFile, prevent) {
      if (newFile && !oldFile) {
        // Before adding a file
        // Filter system files or hide files
        if (/(\/|^)(Thumbs\.db|desktop\.ini|\..+)$/.test(newFile.name)) {
          return prevent();
        }
        // Filter php html js file
        if (/\.(php5?|html?|jsx?)$/i.test(newFile.name)) {
          return prevent();
        }
        // Automatic compression
        if (newFile.file && newFile.type.substr(0, 6) === 'image/' && this.autoCompress > 0 && this.autoCompress <
            newFile.size) {
          newFile.error = 'compressing';
          const imageCompressor = new ImageCompressor(null, {
            convertSize: Infinity,
            maxWidth: 512,
            maxHeight: 512,
          });
          imageCompressor.compress(newFile.file).then((file) => {
            this.$refs.upload.update(newFile, {error: '', file, size: file.size, type: file.type});
          }).catch((err) => {
            this.$refs.upload.update(newFile, {error: err.message || 'compress'});
          });
        }
      }
      if (newFile && (!oldFile || newFile.file !== oldFile.file)) {
        // Create a blob field
        newFile.blob = '';
        let URL = window.URL || window.webkitURL;
        if (URL && URL.createObjectURL) {
          newFile.blob = URL.createObjectURL(newFile.file);
        }
        // Thumbnails
        newFile.thumb = '';
        if (newFile.blob && newFile.type.substr(0, 6) === 'image/') {
          newFile.thumb = newFile.blob;
        }
      }
    },
    inputFile(newFile, oldFile) {
      if (newFile && !oldFile) {
        this.$emit('added', newFile);
      }

      if (!newFile && oldFile) {
        this.$emit('removed', oldFile);
      }
    },
    remove(file) {
      this.$refs.upload.remove(file);
    },
  },
};
</script>

