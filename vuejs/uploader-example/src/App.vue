<template>
  <div id="app">
    <uploader-example/>
  </div>

</template>

<script>
import UploaderExample from './components/UploaderExample.vue';

export default {
  name: 'app',
  components: {
    UploaderExample,
  },
};
</script>

<style>
  #app {
    font-family: 'Avenir', Helvetica, Arial, sans-serif;
    -webkit-font-smoothing: antialiased;
    -moz-osx-font-smoothing: grayscale;
    text-align: center;
    color: #2c3e50;
    margin-top: 60px;
  }

  .upload-box {
    position: relative;
    padding: 1px;
    margin: 5px;
    width: 100%;
    height: 100%;
    border: 2px dashed darkgrey;
  }

  .is-cursor-pointer {
    cursor: pointer;
  }
</style>
