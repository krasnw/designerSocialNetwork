<script>
export default {
  name: 'LikeIcon',
  data() {
    return {
      isLiked: false
    }
  },
  methods: {
    toggleLike() {
      this.isLiked = !this.isLiked;
    }
  }
}
</script>

<template>
  <svg @click="toggleLike" width="22" height="20" viewBox="0 0 22 20" fill="none" xmlns="http://www.w3.org/2000/svg"
    :class="{ 'liked': isLiked }">
    <path id="Like"
      d="M6.5 1C3.4625 1 1 3.4625 1 6.5C1 12 7.5 17 11 18.163C14.5 17 21 12 21 6.5C21 3.4625 18.5375 1 15.5 1C13.64 1 11.995 1.9235 11 3.337C10.4928 2.6146 9.81908 2.02505 9.03577 1.61824C8.25245 1.21144 7.38265 0.999377 6.5 1Z"
      :stroke="isLiked ? 'red' : 'white'" :stroke-opacity="isLiked ? 0.8 : 0.6" :fill="isLiked ? 'red' : 'white'"
      :fill-opacity="isLiked ? 0.6 : 0.2" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
      class="like-icon" />
  </svg>
</template>

<style scoped>
.like-icon {
  transition: stroke 0.3s ease, stroke-opacity 0.3s ease, fill 0.3s ease, fill-opacity 0.3s ease, transform 0.2s ease;
}

.liked {
  animation: scaleAnimation 0.3s ease;
}
</style>