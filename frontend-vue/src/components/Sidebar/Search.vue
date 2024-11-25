<script>
import Lupa from '@/assets/Icons/Lupa.vue';

export default {
  name: "Search",
  components: {
    Lupa
  },
};
</script>

<template>
  <div class="search-container">
    <input type="text" class="search" placeholder="Szukaj" />
    <Lupa class="search-icon" />
  </div>
</template>

<style scoped>
.search-container {
  position: relative;
}

.search {
  margin: 0;
  padding: 10px 20px;
  border: 0.5px solid white;
  border-radius: 10px;
  width: fit-content;
  box-shadow: none;
  font-weight: 600;
  font-size: 14px;
  width: 310px;
  background-color: rgba(255, 255, 255, 0.15);
}

.search-icon {
  position: absolute;
  right: 20px;
  top: 30%;
}
</style>