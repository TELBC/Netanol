<template>
  <div :style="cssProps">hello</div>
</template>

<style scoped>

</style>

<script>
export default {
  name: 'InspirePage',
  computed: {
    cssProps() {
      return {
        'background': this.$vuetify.theme.themes.light.primarySurface
      };
    }
  }

}
</script>
