<template>
  <KeepAlive>
    <main>
      <h2 class="page-name">Dodaj nowy post</h2>
      <form class="" @submit.prevent="">

        <div class="upload-post-window background">
          <transition mode="out-in">
            <div class="upload-post" v-if="!showTagSelection">
              <div class="file-upload"
                :class="{ active: dragActive, 'error-shadow': showErrors && selectedFiles.length === 0 }"
                @dragover="handleDragOver" @dragleave="handleDragLeave" @drop="handleDrop">
                <input type="file" multiple class="hidden-input" @change="handleFileChange" ref="fileInput"
                  accept="image/*" />
                <div class="file-content">
                  <template v-if="selectedFiles.length">
                    <div class="photos-container">
                      <div v-for="(file, index) in selectedFiles" :key="index" class="photo-wrapper">
                        <img :src="getPreviewUrl(file)" class="photo-preview" />
                        <button type="button" class="star-button" :class="{ 'starred': mainPhotoIndex === index }"
                          @click.stop.prevent="setMainPhoto(index)">
                          ★
                        </button>
                        <button type="button" class="remove-button" @click.stop.prevent="removeFile(index)">
                          ×
                        </button>
                      </div>
                    </div>
                    <button type="button" class="add-more" @click.stop.prevent="$refs.fileInput.click()"
                      v-if="selectedFiles.length < 10"
                      style="background: transparent; border: none; color: rgba(255, 255, 255, 0.7); cursor: pointer;">
                      + Dodaj więcej zdjęć
                    </button>
                  </template>
                  <template v-else>
                    <div class="placeholder" @click.stop.prevent="$refs.fileInput.click()">
                      <span>+</span>
                      <p>Przeciągnij obrazki lub wybierz pliki (max. 10)</p>
                    </div>
                  </template>
                </div>
              </div>

              <label class="title-input">
                <h3>Tutył</h3>
                <input v-model="title" type="text" placeholder="Wpisz tutył"
                  :class="{ 'error-shadow': showErrors && !title }" />
              </label>
              <label class="description-input">
                <h3>Opis</h3>
                <textarea v-model="description" placeholder="Dodaj opis"></textarea>
              </label>

              <div class="button-continue">
                <button type="button" class="submit-button" @click="handleSubmit">
                  ➔
                </button>
              </div>
            </div>
            <TagSelectionPage v-else :goBack="() => showTagSelection = false" :publish="publish"
              :getGatherCallback="(fn) => tagInputGather = fn" />
          </transition>
        </div>
      </form>
    </main>
  </KeepAlive>
</template>


<script>
import TagSelectionPage from "./TagSelectionPage.vue";

export default {
  name: "UploadPostPage",
  components: { TagSelectionPage },
  data() {
    return {
      title: "",
      description: "",
      selectedFiles: [],
      dragActive: false,
      showTagSelection: false,
      showErrors: false, // Used to toggle error states
      mainPhotoIndex: 0,
      tagInputGather: () => { }
    };
  },
  computed: {
    isFormValid() {
      return this.selectedFiles.length > 0 && this.title;
    },
  },
  methods: {
    handleFileChange(event) {
      const newFiles = Array.from(event.target.files)
      if (this.selectedFiles.length + newFiles.length > 10) {
        alert('Możesz dodać maksymalnie 10 zdjęć')
        return
      }
      this.selectedFiles.push(...newFiles)
    },
    handleDrop(event) {
      event.preventDefault()
      this.dragActive = false
      const newFiles = Array.from(event.dataTransfer.files).filter(file => file.type.startsWith('image/'))
      if (this.selectedFiles.length + newFiles.length > 10) {
        alert('Możesz dodać maksymalnie 10 zdjęć')
        return
      }
      this.selectedFiles.push(...newFiles)
    },
    removeFile(index) {
      this.selectedFiles = this.selectedFiles.filter((_, i) => i !== index);
      if (this.mainPhotoIndex === index) {
        this.mainPhotoIndex = 0;
      } else if (this.mainPhotoIndex > index) {
        this.mainPhotoIndex--;
      }
    },
    setMainPhoto(index) {
      this.mainPhotoIndex = index;
    },
    getPreviewUrl(file) {
      return URL.createObjectURL(file);
    },
    handleDragOver(event) {
      event.preventDefault();
      this.dragActive = true;
    },
    handleDragLeave() {
      this.dragActive = false;
    },
    handleSubmit() {
      if (this.isFormValid) {
        this.showTagSelection = true;
        this.showErrors = false;
      } else {
        this.showErrors = true;
      }
    },
    publish() {
      console.log(this.title);
      console.log(this.description)
      const tagInput = this.tagInputGather()

      console.log(tagInput.selected_options)
      console.log(tagInput.selected_access)

      var data = new FormData()

      const other_files = this.selectedFiles.filter((_, i) => i != this.mainPhotoIndex)


      data.append('title', this.title)
      data.append('description', this.description)
      for (const file of other_files) {
        data.append('images', file)
      }
      data.append('mainImage', this.selectedFiles[this.mainPhotoIndex])

      data.append("access", tagInput.selected_access)

      for (const option of tagInput.selected_options) {
        data.append('tags', option)
      }

      fetch('/add-t', {
        method: 'POST',
        body: data
      })

      this.$router.push({ path: 'feed' })
    },
  },
};
</script>



<style scoped>
.upload-post-window {
  padding: 30px;
  max-width: 80vw;
  overflow: hidden;
}

.upload-post {
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
.submit-button {
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
  justify-content: center;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.3);
  aspect-ratio: 85/35;
}

.submit-button:hover {
  background: linear-gradient(135deg, #404158, #28293d);
}

.submit-button:active {
  transform: translateY(2px);
}

.button-continue {
  display: flex;
  justify-content: flex-end;
}

.v-enter-active,
.v-leave-active {
  transition: all 0.3s ease;
}

.v-enter-from,
.v-leave-to {
  opacity: 0;
  transform: translateX(-100%);
}

.error-shadow {
  box-shadow: 0 0 5px 2px red;
  border: 1px red;
}

.photos-container {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
  gap: 1rem;
  margin: 1rem 0;
  padding: 1rem;
}

.photo-wrapper {
  position: relative;
  aspect-ratio: 1;
}

.photo-preview {
  width: 100%;
  height: 100%;
  object-fit: cover;
  border-radius: 4px;
}

.star-button {
  position: absolute;
  top: 8px;
  right: 8px;
  background: transparent;
  border: none;
  color: rgba(255, 215, 0, 0.5);
  font-size: 24px;
  cursor: pointer;
  transition: color 0.2s;
}

.star-button.starred {
  color: gold;
}

.remove-button {
  position: absolute;
  top: 8px;
  left: 8px;
  background: rgba(0, 0, 0, 0.5);
  border: none;
  color: white;
  border-radius: 50%;
  width: 24px;
  height: 24px;
  cursor: pointer;
}

.selected-files {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
  gap: 1rem;
  margin-bottom: 1rem;
}

.file-item {
  position: relative;
  border: 1px solid #ddd;
  padding: 0.5rem;
}

.file-preview {
  width: 100%;
  height: 150px;
  object-fit: cover;
}

.file-controls {
  margin-top: 0.5rem;
}

.main-photo-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
}

.add-more {
  margin-top: 1rem;
  padding: 0.5rem;
  background: #f0f0f0;
  border: 1px dashed #999;
  cursor: pointer;
  width: 100%;
}
</style>