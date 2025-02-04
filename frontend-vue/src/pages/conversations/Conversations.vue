<script>
import ClipIcon from '@/assets/Icons/ClipIcon.vue';
import SendIcon from '@/assets/Icons/SendIcon.vue';
import { chatMessagesService } from '@/services/chat';

export default {
  name: "ConversationsPage",
  components: {
    ClipIcon,
    SendIcon
  },
  data() {
    return {
      message: '',
      isMenuOpen: false,
      selectedImages: [],
      menuItems: [
        { icon: '🖼️', text: 'Dodaj obrazki', action: 'addImage' },
        { icon: '💰', text: 'Zaproś opłatę', action: 'requestPayment' },
        { icon: '🚫', text: 'Skończ zlecenie', action: 'endChat' }
      ]
    }
  },
  computed: {
    hasSelectedChat() {
      return !!this.$route.params.username;
    },
    chatKey() {
      return this.$route.params.username;
    }
  },
  methods: {
    send() {
      if (this.message || this.selectedImages.length) {
        const formData = new FormData();
        formData.append('ReceiverUsername', this.$route.params.username);
        formData.append('TextContent', this.message);
        this.selectedImages.forEach(image => {
          formData.append('Images', image.file);
        });

        chatMessagesService.sendMessage(formData).catch(error => {
          this.$router.push(`/error/${error.status}`);
        });

        this.message = '';
        this.selectedImages = [];
      }
    },
    toggleMenu() {
      this.isMenuOpen = !this.isMenuOpen;
    },
    handleAction(action) {
      switch (action) {
        case 'addImage':
          this.$refs.fileInput.click();
          break;
        // Other actions will be implemented later
      }
      this.isMenuOpen = false;
    },
    handleFileSelect(event) {
      const file = event.target.files[0];
      if (file && file.type.startsWith('image/')) {
        this.addImagePreview(file);
      }
    },
    handleDrop(event) {
      event.preventDefault();
      const file = event.dataTransfer.files[0];
      if (file && file.type.startsWith('image/')) {
        this.addImagePreview(file);
      }
    },
    addImagePreview(file) {
      const reader = new FileReader();
      reader.onload = (e) => {
        this.selectedImages.push({
          id: Date.now(),
          url: e.target.result,
          file: file
        });
      };
      reader.readAsDataURL(file);
    },
    removeImage(imageId) {
      this.selectedImages = this.selectedImages.filter(img => img.id !== imageId);
    }
  }
};
</script>

<template>
  <main class="conversation">
    <Transition name="fade" mode="out-in">
      <section v-if="!hasSelectedChat" class="conversation-container">
        <h2 class="default-info no-select">Brak wybranego czatu</h2>
      </section>

      <section v-else class="chat-container">
        <!-- chat messages -->
        <router-view v-slot="{ Component }">
          <Transition name="slide" mode="out-in">
            <component :is="Component" :key="chatKey" />
          </Transition>
        </router-view>
        <!-- message input bar -->
        <span class="floating-menus-handler">
          <span class="floating-bars">
            <!-- image previews -->
            <Transition name="menu">
              <div v-if="selectedImages.length" class="image-previews background">
                <div v-for="image in selectedImages" :key="image.id" class="image-preview">
                  <img :src="image.url" alt="Preview">
                  <button class="remove-image" @click="removeImage(image.id)">×</button>
                </div>
              </div>
            </Transition>
            <!-- context menu -->
            <Transition name="menu">
              <div v-show="isMenuOpen" class="clip-menu background">
                <div v-for="item in menuItems" :key="item.action" class="menu-item" @click="handleAction(item.action)">
                  <span>{{ item.icon }}</span>
                  <span>{{ item.text }}</span>
                </div>
              </div>
            </Transition>
          </span>
          <span class="message-input" @dragover="handleDragOver" @drop="handleDrop">
            <span class="text-edit-panel">
              <ClipIcon @click="toggleMenu" />
              <textarea v-model="message" placeholder="Napisz wiadomość" rows="1"
                @input="$event.target.style.height = ''; $event.target.style.height = $event.target.scrollHeight + 'px'"></textarea>
              <input type="file" ref="fileInput" accept="image/*" style="display: none" @change="handleFileSelect">
            </span>
            <Transition name="send-icon">
              <SendIcon v-if="message || selectedImages" @click="send" />
            </Transition>
          </span>
        </span>
      </section>
    </Transition>
  </main>
</template>

<style scoped>
.conversation {
  height: 100svh;
  position: relative;
  margin-right: 40px;
}

.chat-container {
  height: 100%;
  display: flex;
  flex-direction: column;
  justify-content: flex-end;
  gap: 15px;
}

.conversation-container {
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.default-info {
  font-size: 20px;
  font-weight: 600;
  color: var(--info-text-color);
  text-align: center;
  width: max-content;
  background-color: var(--element-dark-color);
  border: 0.5px solid var(--element-light-color);
  padding: 40px 80px;
  border-radius: 10px;
  backdrop-filter: blur(10px);
}

.text-edit-panel {
  display: flex;
  flex-direction: row;
  align-items: center;
  gap: 15px;
  width: 100%;

  textarea {
    margin: 0;
    padding: 15px 5px;
    border: none;
    background: none;
    box-shadow: none;
    width: 100%;
    resize: none;
    max-height: 150px;
    /* approximately 7 lines */
    min-height: 24px;
    overflow-y: auto;
  }

  textarea:focus {
    outline: none;
  }
}

.message-input {
  width: 100%;
  padding: 0 15px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  background-color: var(--message-bg-left);
  border: 0.5px solid var(--element-light-color);
  border-radius: 15px;
  backdrop-filter: blur(80px);
  margin-bottom: 15px;
}

.floating-menus-handler {
  position: relative;
  width: 100%;
}

.floating-bars {
  position: absolute;
  bottom: 80px;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.clip-menu {
  display: flex;
  gap: 10px;
  background-color: var(--element-dark-color);
  border: 0.5px solid var(--element-border-light-color);
  width: max-content;
  padding: 8px;
  min-width: 150px;
}

.menu-item {
  display: flex;
  align-items: center;
  gap: 5px;
  padding: 5px 8px;
  cursor: pointer;
  border-radius: 8px;
  transition: background-color 0.2s;
  background-color: var(--element-light-color);
  border: 0.5px solid var(--element-border-light-color);

  &:hover {
    background-color: var(--element-hover-light-color);
  }
}

.image-previews {
  display: flex;
  gap: 10px;
  width: max-content;
  padding: 10px;
  flex-wrap: wrap;
}

.image-preview {
  position: relative;
  width: 100px;
  height: 100px;
  border-radius: 8px;
  overflow: hidden;
  border: 1px solid var(--element-light-color);

  img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }
}

.remove-image {
  position: absolute;
  top: 5px;
  right: 5px;
  width: 20px;
  height: 20px;
  border-radius: 50%;
  background: rgba(0, 0, 0, 0.5);
  color: white;
  border: none;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  font-size: 16px;
  line-height: 1;
  padding: 0;

  &:hover {
    background: rgba(0, 0, 0, 0.7);
  }
}

/* Fade animation */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

/* Slide animation */
.slide-enter-active,
.slide-leave-active {
  transition: all 0.3s ease;
}

.slide-enter-from {
  opacity: 0;
  transform: translateX(30px);
}

.slide-leave-to {
  opacity: 0;
  transform: translateX(-30px);
}

/* Menu animation */
.menu-enter-active,
.menu-leave-active {
  transition: transform 0.3s ease, opacity 0.3s ease;
}

.menu-enter-from,
.menu-leave-to {
  transform: translateY(10px);
  opacity: 0;
}

/* Send icon animation */
.send-icon-enter-active,
.send-icon-leave-active {
  transition: all 0.2s ease;
}

.send-icon-enter-from,
.send-icon-leave-to {
  opacity: 0;
  transform: scale(0.7);
}
</style>