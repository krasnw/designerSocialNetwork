<script>
import ReloadIcon from '@/assets/Icons/ReloadIcon.vue';
import TagView from '../TagView.vue';
import { filterTagService } from '@/services/filterTag';


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
      ui: [],
      style: [],
      color: [],
      activeDropdown: null,
      activeToggle: 'public',
      isLoading: false,
      isTagNumChanged: false,
      isSpinning: false,
    };
  },
  async created() {
    try {
      const tags = await filterTagService.getTags();

      this.ui = tags
        .filter(tag => tag.tagType === 'UI_ELEMENT')
        .map(tag => ({ value: tag.name, text: tag.name }));

      this.style = tags
        .filter(tag => tag.tagType === 'STYLE')
        .map(tag => ({ value: tag.name, text: tag.name }));

      this.color = tags
        .filter(tag => tag.tagType === 'COLOR')
        .map(tag => ({ value: tag.name, text: tag.name }));

      // Load saved filters from SessionStorage
      this.loadFiltersFromStorage();
    } catch (error) {
      console.error('Failed to load tags:', error);
    }
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
    activeToggle(newVal) {
      this.saveFiltersToStorage();
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
    // Add new methods
    saveFiltersToStorage() {
      const filters = {
        selected_ui: this.selected_ui,
        selected_style: this.selected_style,
        selected_color: this.selected_color,
        activeToggle: this.activeToggle
      };
      sessionStorage.setItem('selectedFilters', JSON.stringify(filters));
    },

    loadFiltersFromStorage() {
      const savedFilters = sessionStorage.getItem('selectedFilters');
      if (savedFilters) {
        const filters = JSON.parse(savedFilters);
        this.selected_ui = filters.selected_ui || [];
        this.selected_style = filters.selected_style || [];
        this.selected_color = filters.selected_color || [];
        this.activeToggle = filters.activeToggle || 'public';

        this.updateSelectedOptions('UI', this.selected_ui);
        this.updateSelectedOptions('Styl', this.selected_style);
        this.updateSelectedOptions('Kolor', this.selected_color);
      }
    },

    removeTagFromStorage(tagText) {
      const [type, value] = tagText.split(': ');
      const savedFilters = JSON.parse(sessionStorage.getItem('selectedFilters') || '{}');

      switch (type) {
        case 'UI':
          this.selected_ui = this.selected_ui.filter(item => item !== value);
          savedFilters.selected_ui = (savedFilters.selected_ui || []).filter(item => item !== value);
          break;
        case 'Styl':
          this.selected_style = this.selected_style.filter(item => item !== value);
          savedFilters.selected_style = (savedFilters.selected_style || []).filter(item => item !== value);
          break;
        case 'Kolor':
          this.selected_color = this.selected_color.filter(item => item !== value);
          savedFilters.selected_color = (savedFilters.selected_color || []).filter(item => item !== value);
          break;
      }

      this.selectedOptions = this.selectedOptions.filter(opt => opt !== tagText);
      sessionStorage.setItem('selectedFilters', JSON.stringify(savedFilters));
    },

    updateSelectedOptions(type, values) {
      this.selectedOptions = this.selectedOptions.filter(opt => !opt.startsWith(`${type}:`));
      values.forEach(value => {
        this.selectedOptions.push(`${type}: ${value}`);
      });
      this.saveFiltersToStorage();
    },

    async toggleOption(option) {
      this.activeToggle = option;
      await this.$nextTick();
      await this.handleReload();
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
    toggleCheckbox(event, id) {
      // Добавляем проверку, что клик не по label и не по самому checkbox
      if (event.target.tagName !== 'LABEL' && event.target.type !== 'checkbox') {
        const checkbox = document.getElementById(id);
        if (checkbox) {
          checkbox.checked = !checkbox.checked;
          checkbox.dispatchEvent(new Event('change', { bubbles: true }));
        }
      }
    },
    async handleReload() {
      this.isSpinning = true;
      setTimeout(() => {
        this.isSpinning = false;
        this.isTagNumChanged = false;
      }, 500);
      this.$root.reloadView();
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
        <div class="toggle-option" :class="{ active: activeToggle === 'private' }" @click="toggleOption('private')">
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

      <section class="selected-tags">
        <TagView v-for="option in selectedOptions" :key="option" :text="option" @delete="removeTagFromStorage" />
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
  background-color: var(--element-light-color);
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
  background: var(--element-light-color);
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
  color: var(--placeholder-color);
}

.dropdown-check-list .anchor:after {
  position: absolute;
  content: "";
  border-left: 2px solid var(--placeholder-color);
  border-top: 2px solid var(--placeholder-color);
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
  overflow-y: auto;

  &::-webkit-scrollbar {
    width: 8px;
  }

  &::-webkit-scrollbar-track {
    background: var(--element-light-color);
    border-radius: 4px;
    backdrop-filter: blur(5px);
  }

  &::-webkit-scrollbar-thumb {
    background: var(--element-border-light-color);
    border-radius: 4px;
  }

  &::-webkit-scrollbar-thumb:hover {
    cursor: pointer;
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
    background: var(--element-hover-light-color);
  }

  label {
    cursor: pointer;
  }
}

.dropdown-check-list.visible .items {
  display: block;
}

.toggle-type {
  display: flex;
  width: 100%;
  background: var(--element-light-color);
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
  color: var(--placeholder-color);
  font-size: 12px;
  font-weight: 700;
  transition: all 0.3s ease;
}

.toggle-option.active {
  background: var(--element-light-color);
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
}

.spin-animation {
  animation: spin 1s linear infinite;
}
</style>