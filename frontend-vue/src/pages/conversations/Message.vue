<script>
import { imageDirectory } from '@/services/constants';

export default {
  name: "Message",
  props: {
    message: {
      type: Object,
      required: true
    }
  },
  data() {
    return {
      selectedImage: null,
      showModal: false
    }
  },
  computed: {
    isOutgoing() {
      return this.message.receiverUsername == this.$route.params.username;
    },
    messageSize() {
      const textLength = this.message.textContent.split(' ').length;
      let size;
      if (this.message.imagePaths.length == 0) {
        size = textLength > 30 ? 'large' : textLength > 15 ? 'medium' : 'small';
      } else {
        size = 'medium';
      }
      return size;
    },
    messageClass() {
      return {
        [this.messageSize]: true,
        'incoming': !this.isOutgoing,
        'outgoing': this.isOutgoing
      }
    }
  },
  methods: {
    imagePathHandler(imagePath) {
      return `${imageDirectory}/${imagePath}`;
    },
    openImage(imagePath) {
      this.selectedImage = this.imagePathHandler(imagePath);
      this.showModal = true;
    },
    closeModal() {
      this.showModal = false;
      this.selectedImage = null;
    }
  }
};
</script>

<template>
  <article class="message-container">
    <div class="message background" :class="messageClass">
      <span class="images-container" v-if="this.message.imagePaths.length > 0">
        <img v-for="imagePath in this.message.imagePaths" :key="imagePath" :src="imagePathHandler(imagePath)"
          @click="openImage(imagePath)" alt="Obrazek" />
      </span>
      <p class="message-text">{{ this.message.textContent }}</p>
    </div>
    <!-- Модальное окно для просмотра изображений -->
    <div v-if="showModal" class="modal" @click="closeModal">
      <img :src="selectedImage" class="modal-image" @click.stop />
    </div>
  </article>
</template>

<style scoped>
.message-container {
  width: 100%;
  display: flex;
  justify-content: flex-start;
}

.message-container:has(.outgoing) {
  justify-content: flex-end;
}

.message {
  background-color: var(--element-dark-color);
  text-wrap: balance;
  padding: 10px;
  width: fit-content;
}

.message.small {
  max-width: 40%;
}

.message.medium {
  max-width: 50%;
}

.message.large {
  max-width: 70%;
}

.message-text {
  margin: 0;
  word-wrap: break-word;
}

.incoming {
  background-color: var(--message-bg-left);
  border-top-left-radius: 15px;
  border-bottom-left-radius: 0;
  border-bottom-right-radius: 15px;
  border-top-right-radius: 15px;
}

.outgoing {
  background-color: var(--message-bg-right);
  border-top-left-radius: 15px;
  border-bottom-left-radius: 15px;
  border-bottom-right-radius: 0;
  border-top-right-radius: 15px;
}

.images-container {
  display: grid;
  grid-gap: 2px;
  width: 100%;
  aspect-ratio: 1;
}

.images-container img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  border-radius: 5px;
  cursor: pointer;
  transition: opacity 0.2s;
}

.images-container img:hover {
  opacity: 0.9;
}

/* Динамические стили для разного количества изображений */
.images-container:has(img:only-child) {
  grid-template-columns: 1fr;
}

.images-container:has(img:nth-child(2)) {
  grid-template-columns: repeat(2, 1fr);
}

.images-container:has(img:nth-child(3)) {
  grid-template-columns: repeat(2, 1fr);
}

.images-container:has(img:nth-child(3)) img:first-child {
  grid-column: span 2;
}

.images-container:has(img:nth-child(4)) {
  grid-template-columns: repeat(2, 1fr);
}

.modal {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: var(--element-dark-color);
  backdrop-filter: blur(10px);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal-image {
  max-width: 90%;
  max-height: 90%;
  object-fit: contain;
}

@media (max-width: 768px) {
  .message.small {
    max-width: 60%;
  }

  .message.medium {
    max-width: 75%;
  }

  .message.large {
    max-width: 85%;
  }

  .images-container {
    width: 250px;
  }
}
</style>