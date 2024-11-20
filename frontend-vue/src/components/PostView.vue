<script>
export default {
  name: 'PostView',
  props: {
    username: {
      type: String,
      required: true
    },
    userProfilePicture: {
      type: String,
      required: true
    },

    postPictures: {
      type: Array,
      required: true
    },

    title: {
      type: String,
      required: true
    },
    description: {
      type: String,
      required: true
    },
    tags: {
      type: Array,
      default: []
    },

    shares: {
      type: Number,
      default: 0
    },
    likes: {
      type: Number,
      default: 0
    },
  },
  data() {

  },
  methods: {

  }
}
</script>

<template>
  <article class="post">
    <span class="picture-slider">
      <img v-for="(picture, index) in postPictures" :key="index" :src="picture" alt="Post Picture">
    </span>
  </article>
</template>

<style scoped></style>