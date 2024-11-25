<script>
export default {
  name: "Filter",
  data() {
    return {
      selectedOptions: '',
      selected_ui: '',
      selected_style: '',
      selected_color: '',
      selected_order: '',
      ui: [
        { value: 'option1', text: 'Opcja 1' },
        { value: 'option2', text: 'Opcja 2' },
        { value: 'option3', text: 'Opcja 3' }
      ],
      style: [
        { value: 'option1', text: 'Opcja 1' },
        { value: 'option2', text: 'Opcja 2' },
        { value: 'option3', text: 'Opcja 3' }
      ],
      color: [
        { value: 'option1', text: 'Opcja 1' },
        { value: 'option2', text: 'Opcja 2' },
        { value: 'option3', text: 'Opcja 3' }
      ],
      order: [
        { value: 'option1', text: 'Opcja 1' },
        { value: 'option2', text: 'Opcja 2' },
        { value: 'option3', text: 'Opcja 3' }
      ],
    };
  }
};
</script>

<template>
  <div class="filter-wrapper">
    <div class="filter">
      <h3>Filtruj</h3>
      <select id="filter-ui" v-model="selected_ui">
        <option disabled value="">Wybierz UI</option>
        <option v-for="option in ui" :key="option.value" :value="option.value">
          {{ option.text }}
        </option>
      </select>
      <select id="filter-style" v-model="selected_style">
        <option disabled value="">Wybierz styl</option>
        <option v-for="option in style" :key="option.value" :value="option.value">
          {{ option.text }}
        </option>
      </select>
      <select id="filter-color" v-model="selected_color">
        <option disabled value="">Wybierz kolor</option>
        <option v-for="option in color" :key="option.value" :value="option.value">
          {{ option.text }}
        </option>
      </select>
      <select id="filter-order" v-model="selected_order">
        <option disabled value="">Wybierz kolejność</option>
        <option v-for="option in order" :key="option.value" :value="option.value">
          {{ option.text }}
        </option>
      </select>
    </div>
  </div>
</template>

<style scoped>
.filter {
  padding: 15px 20px;
  display: flex;
  flex-direction: column;
  gap: 15px;
  background-color: rgba(255, 255, 255, 0.15);
  border-radius: 10px;
  border: 0.5px solid white;
}
</style>