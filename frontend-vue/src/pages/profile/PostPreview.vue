<script>
import LikeIcon from '@/assets/Icons/LikeIcon.vue';

export default {
  name: 'PostPreview',
  components: {
    LikeIcon,
  },
  props: {
    post: {
      type: Object,
      required: true
    }
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
}
</script>

<template>
  <article class="post-preview background">
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
</template>

<style scoped>
.post-preview {
  display: flex;
  flex-direction: column;
  width: fit-content;
  transition: transform 0.3s ease;
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
</style>