<script>
import UserBadge from './UserBadge.vue';
import postReportService from '../../services/posts';
import { imageDirectory } from '../../services/constants';

export default {
  name: 'PostView',
  components: {
    UserBadge,
  },
  props: {
    post: {
      type: Object,
      required: true,
    },
  },
  data() {
    return {
      selectedImage: null,
    };
  },
  methods: {
    async deletePost() {
      await postReportService.deletePost(this.post.reportedPost.id);
    },
    async dismissReport() {
      await postReportService.dismissPost(this.post.id);
    },
    imageUrl(image) {
      return `${imageDirectory}${image}`;
    },
    formatDate(date) {
      return new Date(date).toLocaleDateString();
    },
    openImage(image) {
      this.selectedImage = image;
    },
    closeImage() {
      this.selectedImage = null;
    },
    handleModalClick(event) {
      // Закрываем модальное окно только при клике на фон
      if (event.target.classList.contains('modal-overlay')) {
        this.closeImage();
      }
    }
  }
}
</script>

<template>
  <article class="flex flex-col gap-4 p-4 border border-gray-300 rounded-lg max-w-max">
    <span class="flex flex-row gap-2 items-center justify-around">
      <span class="flex flex-col gap-2">
        <p>Użytkownik reklamowany</p>
        <UserBadge :user="post.reportedUser" />
      </span>
      <span class="flex flex-col gap-2">
        <p>Użytkownik zgłaszający</p>
        <UserBadge :user="post.reporter" />
      </span>
    </span>
    <span class="flex flex-col gap-2">
      <p>{{ post.reportedPost.title }}</p>
      <p>{{ post.reportedPost.content }}</p>
      <span class="flex gap-2 flex-wrap max-w-108">
        <img v-if="post.reportedPost.images.mainImage" :src="imageUrl(post.reportedPost.images.mainImage)"
          alt="Main post image" class="w-32 h-32 object-cover rounded-lg cursor-pointer hover:opacity-90"
          @click="openImage(post.reportedPost.images.mainImage)">
        <img v-for="image in post.reportedPost.images.images" :key="image" :src="imageUrl(image)" alt="Post image"
          class="w-32 h-32 object-cover rounded-lg border border-gray-300 cursor-pointer hover:opacity-90"
          @click="openImage(image)">
      </span>
    </span>
    <span class="flex flex-row gap-2 justify-between ">
      <span class="flex flex-col gap-2">
        <p>{{ post.reportReason }}</p>
        <p>{{ post.description }}</p>
        <p>{{ formatDate(post.reportDate) }}</p>
      </span>
      <span class="flex flex-col gap-2 max-w-max">
        <button class="accept-button" @click="dismissReport">Odrzuć zgłoszenie</button>
        <button class="reject-button" @click="deletePost">Usuń post</button>
      </span>
    </span>
  </article>

  <!-- Modal для просмотра изображения -->
  <div v-if="selectedImage"
    class="modal-overlay fixed inset-0 bg-black bg-opacity-75 flex items-center justify-center z-50"
    @click="handleModalClick">
    <div class="modal-content max-w-[90%] max-h-[90vh]">
      <img :src="imageUrl(selectedImage)" alt="Full size image" class="max-w-full max-h-[90vh] object-contain">
    </div>
  </div>
</template>

<style scoped>
.modal-overlay {
  backdrop-filter: blur(3px);
}

.modal-content {
  animation: zoom-in 0.2s ease-out;
}

@keyframes zoom-in {
  from {
    transform: scale(0.95);
    opacity: 0;
  }

  to {
    transform: scale(1);
    opacity: 1;
  }
}
</style>