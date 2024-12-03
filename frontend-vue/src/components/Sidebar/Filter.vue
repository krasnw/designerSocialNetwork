<script>
import ReloadIcon from '@/assets/Icons/ReloadIcon.vue';
import TagView from '../TagView.vue';

export default {
  name: "Filter",
  components: {
    TagView,
    ReloadIcon,
  },
  data() {
    return {
      selectedOptions: [],
      selected_ui: [],
      selected_style: [],
      selected_color: [],
      selected_order: '',
      ui: [
        { value: 'przycisk', text: 'Przycisk' },
        { value: 'lista', text: 'Lista rozwijana' },
        { value: 'slider', text: 'Slider' },
        { value: 'input', text: 'Input' },
        { value: 'checkbox', text: 'Checkbox' },
        { value: 'radio', text: 'Radio' },
        { value: 'toggle', text: 'Toggle' },
        { value: 'loader', text: 'Loader' },
        { value: 'calendar', text: 'Calendar' },
        { value: 'menu', text: 'Menu' },
        { value: 'sidebar', text: 'Sidebar' },
        { value: 'navbar', text: 'Navbar' },
        { value: 'footer', text: 'Footer' },
        { value: 'form', text: 'Form' },
        { value: 'typography', text: 'Typography' },
        { value: 'animation', text: 'Animation' },
      ],
      style: [
        { value: 'glassmorphism', text: 'Glassmorphism' },
        { value: 'neumorphism', text: 'Neumorphism' },
        { value: 'minimalism', text: 'Minimalism' },
        { value: 'material', text: 'Material' },
        { value: 'flat', text: 'Flat' },
        { value: '3d', text: '3D' },
        { value: 'retro', text: 'Retro' },
      ],
      color: [
        { value: 'light', text: 'Light' },
        { value: 'dark', text: 'Dark' },
        { value: 'colorful', text: 'Colorful' }
      ],
      order: [
        { value: 'popularne', text: 'Popularne' },
        { value: 'najnowsze', text: 'Najnowsze' },
        { value: 'polubione', text: 'Polubione' }
      ],
      activeDropdown: null,
      activeToggle: 'public',
      isLoading: false,
      isTagNumChanged: false,
      isSpinning: false,
    };
  },
  mounted() {
    // Добавляем слушатель при монтировании компонента
    window.addEventListener('click', this.handleClickOutside);
  },
  beforeDestroy() {
    // Удаляем слушатель при уничтожении компонента
    window.removeEventListener('click', this.handleClickOutside);
  },
  watch: {
    selected_ui(newVal) {
      this.updateSelectedOptions('UI', newVal);
    },
    selected_style(newVal) {
      this.updateSelectedOptions('Styl', newVal);
    },
    selected_color(newVal) {
      this.updateSelectedOptions('Kolor', newVal);
    },
    selected_order(newVal) {
      if (newVal) {
        this.selectedOptions = this.selectedOptions.filter(opt => !opt.startsWith('Kolejność:'));
        this.selectedOptions.push(`Kolejność: ${newVal}`);
      }
    },
    selectedOptions: {
      handler(newVal, oldVal) {
        if (newVal.length !== oldVal.length) {
          this.isTagNumChanged = true;
        }
      },
      deep: true
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
    toggleDropdown(id, event) {
      event.stopPropagation();
      this.activeDropdown = this.activeDropdown === id ? null : id;
    },
    handleClickOutside(event) {
      if (this.activeDropdown) {
        const dropdownEl = this.$refs[this.activeDropdown];
        if (dropdownEl && !dropdownEl.contains(event.target)) {
          this.activeDropdown = null;
        }
      }
    },
    removeTag(tagText) {
      const [type, value] = tagText.split(': ');

      switch (type) {
        case 'UI':
          this.selected_ui = this.selected_ui.filter(item => item !== value);
          break;
        case 'Styl':
          this.selected_style = this.selected_style.filter(item => item !== value);
          break;
        case 'Kolor':
          this.selected_color = this.selected_color.filter(item => item !== value);
          break;
        case 'Kolejność':
          this.selected_order = '';
          break;
      }

      this.selectedOptions = this.selectedOptions.filter(opt => opt !== tagText);
    },
    toggleOption(option) {
      this.activeToggle = option;
    },
    toggleCheckbox(event, id) {
      // Добавляем проверку, что клик не по label и не по самому checkbox
      if (event.target.tagName !== 'LABEL' && event.target.type !== 'checkbox' && event.target.type !== 'radio') {
        const checkbox = document.getElementById(id);
        if (checkbox) {
          checkbox.checked = !checkbox.checked;
          checkbox.dispatchEvent(new Event('change', { bubbles: true }));
        }
      }
    },
    handleReload() {
      this.isSpinning = true;
      setTimeout(() => {
        this.isSpinning = false;
        this.isTagNumChanged = false;
      }, 3000);
    },
  }
};
</script>

<template>
  <div class="filter-wrapper no-select">
    <div class="filter">
      <div class="filter-header">
        <h3>Filtruj</h3>
        <ReloadIcon v-if="isTagNumChanged" @click="handleReload" :class="{ 'spin-animation': isSpinning }"
          class="reload-icon" />
      </div>

      <div class="toggle-type">
        <div class="toggle-option" :class="{ active: activeToggle === 'public' }" @click="toggleOption('public')">
          Publiczne
        </div>
        <div class="toggle-option" :class="{ active: activeToggle === 'purchased' }" @click="toggleOption('purchased')">
          Płatne
        </div>
      </div>

      <div class="dropdown-check-list" :class="{ visible: activeDropdown === 'ui' }" ref="ui">
        <span class="anchor" @click="toggleDropdown('ui', $event)">Element UI</span>
        <ul class="items">
          <li v-for="option in ui" :key="option.value" @click="toggleCheckbox($event, 'ui-' + option.value)">
            <input type="checkbox" :id="'ui-' + option.value" :value="option.value" v-model="selected_ui">
            <label :for="'ui-' + option.value">{{ option.text }}</label>
          </li>
        </ul>
      </div>

      <div class="dropdown-check-list" :class="{ visible: activeDropdown === 'style' }" ref="style">
        <span class="anchor" @click="toggleDropdown('style', $event)">Styl</span>
        <ul class="items">
          <li v-for="option in style" :key="option.value" @click="toggleCheckbox($event, 'style-' + option.value)">
            <input type="checkbox" :id="'style-' + option.value" :value="option.value" v-model="selected_style">
            <label :for="'style-' + option.value">{{ option.text }}</label>
          </li>
        </ul>
      </div>

      <div class="dropdown-check-list" :class="{ visible: activeDropdown === 'color' }" ref="color">
        <span class="anchor" @click="toggleDropdown('color', $event)">Kolor</span>
        <ul class="items">
          <li v-for="option in color" :key="option.value" @click="toggleCheckbox($event, 'color-' + option.value)">
            <input type="checkbox" :id="'color-' + option.value" :value="option.value" v-model="selected_color">
            <label :for="'color-' + option.value">{{ option.text }}</label>
          </li>
        </ul>
      </div>

      <div class="dropdown-check-list" :class="{ visible: activeDropdown === 'order' }" ref="order">
        <span class="anchor" @click="toggleDropdown('order', $event)">Kolejność</span>
        <ul class="items">
          <li v-for="option in order" :key="option.value" @click="toggleCheckbox($event, 'order-' + option.value)">
            <input type="radio" :id="'order-' + option.value" :value="option.value" v-model="selected_order">
            <label :for="'order-' + option.value">{{ option.text }}</label>
          </li>
        </ul>
      </div>

      <section class="selected-tags">
        <!-- ready tag -->
        <TagView v-for="option in selectedOptions" :key="option" :text="option" @delete="removeTag" />
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
  background: #aaad;
  backdrop-filter: blur(50px);
  box-sizing: border-box;
  z-index: 100;
  max-height: 200px;
  /* Добавляем максимальную высоту */
  overflow-y: auto;
  /* Добавляем вертикальную прокрутку */

  /* Стилизация скроллбара */
  &::-webkit-scrollbar {
    width: 8px;
  }

  &::-webkit-scrollbar-track {
    background: rgba(255, 255, 255, 0.1);
    border-radius: 4px;
    backdrop-filter: blur(5px);
  }

  &::-webkit-scrollbar-thumb {
    background: rgba(255, 255, 255, 0.3);
    border-radius: 4px;
  }

  &::-webkit-scrollbar-thumb:hover {
    background: rgba(255, 255, 255, 0.4);
  }
}

.dropdown-check-list ul.items li {
  list-style: none;
  padding: 10px;
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
  transition: background 0.2s ease;

  input[type="checkbox"],
  input[type="radio"] {
    margin: 0;
  }

  &:hover {
    background: rgba(255, 255, 255, 0.2);
  }

  label {
    cursor: pointer;
  }
}

/* Удалить или закомментировать старый hover стиль */
/* .dropdown-check-list ul.items :hover {
  background: rgba(255, 255, 255, 0.1);
} */

.dropdown-check-list.visible .items {
  display: block;
}

.toggle-type {
  display: flex;
  width: 100%;
  background: rgba(255, 255, 255, 0.15);
  border-radius: 10px;
  border: 1px solid white;
  overflow: hidden;
}

.toggle-option {
  flex: 1;
  padding: 8px 13px;
  margin: 2px;
  text-align: center;
  cursor: pointer;
  color: rgba(255, 255, 255, 0.6);
  font-size: 12px;
  font-weight: 700;
  transition: all 0.3s ease;
}

.toggle-option.active {
  background: rgba(255, 255, 255, 0.1);

  color: white;
  border-radius: 7px;
  box-shadow: inset 0 0 0 1px white;
}

.filter-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.reload-icon {
  cursor: pointer;
  color: white;
}

.spin-animation {
  animation: spin 1s linear infinite;
}
</style>