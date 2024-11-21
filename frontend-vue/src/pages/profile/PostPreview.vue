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
  // data() {
  //   return {
  //     post: {
  //       title: 'Post title lorem ipsum dolor sit amet. Post title lorem ipsum dolor sit amet.',
  //       image: 'https://placehold.co/600x500',
  //       like: 2334,
  //       isLiked: false
  //     }
  //   }
  // },
  computed: {
    formattedLikes() {
      const likes = this.post.like;
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
    <img class="post-preview-img" :src="post.image" alt="post.title">
    <span class="post-preview-info background">
      <p class="post-preview-title">{{ post.title }}</p>
      <span class="post-like">
        <LikeIcon />
        <p>{{ formattedLikes }}</p>
      </span>
    </span>
  </article>
</template>

<style scoped>
.post-preview {
  display: flex;
  flex-direction: column;
  width: fit-content;
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
  background: rgba(96, 96, 96, 0.40);
  padding: 15px 30px;
  box-shadow: 5px 5px 25px 0px rgba(0, 0, 0, 0.25);

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

  p {
    font-weight: 600;
  }
}
</style>