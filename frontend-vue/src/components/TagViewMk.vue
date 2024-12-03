<script>
export default {
  name: "TagView",
  props: {
    text: {
      type: String,
      required: true,
    },
    ondelete: {
      type: Function,
      required: false,
    },
  },
};
</script>

<template>
  <span class="tag">
    <p>
      {{ text }}
    </p>
    <svg v-if="ondelete" @click="ondelete()" width="8" height="8" viewBox="0 0 11 11" fill="none"
      xmlns="http://www.w3.org/2000/svg">
      <path fill-rule="evenodd" clip-rule="evenodd"
        d="M0.292895 9.2929C-0.0976292 9.68342 -0.0976292 10.3166 0.292895 10.7071C0.68342 11.0976 1.31658 11.0976 1.70711 10.7071L5.5 6.91422L9.2929 10.7071C9.68342 11.0976 10.3166 11.0976 10.7071 10.7071C11.0976 10.3166 11.0976 9.68342 10.7071 9.2929L6.91421 5.5L10.7071 1.70711C11.0976 1.31658 11.0976 0.683418 10.7071 0.292893C10.3166 -0.0976309 9.68342 -0.0976311 9.29289 0.292894L5.5 4.08579L1.70711 0.292895C1.31658 -0.0976294 0.683419 -0.0976294 0.292895 0.292895C-0.0976293 0.683419 -0.0976293 1.31658 0.292895 1.70711L4.08579 5.5L0.292895 9.2929Z"
        fill="white" />
    </svg>
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
  padding: 6px 10px;
  background-color: rgba(255, 255, 255, 0.12);
  border-radius: 10px;
  backdrop-filter: blur(10px);
  border: 0.5px solid white;

  svg:hover {
    cursor: pointer;
  }

}
</style>