<script>
export default {
  name: "UploadPostPage",
  data() {
    return {
      title: "",
      description: "",
      selectedFile: null,
      dragActive: false, 
    };
  },
  methods: {
    handleFileChange(event) {
      const file = event.target.files[0];
      if (file) {
        this.selectedFile = file;
      }
    },
    handleDrop(event) {
      event.preventDefault();
      const file = event.dataTransfer.files[0];
      if (file) {
        this.selectedFile = file;
      }
      this.dragActive = false;
    },
    removeFile(event) {
      this.selectedFile = null;
      event.stopPropagation();
    },
    handleDragOver(event) {
      event.preventDefault();
      this.dragActive = true;
    },
    handleDragLeave() {
      this.dragActive = false;
    },
  },
};
</script>

<template>
  <main>
    <h2 class="page-name">Dodaj nowy post</h2>
    <form class="upload-post background" method="post">
      <div
        class="file-upload"
        :class="{ active: dragActive }"
        @dragover="handleDragOver"
        @dragleave="handleDragLeave"
        @drop="handleDrop"
      >
        <input
          type="file"
          class="hidden-input"
          @change="handleFileChange"
          ref="fileInput"
        />
        <div class="file-content" @click="$refs.fileInput.click()">
          <template v-if="selectedFile">
            <p>{{ selectedFile.name }}</p>
            <button type="button" class="remove-file"  @click="removeFile">
              Usuń
            </button>
          </template>
          <template v-else>
            <div class="placeholder">
              <span>+</span>
              <p>Przeciągnij obrazek lub wybierz plik</p>
            </div>
          </template>
        </div>
      </div>

      <label class="title-input">
        <h3>Tutył</h3>
        <input v-model="title" type="text" placeholder="Wpisz tutył" />
      </label>
      <label class="description-input">
        <h3>Opis</h3>
        <textarea v-model="description" placeholder="Description"></textarea>
      </label>

      <div class="button-continue">
        <button type="submit" class="submit-button" >➔</button>
      </div>
    </form>
  </main>
</template>

<style scoped>
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

</style>