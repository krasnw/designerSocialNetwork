<script>
import TagView from '../TagView.vue';

export default {
  name: "Filter",
  components: {
    TagView,
  },
  data() {
    return {
      selectedOptions: [],
      selected_ui: [],
      selected_style: [],
      selected_color: [],
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
      activeDropdown: null
    };
  },
  watch: {
    selected_ui(newVal) {
      this.updateSelectedOptions('UI', newVal);
    },
    selected_style(newVal) {
      this.updateSelectedOptions('Style', newVal);
    },
    selected_color(newVal) {
      this.updateSelectedOptions('Color', newVal);
    },
    selected_order(newVal) {
      if (newVal) {
        this.selectedOptions = this.selectedOptions.filter(opt => !opt.startsWith('Order:'));
        this.selectedOptions.push(`Order: ${newVal}`);
      }
    }
  },
  methods: {
    updateSelectedOptions(type, values) {
      // Удаляем старые опции данного типа
      this.selectedOptions = this.selectedOptions.filter(opt => !opt.startsWith(`${type}:`));
      // Добавляем новые опции
      values.forEach(value => {
        this.selectedOptions.push(`${type}: ${value}`);
      });
    },
    toggleDropdown(id) {
      this.activeDropdown = this.activeDropdown === id ? null : id;
    }
  }
};
</script>

<template>
  <div class="filter-wrapper">
    <div class="filter">
      <h3>Filtruj</h3>

      <div class="dropdown-check-list" :class="{ visible: activeDropdown === 'ui' }">
        <span class="anchor" @click="toggleDropdown('ui')">Wybierz UI</span>
        <ul class="items">
          <li v-for="option in ui" :key="option.value">
            <input type="checkbox" :id="'ui-' + option.value" :value="option.value" v-model="selected_ui">
            <label :for="'ui-' + option.value">{{ option.text }}</label>
          </li>
        </ul>
      </div>

      <div class="dropdown-check-list" :class="{ visible: activeDropdown === 'style' }">
        <span class="anchor" @click="toggleDropdown('style')">Wybierz styl</span>
        <ul class="items">
          <li v-for="option in style" :key="option.value">
            <input type="checkbox" :id="'style-' + option.value" :value="option.value" v-model="selected_style">
            <label :for="'style-' + option.value">{{ option.text }}</label>
          </li>
        </ul>
      </div>

      <div class="dropdown-check-list" :class="{ visible: activeDropdown === 'color' }">
        <span class="anchor" @click="toggleDropdown('color')">Wybierz kolor</span>
        <ul class="items">
          <li v-for="option in color" :key="option.value">
            <input type="checkbox" :id="'color-' + option.value" :value="option.value" v-model="selected_color">
            <label :for="'color-' + option.value">{{ option.text }}</label>
          </li>
        </ul>
      </div>

      <div class="dropdown-check-list" :class="{ visible: activeDropdown === 'order' }">
        <span class="anchor" @click="toggleDropdown('order')">Wybierz kolejność</span>
        <ul class="items">
          <li v-for="option in order" :key="option.value">
            <input type="radio" :id="'order-' + option.value" :value="option.value" v-model="selected_order">
            <label :for="'order-' + option.value">{{ option.text }}</label>
          </li>
        </ul>
      </div>

      <section class="selected-tags">
        <TagView v-for="option in selectedOptions" :key="option" :text="option" />
      </section>
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

.selected-tags {
  display: flex;
  flex-wrap: wrap;
  row-gap: 8px;
  column-gap: 8px
}

.dropdown-check-list {
  width: 100%;
  position: relative;
  background: rgba(255, 255, 255, 0.15);
  border-radius: 10px;
  border: 1px solid white;
  font-size: 12px;
  font-weight: 700;
}

.dropdown-check-list .anchor {
  position: relative;
  cursor: pointer;
  display: block;
  padding: 8px 15px;
  border-radius: 10px;
  color: rgba(255, 255, 255, 0.6);
}

.dropdown-check-list .anchor:after {
  position: absolute;
  content: "";
  border-left: 2px solid rgba(255, 255, 255, 0.5);
  border-top: 2px solid rgba(255, 255, 255, 0.5);
  padding: 3px;
  right: 10px;
  top: 35%;
  transform: rotate(-135deg);
  transition: transform 0.4s;
}

.dropdown-check-list.visible .anchor:after {
  transform: rotate(45deg);
  top: 40%;
}

.dropdown-check-list ul.items {
  padding: 5px;
  display: none;
  margin: 0;
  border: 1px solid white;
  border-top: none;
  border-radius: 0 0 10px 10px;
  position: absolute;
  width: 100%;
  background: rgba(0, 0, 0, 0.7);
  backdrop-filter: blur(50px);
  box-sizing: border-box;
  z-index: 100;
}

.dropdown-check-list ul.items li {
  list-style: none;
  padding: 10px;
  display: flex;
  align-items: center;
  gap: 10px;

  input[type="checkbox"],
  input[type="radio"] {
    margin: 0;
  }
}

.dropdown-check-list.visible .items {
  display: block;
}
</style>