<script>
export default {
  name: 'ShareIcon',
}
</script>

<template>
  <svg class="share" width="18" height="16" viewBox="0 0 18 16" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path class="share-path" d="M17 8L10.6 1V4.5C7.4 4.5 1 6.6 1 15C1 13.833 2.92 11.5 10.6 11.5V15L17 8Z"
      stroke="white" stroke-opacity="0.6" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" />
  </svg>
</template>


<style scoped>
.share {
  transition: stroke 0.3s ease, stroke-opacity 0.3s ease;
}

.share::after {
  content: '';
  width: 100%;
  height: 100%;
  top: 0;
  position: absolute;
  background: inherit;
  filter: blur(20px);
  z-index: 2;
}

.share:hover .share-path {
  stroke: #71DEFF;
  stroke-opacity: 0.8;
  /* свечение */
  animation: glow 0.5s infinite alternate;
  z-index: 3;
}
</style>