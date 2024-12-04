<script>
export default {
  name: "TagView",
  data() {
    return {
      isHovered: false
    }
  },
  props: {
    text: {
      type: String,
      required: true,
    },
  },
  methods: {
    handleDelete() {
      this.$emit('delete', this.text);
    }
  }
};
</script>

<template>
  <span class="tag" @click="handleDelete">
    <p>{{ text }}</p>
  </span>
</template>

<style scoped>
.tag {
  display: flex;
  flex-direction: row;
  align-items: center;
  width: max-content;
  gap: 10px;
  font-size: 12px;
  font-weight: 600;
  padding: 6px 12px;
  background-color: var(--element-light-color);
  border-radius: 10px;
  border: 0.5px solid white;
}

.tag:hover {
  background-color: var(--delete-button-color);
  border: 0.5px solid var(--delete-button-border-color);
  animation: waving 0.2s infinite;
  transition: all 1s;
  cursor: url('/src/assets/Icons/TrashCan.svg'), auto;

  p {
    color: #fdd;
  }
}

@keyframes waving {
  0% {
    transform: rotate(0deg);
  }

  50% {
    transform: rotate(1.5deg);
  }

  100% {
    transform: rotate(0deg);
  }
}
</style>