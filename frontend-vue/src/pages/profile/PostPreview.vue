<script>
import LikeIcon from '@/assets/Icons/LikeIcon.vue';
import PostView from '@/components/PostView.vue';
import { postsContentService } from '@/services/postsContent';

export default {
  name: 'PostPreview',
  components: {
    LikeIcon,
    PostView
  },
  data() {
    return {
      isLoading: false,
      fullPostVisible: false,
      fullPost: null
    }
  },
  props: {
    post: {
      type: Object,
      required: true
    },
  },
  computed: {
    formattedLikes() {
      const likes = this.post.likes;
      if (likes >= 1000) {
        return (likes / 1000).toFixed(1).replace('.', ',') + ' K';
      }
      return likes;
    }
  },
  methods: {
    async showPost() {
      this.isLoading = true;
      this.fullPost = await postsContentService.getPost(this.post.id).finally(() => {
        this.isLoading = false;
      });
      this.fullPostVisible = true;
    },
    hidePost() {
      this.fullPostVisible = false;
    },
    async deletePost() {
      await postsContentService.deletePost(this.post.id);
      this.$root.reloadView();
    },
  }
}
</script>

<template>
  <section class="post-wrapper">
    <article v-if="!fullPostVisible" class="post-preview background" @click="showPost">
      <img class="post-preview-img" :src="post.image" alt="post.title" onmousedown='return false;'
        ondragstart='return false;'>
      <span class="post-preview-info background">
        <h4 class="post-preview-title">{{ post.title }}</h4>
        <span class="post-like">
          <LikeIcon />
          <h4>{{ formattedLikes }}</h4>
        </span>
      </span>
    </article>
    <div v-if="isLoading" class="loading">Ładowanie
      <Spinner class="spinner" />
    </div>
    <article v-if="fullPostVisible" class="post-view">
      <PostView :post="fullPost" />
      <section class="post-control-buttons">
        <button class="accept-button button" @click="hidePost">Schowaj</button>
        <button class="delete-button button" v-if="!$route.params.username" @click="deletePost">Usuń</button>
        <button class="delete-button button" v-else>Report</button>
      </section>
    </article>
  </section>
</template>

<style scoped>
.post-wrapper {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.post-preview {
  display: flex;
  flex-direction: column;
  width: fit-content;
  transition: transform 0.3s ease;
  cursor: pointer;
}

.post-view {
  display: flex;
  flex-direction: column;
  gap: 20px;
  width: 100%;
}

.post-preview:hover {
  transform: scale(1.01);
  transition: transform 0.3s ease;
}

.post-preview-img {
  margin: 20px;
  border-radius: 10px;
  width: 420px;
  height: 270px;
  object-fit: cover;
}

.post-preview-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: var(--element-light-color);
  ;
  padding: 15px 30px;
  box-shadow: 5px 5px 25px 0px var(--shadow-color);

}

.post-preview-title {
  font-weight: 700;
  flex: 1;
  max-width: 290px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  margin-right: 10px;
}

.post-like {
  display: flex;
  align-items: center;
  gap: 15px;
  flex-shrink: 0;

  h4 {
    font-weight: 600;
  }
}

.post-control-buttons {
  display: flex;
  width: max-content;
  gap: 15px;
  /* padding: 20px; */
}

.delete-button {
  background: var(--delete-button-color);
  border-color: var(--delete-button-border-color);
}

.accept-button {
  background: var(--element-light-color);
  border-color: var(--element-border-light-color);
}
</style>