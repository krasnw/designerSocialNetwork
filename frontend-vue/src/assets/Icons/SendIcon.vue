<script>
export default {
  name: 'SendIcon',
  data() {
    return {
      animation: false
    }
  },
  methods: {
    sendAnimationTrigger() {
      this.animation = true;
      setTimeout(() => {
        this.animation = false;
      }, 400);
    }
  }
}
</script>

<template>
  <svg @click="sendAnimationTrigger" width="24" height="21" viewBox="0 0 24 21" fill="none"
    xmlns="http://www.w3.org/2000/svg" :class="{ 'send': animation }" class="send-icon">
    <path fill-rule="evenodd" clip-rule="evenodd"
      d="M21.514 0.140265C21.8091 0.0160679 22.1321 -0.026767 22.4493 0.0162186C22.7666 0.0592043 23.0665 0.186439 23.3179 0.38468C23.5693 0.582921 23.763 0.844921 23.8788 1.14341C23.9946 1.4419 24.0282 1.76596 23.9762 2.08188L21.268 18.5091C21.0053 20.0937 19.2667 21.0024 17.8135 20.2131C16.5979 19.5528 14.7924 18.5354 13.1684 17.4739C12.3564 16.9425 9.86912 15.2409 10.1748 14.0301C10.4375 12.9948 14.6169 9.10439 17.0051 6.79142C17.9424 5.8827 17.515 5.35849 16.408 6.19436C13.6592 8.26971 9.2458 11.4257 7.78661 12.3141C6.49937 13.0975 5.82828 13.2312 5.02584 13.0975C3.56187 12.8539 2.20418 12.4765 1.09605 12.0168C-0.401352 11.3959 -0.328512 9.33724 1.09486 8.7378L21.514 0.140265Z"
      fill="url(#paint0_linear_82_1579)" />
    <defs>
      <linearGradient id="paint0_linear_82_1579" x1="0" y1="0" x2="24" y2="21" gradientUnits="userSpaceOnUse">
        <stop offset="0.2" stop-color="#478319" />
        <stop offset="1" stop-color="#05D493" />
      </linearGradient>
    </defs>
  </svg>
</template>

<style scoped>
.send-icon {
  transition: all 0.3s ease;
}

.send-icon:hover {
  cursor: pointer;
  transform: scale(1.1);
  transition: all 0.3s ease;
}

.send {
  animation: scaleAnimation 0.3s ease;
}
</style>