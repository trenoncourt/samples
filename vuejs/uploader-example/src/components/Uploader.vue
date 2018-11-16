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
    postAction: String,
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
        // 添加文件前
        // Filter system files or hide files
        // 过滤系统文件 和隐藏文件
        if (/(\/|^)(Thumbs\.db|desktop\.ini|\..+)$/.test(newFile.name)) {
          return prevent();
        }
        // Filter php html js file
        // 过滤 php html js 文件
        if (/\.(php5?|html?|jsx?)$/i.test(newFile.name)) {
          return prevent();
        }
        // Automatic compression
        // 自动压缩
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
        // 创建 blob 字段
        newFile.blob = '';
        let URL = window.URL || window.webkitURL;
        if (URL && URL.createObjectURL) {
          newFile.blob = URL.createObjectURL(newFile.file);
        }
        // Thumbnails
        // 缩略图
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
