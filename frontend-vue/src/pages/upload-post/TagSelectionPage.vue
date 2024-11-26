<template>
  <div class="tag-sel-page">
    <!-- <h2 class="page-name">Dodaj nowy post</h2>
    <div class="dropdown-section">
      <label v-for="(options, index) in tagOptions" :key="index">
        <select v-model="selectedTags[index]" @change="addTag(index)">
          <option disabled value="">Wybierz...</option>
          <option v-for="option in options" :key="option">{{ option }}</option>
        </select>
      </label>
    </div>

    <div class="tags-display">
      <div v-for="(tag, index) in tags" :key="index" class="tag">
        {{ tag }}
        <span class="remove-icon" @click="removeTag(index)">
          ✖
        </span>
      </div>
    </div> -->

    <div class="left">
      <div class="left-container">
        <h3>Wybierz tagi</h3>
        <div class="filter">

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

          <section class="selected-tags">
            <template v-for="[type, optionObj] in selectedOptions">
              <TagView v-for="value in optionObj.list" :key="`${type}_${value}`" :text="`${type}_${value}`"
                :ondelete="() => deleteOption(type, value)" />
            </template>
          </section>
        </div>
      </div>
    </div>

    <div class="right">
      <div class="right-container">
        <h3>Wybierz poziom dostępu</h3>

        <div class="dropdown-check-list" :class="{ visible: activeDropdown === 'access' }">
          <span class="anchor" @click="toggleDropdown('access')">
            {{
              selected_access ? access.find((opt) => opt.value == selected_access).text : "Dostęp"
            }}
          </span>
          <ul class="items">
            <li v-for="option in access" :key="option.value">
              <input type="radio" :id="'access-' + option.value" :value="option.value" v-model="selected_access">
              <label :for="'access-' + option.value">{{ option.text }}</label>
            </li>
          </ul>
        </div>
      </div>

    </div>

    <div class="buttons bottom">
      <button @click="goBack?.()" class="back-button"><span class="rotate-arrow">➔</span></button>
      <button @click="publish" class="publish-button">Publikuj</button>
    </div>
  </div>
</template>

<script>
import TagView from "@/components/TagView.vue";
export default {
  name: "TagSelectionPage",
  props: {
    goBack: {
      type: Function,
      required: false,
    },
  },
  components: {
    TagView,
  },
  data() {
    return {
      // tagOptions: [
      //   ["Element UI", "Styl", "Kolor", "Block", "Layout", "Flat"],
      //   ["Printing", "Font"],
      // ],
      // selectedTags: ["", "", ""],
      // tags: [],


      selectedOptions: new Map(
        [
          ['UI', { model: { get: () => this.selected_ui, set: (val) => this.selected_ui = val }, list: [] }],
          ['Style', { model: { get: () => this.selected_style, set: (val) => this.selected_style = val }, list: [] }],
          ['Color', { model: { get: () => this.selected_color, set: (val) => this.selected_color = val }, list: [] }]
        ]),
      selected_ui: [],
      selected_style: [],
      selected_color: [],
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
      activeDropdown: null,

      selected_access: '',
      access: [
        { value: 'option1', text: 'Opcja 1' },
        { value: 'option2', text: 'Opcja 2' },
        { value: 'option3', text: 'Opcja 3' }
      ],

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
    }
  },
  methods: {
    // addTag(index) {
    //   const newTag = this.selectedTags[index];
    //   if (newTag && !this.tags.includes(newTag)) {
    //     this.tags.push(newTag);
    //     this.selectedTags[index] = "";
    //   }
    // },
    // removeTag(index) {
    //   this.tags.splice(index, 1);
    // },

    deleteOption(type, value) {
      const optionObj = this.selectedOptions.get(type)
      optionObj.list = optionObj.list.filter(opt => opt != value)
      optionObj.model.set(optionObj.list)
    },
    updateSelectedOptions(type, values) {
      this.selectedOptions.get(type).list = values
    },
    toggleDropdown(id) {
      this.activeDropdown = this.activeDropdown === id ? null : id;
    },

    publish() {
      alert("Post opublikowany!");
    },
  },
};
</script>

<style scoped>
.left {
  grid-area: 1 / 1 / 2 / 2;
  display: flex;
  justify-content: center;
}

.left-container {
  width: 100%;
  min-width: 250px;
  max-width: 400px;
}

.right {
  grid-area: 1 / 2 / 2 / 3;
  display: flex;
  justify-content: center;
}

.right-container {
  width: 100%;
  min-width: 250px;
  max-width: 400px;
}

.right-container>*:first-child {
  margin-bottom: 20px;
}

.bottom {
  grid-area: 2 / 1 / 3 / 3;
}

.tag-sel-page {
  /* color: black; */

  display: grid;
  grid-template-columns: repeat(2, 1fr);
  grid-template-rows: 1fr 50px;
  grid-column-gap: 80px;
  grid-row-gap: 40px;
}

/* .tag {
  display: inline-block;
  background: #ddd;
  margin: 5px;
  padding: 5px 10px;
  border-radius: 20px;
  position: relative;
  transition: background 0.3s ease;
} */

/* .tag:hover {
  background: #bbb;
} */

.remove-icon {
  margin-left: 10px;
  cursor: pointer;
}

.buttons {
  /* margin-top: 20px; */
  display: flex;
  justify-content: space-between;
}


.upload-post {
  padding: 30px;
  max-width: 80vw;
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  grid-template-rows: 1fr 2fr 50px;
  grid-column-gap: 80px;
  grid-row-gap: 40px;
}

.file-upload {
  grid-area: 1 / 1 / 3 / 2;
}

.title-input {
  grid-area: 1 / 2 / 2 / 3;

  input {
    width: 100%;
  }
}

.description-input {
  grid-area: 2 / 2 / 3 / 3;
  display: flex;
  flex-direction: column;

  textarea {
    width: 100%;
    height: 100%;
  }
}

.button-continue {
  grid-area: 3 / 2 / 4 / 3;
  justify-self: right;
}

.file-upload {
  border: 2px dashed rgba(255, 255, 255, 0.5);
  border-radius: 10px;
  text-align: center;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: border-color 0.3s ease-in-out;
}

.file-upload.active {
  border-color: #007bff;
}

.file-upload .file-content {
  cursor: pointer;
}

.file-upload .placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.file-upload .placeholder span {
  font-size: 3rem;
  color: rgba(255, 255, 255, 0.7);
}

.file-upload .placeholder p {
  margin-top: 10px;
  font-size: 1rem;
  color: rgba(255, 255, 255, 0.7);
}

.file-upload .remove-file {
  background: none;
  border: none;
  color: red;
  font-size: 1.2rem;
  cursor: pointer;
  margin-top: 10px;
}

.hidden-input {
  display: none;
}

/* Button Style */
.publish-button {
  background: linear-gradient(135deg, #32334a, #1a1b2d);
  border: none;
  border-radius: 12px;
  color: white;
  font-size: 1.2rem;
  font-weight: bold;
  cursor: pointer;
  padding: 12px 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.3);
  aspect-ratio: 85/35;
}

.publish-button:hover {
  background: linear-gradient(135deg, #404158, #28293d);
}

.publish-button:active {
  transform: translateY(2px);
}

.button-continue {
  display: flex;
  justify-content: flex-end;
}

.back-button {
  background: linear-gradient(135deg, #32334a, #1a1b2d);
  border: none;
  border-radius: 12px;
  color: white;
  font-size: 1.5rem;
  font-weight: bold;
  cursor: pointer;
  padding: 12px 20px;
  display: flex;
  align-items: center;
  transform: rorate(180deg);
  justify-content: center;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.3);
  aspect-ratio: 85/35;
}

.back-button:hover {
  background: linear-gradient(135deg, #404158, #28293d);
}

.back-button:active {
  transform: translateY(2px);
}

.button-continue {
  display: flex;
  justify-content: flex-end;
}

.rotate-arrow {
  transform: rotate(180deg);
}

/* ////// */


.filter {
  padding: 20px 0;
  display: flex;
  flex-direction: column;
  gap: 15px;
  /* background-color: rgba(255, 255, 255, 0.15); */
  /* border-radius: 10px;
  border: 0.5px solid white; */
}

.selected-tags {
  display: flex;
  flex-wrap: wrap;
  row-gap: 8px;
  column-gap: 8px
}

.dropdown {
  width: 100%;
  position: relative;
  background: rgba(255, 255, 255, 0.15);
  border-radius: 10px;
  border: 1px solid white;
  font-size: 12px;
  font-weight: 700;
  padding: 8px 15px;
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