<script>
export default {
  name: "UploadPostPage",
  data() {
    return {
      title: '',
      description: ''
    }
  }
};
</script>

<template>
  <main>
    <h2 class="page-name">Upload new post</h2>
    <form class="upload-post background" method="post">
      <label class="file-upload">
        <h3>Upload file</h3>
        <input type="file" />
      </label>
      <label class="title-input">
        <h3>Title</h3>
        <input v-model="title" type="text" placeholder="Title" />
      </label>
      <label class="description-input">
        <h3>Description</h3>
        <textarea v-model="description" placeholder="Description"></textarea>
      </label>
    </form>
  </main>
</template>

<style scoped>
.upload-post {
  padding: 30px;
  max-width: 80vw;
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  grid-template-rows: 1fr 2fr;
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

  textarea {
    width: 100%;
  }
}
</style>