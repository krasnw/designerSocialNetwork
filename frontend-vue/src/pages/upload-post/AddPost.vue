<script>
import TagView from '@/components/TagView.vue';
import { filterTagService } from '@/services/filterTag';
import { postsContentService } from '@/services/postsContent';

export default {
  name: "AddPost",
  components: {
    TagView
  },
  data() {
    return {
      // post data
      title: "",
      content: "",
      images: [],
      mainImageIndex: 0,
      isDragging: false,
      // data for post tags
      accessLevel: "public",
      selectedOptions: [],
      selected_ui: [],
      selected_style: [],
      selected_color: [],
      ui: [],
      style: [],
      color: [],
      accessLevels: [
        { value: "public", text: "Public" },
        { value: "protected", text: "By link" },
        { value: "private", text: "Private" },
      ],
      activeDropdown: null,
      // errors 
      errors: {
        title: false,
        images: false,
      },
      // page flags
      activePage: 1,
      showSuccess: false,
      protectedPost: false,
      protectedLink: '',
      isCopied: false,
    }
  },
  computed: {
    selectedAccessLevelText() {
      const selected = this.accessLevels.find(level => level.value === this.accessLevel)
      return selected ? selected.text : 'Access level'
    }
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
    } catch (error) {
      throw new Error('Failed to fetch tags');
    }
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
      this.selectedOptions = this.selectedOptions.filter(opt => !opt.startsWith(`${type}:`));
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
    removeTag(tagText) {
      const [type, value] = tagText.split(': ');
      switch (type) {
        case 'UI':
          this.selected_ui = this.selected_ui.filter(item => item !== value);
          break;
        case 'Style':
          this.selected_style = this.selected_style.filter(item => item !== value);
          break;
        case 'Color':
          this.selected_color = this.selected_color.filter(item => item !== value);
          break;
      }
      this.selectedOptions = this.selectedOptions.filter(opt => opt !== tagText);
    },
    nextPage() {
      if (this.validateFirstPage()) {
        this.activePage++;
      }
    },
    prevPage() {
      this.activePage--;
    },
    validateFirstPage() {
      this.errors.title = !this.title;
      this.errors.images = this.images.length === 0;
      return !this.errors.title && !this.errors.images;
    },
    handleFileUpload(event) {
      const files = event.target.files || event.dataTransfer.files;
      const newFiles = Array.from(files).filter(file => file.type.startsWith('image/'));

      newFiles.forEach(file => {
        const reader = new FileReader();
        reader.onload = (e) => {
          this.images.push({
            preview: e.target.result,
            file: file
          });
        };
        reader.readAsDataURL(file);
      });
    },

    triggerFileInput() {
      this.$refs.fileInput.click();
    },

    onDragEnter(e) {
      e.preventDefault();
      this.isDragging = true;
    },

    onDragLeave(e) {
      e.preventDefault();
      this.isDragging = false;
    },

    onDrop(e) {
      e.preventDefault();
      this.isDragging = false;
      this.handleFileUpload(e);
    },

    removeImage(index) {
      this.images.splice(index, 1);
      if (this.mainImageIndex >= this.images.length) {
        this.mainImageIndex = Math.max(0, this.images.length - 1);
      }
    },

    setMainImage(index) {
      this.mainImageIndex = index;
    },

    async copyLink() {
      const fullLink = `http://localhost:8084/post/protected/${this.protectedLink}`;
      try {
        await navigator.clipboard.writeText(fullLink);
        this.isCopied = true;
      } catch (err) {
        console.error('Failed to copy link:', err);
      }
    },

    async publishPost() {
      try {
        this.isCopied = false;
        const formData = new FormData();
        formData.append('Title', this.title);
        formData.append('Content', this.content);
        formData.append('MainImageIndex', this.mainImageIndex);
        formData.append('AccessLevel', this.accessLevel);
        this.selectedOptions.forEach(tag => {
          formData.append('Tags', tag.split(': ')[1]);
        });

        // Append all images
        this.images.forEach(image => {
          formData.append('Images', image.file);
        });

        const response = await postsContentService.createPost(formData);
        this.protectedPost = response.access === 'protected';
        if (this.protectedPost) {
          this.protectedLink = response.protectedAccessLink;
        }
        this.activePage = 3;
        this.showSuccess = true;
      } catch (error) {
        console.error('Failed to publish post:', error);
      }
    }
  }
}
</script>

<template>
  <main>
    <h2 class="page-name">Add new post</h2>
    <section class="wrapper">
      <!-- first page of the form -->
      <section v-if="activePage === 1">
        <form class="form-page background">
          <div class="left-side">



            <h3 class="no-select">Select images</h3>

            <div class="file-upload" @click="triggerFileInput" @dragenter="onDragEnter" @dragleave="onDragLeave"
              @dragover.prevent @drop="onDrop" :class="{ 'dragging': isDragging }">
              <input type="file" multiple accept="image/*" class="hidden-input" ref="fileInput"
                @change="handleFileUpload" />

              <div v-if="images.length === 0" class="empty-file-upload">
                <span class="icon">+</span>
                <h4 class="text" :class="{ 'error': errors.images }">Drag and drop here or upload from your device</h4>
              </div>

              <div v-else class="images-preview">
                <div class="preview-grid">
                  <div v-for="(image, index) in images" :key="index" class="image-preview"
                    :class="{ 'main-image': index === mainImageIndex }">
                    <img :src="image.preview" :alt="`Preview ${index + 1}`" />
                    <div class="image-actions">
                      <button type="button" @click.stop="setMainImage(index)" :disabled="index === mainImageIndex">
                        ★
                      </button>
                      <button type="button" @click.stop="removeImage(index)">✕</button>
                    </div>
                  </div>
                </div>
                <p class="add-more-hint">Click or drag to add more images</p>
              </div>
            </div>



          </div>
          <div class="right-side">
            <h3 class="no-select">Title</h3>
            <input type="text" v-model="title" placeholder="Post title" :class="{ 'error': errors.title }" />
            <h3 class="no-select">Description</h3>
            <textarea v-model="content" placeholder="Post description" rows="7" />
          </div>
        </form>
        <span class="controls">
          <button class="button" @click="nextPage">Next</button>
        </span>
      </section>

      <!-- second page of the form -->
      <section v-if="activePage === 2">
        <form class="form-page background">
          <div class="left-side no-select">
            <h3>Category</h3>
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
              <span class="anchor" @click="toggleDropdown('style', $event)">Style</span>
              <ul class="items">
                <li v-for="option in style" :key="option.value"
                  @click="toggleCheckbox($event, 'style-' + option.value)">
                  <input type="checkbox" :id="'style-' + option.value" :value="option.value" v-model="selected_style">
                  <label :for="'style-' + option.value">{{ option.text }}</label>
                </li>
              </ul>
            </div>

            <div class="dropdown-check-list" :class="{ visible: activeDropdown === 'color' }" ref="color">
              <span class="anchor" @click="toggleDropdown('color', $event)">Color</span>
              <ul class="items">
                <li v-for="option in color" :key="option.value"
                  @click="toggleCheckbox($event, 'color-' + option.value)">
                  <input type="checkbox" :id="'color-' + option.value" :value="option.value" v-model="selected_color">
                  <label :for="'color-' + option.value">{{ option.text }}</label>
                </li>
              </ul>
            </div>

            <h3>Selected categories</h3>
            <p v-if="selectedOptions.length === 0">No categories selected</p>
            <section v-else class="selected-tags">
              <TagView v-for="option in selectedOptions" :key="option" :text="option" @delete="removeTag" />
            </section>
          </div>

          <div class="right-side no-select">
            <h3>Access level</h3>
            <div class="dropdown-check-list" :class="{ visible: activeDropdown === 'access' }" ref="access">
              <span class="anchor" @click="toggleDropdown('access', $event)">{{ selectedAccessLevelText }}</span>
              <ul class="items">
                <li v-for="option in accessLevels" :key="option.value" @click="accessLevel = option.value">
                  <input type="radio" :id="'access-' + option.value" :value="option.value" v-model="accessLevel">
                  <label :for="'access-' + option.value">{{ option.text }}</label>
                </li>
              </ul>
            </div>
          </div>
        </form>
        <span class="controls">
          <button class="button" @click="prevPage">Back</button>
          <button class="button" @click="publishPost">Publish</button>
        </span>
      </section>

      <!-- third page of the form -->
      <section v-if="showSuccess" class="post-info">
        <section class="success-message background no-select">
          <h3>Post submitted</h3>
          <p v-if="protectedPost" class="advice">The post is saved as <span class="green-gradient">protected</span> and
            is available in
            your profile
            under the <span class="red-gradient">Private</span> tab.
            <br />
            It is available to everyone via <a @click="copyLink">Link</a>, <span v-if="isCopied"
              class="green-gradient">Copied</span><span class="advice" v-else>click to copy</span>
          </p>
          <a class="link" @click="this.$router.push('profile/me')">Go to profile</a>
        </section>

        <span>
          <button class="button" @click="activePage = 1; showSuccess = false">Add another</button>
        </span>
      </section>
    </section>
  </main>
</template>

<style scoped>
.advice {
  text-wrap: balance;
  font-weight: 600;
}

.post-info {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.success-message {
  display: flex;
  flex-direction: column;
  gap: 5px;
  padding: 20px;
}

.link {
  font-weight: 600;
  cursor: pointer;
}

.wrapper {
  display: flex;
  flex-direction: column;
  gap: 30px;
  max-width: 925px;
  width: 80%;
  min-width: 550px;
  margin-bottom: 100px;
}

.form-page {
  padding: 30px;
  display: flex;
  flex-direction: row;
  gap: 30px;
  width: 100%;
}

.left-side {
  display: flex;
  flex-direction: column;
  flex: 1 1 50%;
  /* flex-grow | flex-shrink | flex-basis */
  gap: 20px;
  min-width: 0;
  /* prevents content from forcing width */
}

.right-side {
  display: flex;
  flex-direction: column;
  flex: 1 1 50%;
  gap: 20px;
  min-width: 0;
}

.dropdown-check-list {
  width: 100%;
  position: relative;
  background: var(--element-light-color);
  border-radius: 10px;
  border: 1px solid var(--element-border-light-color);
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

.selected-tags {
  display: flex;
  flex-wrap: wrap;
  row-gap: 8px;
  column-gap: 8px
}

.controls {
  display: flex;
  justify-content: flex-end;
  margin-top: 20px;
  gap: 20px;
}

button {
  background-color: var(--element-light-color);
  border: 0.5px solid var(--element-border-light-color);
  backdrop-filter: blur(40px);
}

.file-upload {
  border: 1px dashed var(--element-border-light-color);
  background-color: var(--element-light-color);
  border-radius: 10px;
  text-align: center;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: border-color 0.3s ease-in-out;
  height: 100%;
  cursor: pointer;
  box-shadow: 5px 5px 25px var(--shadow-color);
  display: flex;
  padding: 20px;
  min-height: 200px;
}

.file-upload.dragging {
  border-color: var(--accent-color);
  background-color: var(--element-hover-light-color);
}

.drag-hint {
  color: var(--placeholder-color);
  font-size: 12px;
  margin-top: 8px;
}

.images-preview {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  gap: 15px;
  max-height: 280px;
  overflow-y: auto;
  padding-right: 5px;

  &::-webkit-scrollbar {
    width: 8px;
  }

  &::-webkit-scrollbar-track {
    background: var(--element-light-color);
    border-radius: 4px;
  }

  &::-webkit-scrollbar-thumb {
    background: var(--element-border-light-color);
    border-radius: 4px;
  }

  &::-webkit-scrollbar-thumb:hover {
    background: var(--element-hover-light-color);
  }
}

.preview-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(100px, 1fr));
  gap: 10px;
  width: 100%;
  min-height: min-content;
  /* Позволяет контейнеру расширяться */
}

.image-preview {
  position: relative;
  aspect-ratio: 1;
  border-radius: 8px;
  overflow: hidden;
  border: 2px solid transparent;
}

.image-preview.main-image {
  border-color: var(--accent-color);
}

.image-preview img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.image-actions {
  position: absolute;
  top: 5px;
  right: 5px;
  display: flex;
  gap: 5px;
  opacity: 0;
  transition: opacity 0.2s;
}

.image-preview:hover .image-actions {
  opacity: 1;
}

.image-actions button {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  border: none;
  background: rgba(0, 0, 0, 0.5);
  color: white;
  cursor: pointer;
  padding: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
}

.image-actions button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.add-more-hint {
  position: sticky;
  /* Делаем подсказку прикрепленной */
  bottom: 0;
  background: var(--element-light-color);
  padding: 10px 0;
  margin: 0;
  text-align: center;
  color: var(--placeholder-color);
  font-size: 12px;
}

.empty-file-upload {
  flex-direction: column;
  justify-content: center;
  align-items: center;

  .icon {
    font-size: 40px;
    color: var(--placeholder-color);
  }

  .text {
    color: var(--placeholder-color);
    font-weight: 700;
    font-size: 14px;
    max-width: 160px;
  }
}

.error {
  color: var(--placeholder-error-color);
}

.error::placeholder {
  color: var(--placeholder-error-color);
}

.empty-file-upload .text.error {
  color: var(--placeholder-error-color);
}

.hidden-input {
  display: none;
}
</style>