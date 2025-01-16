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
  border: 0.5px solid white;
  background-color: var(--element-light-color);
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.search-container:focus-within {
  outline: 1px solid white;
}

.search {
  margin: 0;
  padding: 10px 20px;
  width: 100%;
  box-shadow: none;
  border: none;
  background-color: transparent;
  font-weight: 600;
  font-size: 14px;
}

.search:focus {
  outline: none;
}

.search-icon {
  margin: 0 21px;
}
</style>