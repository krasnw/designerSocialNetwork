<script>
import LikeIcon from '@/assets/Icons/LikeIcon.vue';
import Lock from '@/assets/Icons/Lock.vue';
import ShareIcon from '@/assets/Icons/ShareIcon.vue';
import { Splide, SplideSlide } from '@splidejs/vue-splide';
import '@splidejs/vue-splide/css'; // Добавляем стили


export default {
  name: 'PostView',

  components: {
    LikeIcon,
    ShareIcon,
    Lock
  },

  props: {
    post: {
      type: Object,
      required: true,
      validator(post) {
        return ['username', 'userProfilePicture', 'postPictures', 'title', 'description'].every(prop => prop in post)
      }
    }
  },

  computed: {
    formattedLikes() {
      const likes = this.post.likes;
      if (likes >= 1000) {
        return (likes / 1000).toFixed(1).replace('.', ',') + ' K';
      }
      return likes;
    },
    formattedShares() {
      const shares = this.post.shares;
      //for thousands
      if (shares >= 1000) {
        return (shares / 1000).toFixed(1).replace('.', ',') + ' K';
      } // for millions
      else if (shares >= 1000000) {
        return (shares / 1000000).toFixed(1).replace('.', ',') + ' M';
      } // for billions
      else if (shares >= 1000000000) {
        return (shares / 1000000000).toFixed(1).replace('.', ',') + ' B';
      }
      return shares;
    },
    splideOptions() {
      return {
        rewind: true,
        arrows: false,
        gap: '20px',
        pagination: this.post.postPictures.length > 1, // скрываем пагинацию если картинка одна
      }
    }
  },
}
</script>

<template>
  <article class="post background">
    <div class="post-content">
      <Splide class="picture-slider" :options="splideOptions" aria-label="Post slides">
        <!-- <img v-for="(picture, index) in post.postPictures" :key="index" :src="picture" alt="Post Picture"> -->
        <SplideSlide v-for="(picture, index) in post.postPictures" :key="index">
          <img class="slide" :src="picture" alt="Post Picture">
        </SplideSlide>
      </Splide>
      <article class="post-description">
        <h3>{{ post.title }}</h3>
        <div class="divider-horizontal"></div>
        <span class="post-sub-content">
          <p style="white-space: pre-wrap">{{ post.description }}</p>
          <span class="tags">
            <span v-for="(tag, index) in post.tags" :key="index">{{ tag }}</span>
          </span>
        </span>
      </article>
    </div>

    <header class="post-header">
      <span class="post-profile">
        <img :src="post.userProfilePicture" alt="User Profile Picture">
        <p>{{ post.username }}</p>
      </span>

      <div class="stats">
        <span v-if="post.isPrivate">
          <Lock />Prywatne
        </span>
        <span>
          <ShareIcon />{{ formattedShares }}
        </span>
        <span>
          <LikeIcon />{{ formattedLikes }}
        </span>
      </div>
    </header>
  </article>
</template>

<style scoped>
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

.picture-slider {
  display: flex;
  flex-direction: row;
  width: 400px;
  height: 350px;

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
  border-left: 2px solid rgb(255 255 255 / 0.15);
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
    border-top: 1px solid rgb(255 255 255 / 0.15);

    span {
      padding: 5px 10px;
      border-radius: 10px;
      background: rgba(255, 255, 255, 0.15);
      border: 0.5px solid rgba(255, 255, 255, 0.3);
      font-weight: 600;
      font-size: 12px;
    }
  }

  /* scrollbar styling */
  ::-webkit-scrollbar {
    width: 4px;
  }

  ::-webkit-scrollbar-thumb {
    background: rgba(255, 255, 255, 0.2);
    border-radius: 1px;
  }

  ::-webkit-scrollbar-track {
    background: rgba(255, 255, 255, 0.15);
    border-radius: 1px;
  }
}

.divider-horizontal {
  height: 1px;
  width: auto;
  margin: 0;
  border: none;
  background-color: rgb(255 255 255 / 0.15);
  border-radius: 1px;
}

.post-header {
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: space-between;
  border-radius: 15px;
  border: 0.5px solid rgba(255, 255, 255, 0.3);
  background: rgba(255, 255, 255, 0.15);
  padding: 10px 20px 10px 15px;

  .post-profile {
    display: flex;
    flex-direction: row;
    align-items: center;
    gap: 10px;
    font-weight: 700;

    img {
      width: 30px;
      height: 30px;
      padding: 2px;
      border-radius: 50%;
      border: 0.5px solid white;
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