<template>
  <div>
    <label for="logo">
      <uploader @added="fileAdded" name="logo"
                class="upload-box has-background-light is-cursor-pointer" ref="upload"
                post-action=""
                style="display: table-cell; vertical-align: middle; width: 160px; height: 200px;">
        <img v-if="file" :src="file.blob" alt="">
        <b-icon
            v-else
            style="cursor: pointer"
            icon="plus-circle"
            size="is-large"
            type="is-grey-lighter">
        </b-icon>
      </uploader>
    </label>
  </div>
</template>

<script>
import Uploader from './Uploader.vue';

export default {
  name: 'logo-client',
  components: {Uploader},
  data() {
    return {
      file: null,
    };
  },
  methods: {
    fileAdded(file) {
      this.file = file;
    },
  },
};
</script>
