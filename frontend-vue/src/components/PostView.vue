<script>
import LikeIcon from '@/assets/Icons/LikeIcon.vue';
import Lock from '@/assets/Icons/Lock.vue';
import Link from '@/assets/Icons/Link.vue';
import ShareIcon from '@/assets/Icons/ShareIcon.vue';
import { Splide, SplideSlide } from '@splidejs/vue-splide';
import '@splidejs/vue-splide/css';
import defaultAvatar from '@/assets/Images/avatar.png';
import { imageDirectory, formatNumber } from '@/services/constants';
import { postsContentService } from '@/services/postsContent';

export default {
  name: 'PostView',
  data() {
    return {
      isCopied: false,
      isLiked: false
    }
  },

  components: {
    LikeIcon,
    ShareIcon,
    Lock,
    Link
  },
  props: {
    post: {
      type: Object,
      required: true,
    }
  },
  methods: {
    splideOptions() {
      return {
        rewind: true,
        arrows: (localStorage.getItem('allowArrow') === 'true') ? this.post.images.length > 1 : false,
        wheel: localStorage.getItem('allowWheel') === 'true',
        waitForTransition: localStorage.getItem('allowWheel') === 'true',
        releaseWheel: false,
        lazyLoad: 'nearby',
        preloadPages: 1,
        gap: '20px',
        pagination: this.post.images.length > 1,
      }
    },
    formatLikes() {
      return formatNumber(this.post.likes);
    },
    imagePathHandler(image) {
      if (!image) {
        return defaultAvatar;
      }
      return imageDirectory + image;
    },
    navigateToPortfolio() {
      if (this.post.author.username) {
        this.$router.push(`/${this.post.author.username}/portfolio`);
      }
    },
    copyLink() {
      let url;
      if (this.post.access === 'protected') {
        url = `${window.location.origin}/post/protected/${this.post.protectedAccessLink}`;
      } else {
        url = `${window.location.origin}/post/${this.post.id}`;
      }
      navigator.clipboard.writeText(url);
      this.isCopied = true;
      setTimeout(() => {
        this.isCopied = false;
      }, 3000);
    },
    async likePost() {
      try {
        const response = await postsContentService.likePost(this.post.id)
        this.post.isLiked = response.isLiked;
        this.post.likes = response.likes;
      } catch (error) {
        this.$router.push(`/login`);
      };
    }
  }
}
</script>

<template>
  <article class="post background">
    <div class="post-content">
      <Splide class="picture-slider" :options="splideOptions" aria-label="Post slides">
        <SplideSlide v-for="(picture, index) in post.images" :key="index">
          <img class="slide" :src="imagePathHandler(picture)" loading="lazy" alt="Post Picture"
            onmousedown='return false;' ondragstart='return false;'>
        </SplideSlide>
      </Splide>
      <article class="post-description">
        <h3>{{ post.title }}</h3>
        <div class="divider-horizontal"></div>
        <span class="post-sub-content">
          <p style="white-space: pre-wrap">{{ post.content }}</p>
          <span class="tags no-select" :class="{ 'tag-top-divider': post.tags?.length > 0 }">
            <span v-for="(tag, index) in post.tags" :key="index">{{ tag }}</span>
          </span>
        </span>
      </article>
    </div>

    <header class="post-header">
      <span class="post-profile" @click="navigateToPortfolio" role="button">
        <img :src="imagePathHandler(post.author.profileImage)" loading="lazy" alt="User Profile Picture">
        <h4>{{ post.author.firstName }} {{ post.author.lastName }}</h4>
      </span>

      <div class="stats no-select">
        <span v-if="post.access === 'protected'">
          <p class="access-type">Chroniony</p>
          <Link />
        </span>
        <span v-if="post.access === 'private'">
          <p class="access-type">Prywatny</p>
          <Lock />
        </span>
        <span>
          <p v-if="isCopied" class="access-type">Skopiowane</p>
          <ShareIcon @click="copyLink" />
        </span>
        <span @click="likePost" class="post-like">
          <LikeIcon :isLiked="post.isLiked" />{{ formatLikes() }}
        </span>
      </div>
    </header>
  </article>
</template>

<style scoped>
.access-type {
  font-weight: 500;
  font-size: 13px;
}

.post {
  display: flex;
  flex-direction: column;
  width: max-content;
}

.post-content {
  display: flex;
  flex-direction: row;
  gap: 30px;
  padding: 20px 30px 20px 20px;
}

.post-like {
  cursor: pointer;
}

.picture-slider {
  display: flex;
  flex-direction: row;
  width: 400px;
  height: 350px;
  display: flex;
  justify-content: center;

  :deep(.splide__track) {
    height: 100%;
  }

  :deep(.splide__list) {
    height: 100%;
  }

  :deep(.splide__slide) {
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .slide {
    width: 100%;
    height: 100%;
    object-fit: cover;
    border-radius: 5px;
  }
}

.post-description {
  padding-left: 20px;
  border-left: 2px solid var(--element-light-color);
  width: 250px;
  max-width: 250px;
  height: 350px;
  max-height: 350px;
  display: flex;
  flex-direction: column;

  h3 {
    font-size: 18px;
    font-weight: 700;
    margin-bottom: 6px;
  }
}

.post-sub-content {
  display: flex;
  flex-direction: column;
  flex: 1;
  min-height: 0;
  gap: 10px;

  p {
    flex: 1;
    overflow-y: auto;
    margin: 0;
    margin-top: 10px;
    padding-right: 10px;
  }

  .tags {
    padding-top: 5px;
    display: flex;
    flex-wrap: wrap;
    gap: 5px;

    span {
      padding: 5px 10px;
      border-radius: 10px;
      background: var(--element-light-color);
      border: 0.5px solid var(--element-border-light-color);
      font-weight: 600;
      font-size: 12px;
      color: var(--primery-text-color);
    }
  }

  .tag-top-divider {
    border-top: 1px solid var(--element-light-color);
  }

  /* scrollbar styling */
  ::-webkit-scrollbar {
    width: 4px;
  }

  ::-webkit-scrollbar-thumb {
    background: var(--element-hover-light-color);
    border-radius: 1px;
  }

  ::-webkit-scrollbar-track {
    background: var(--element-light-color);
    border-radius: 1px;
  }
}

.divider-horizontal {
  height: 1px;
  width: auto;
  margin: 0;
  border: none;
  background-color: var(--element-light-color);
  border-radius: 1px;
}

.post-header {
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: space-between;
  border-radius: 15px;
  border: 0.5px solid var(--element-border-light-color);
  background: var(--element-light-color);
  padding: 10px 20px 10px 15px;

  .post-profile {
    display: flex;
    flex-direction: row;
    align-items: center;
    gap: 10px;
    font-weight: 700;
    cursor: pointer;
    transition: opacity 0.2s ease;

    &:hover {
      opacity: 0.8;
    }

    img {
      width: 30px;
      height: 30px;
      padding: 2px;
      border-radius: 50%;
      border: 0.5px solid var(--element-border-light-color);
      object-fit: cover;
    }
  }
}

.stats {
  display: flex;
  flex-direction: row;
  align-items: center;
  gap: 30px;
  font-weight: 600;

  span {
    display: flex;
    flex-direction: row;
    align-items: center;
    gap: 12px;
  }
}
</style>