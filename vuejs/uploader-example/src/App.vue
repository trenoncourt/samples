<template>
  <div id="app">
    <img alt="Vue logo" src="./assets/logo.png">
    <h1>Uploader Example</h1>
    <uploader-example />
  </div>
</template>

<script>
import UploaderExample from './components/UploaderExample.vue'

export default {
  name: 'app',
  components: {
    UploaderExample
  }
}
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
</style>
