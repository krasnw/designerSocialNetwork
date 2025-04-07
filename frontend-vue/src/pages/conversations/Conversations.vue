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
        { icon: '💰', text: 'Zaproś opłatę', action: 'requestPayment' },
        { icon: '🚫', text: 'Skończ zlecenie', action: 'endChat' }
      ],
      amount: 0,
      paymentDescription: '',
      isPaymentModalOpen: false,
      isEndChatModalOpen: false
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
    async send() {
      if (this.$route.query.isDisabled) {
        return;
      }
      if (this.message || this.selectedImages.length) {
        const formData = new FormData();
        const cleanUpMessage = this.message.trim();
        formData.append('ReceiverUsername', this.$route.params.username);
        formData.append('TextContent', cleanUpMessage);
        this.selectedImages.forEach(image => {
          formData.append('Images', image.file);
        });

        await chatMessagesService.sendMessage(formData).catch(error => {
          this.$router.push(`/error/${error.status}`);
        }).finally(() => {
          this.message = '';
          this.selectedImages = [];
          const textarea = this.$el.querySelector('textarea');
          if (textarea) {
            textarea.style.height = 'auto';
          }
        });
      }
    },
    handleEnterPress(event) {
      if (event.shiftKey) {
        // Allow new line with Shift+Enter
        return;
      }
      // Prevent new line and send message
      event.preventDefault();
      this.send();
    },
    adjustTextareaHeight(event) {
      const textarea = event.target;
      textarea.style.height = 'auto';
      textarea.style.height = textarea.scrollHeight + 'px';
    },
    toggleMenu() {
      this.isMenuOpen = !this.isMenuOpen;
    },
    handleAction(action) {
      switch (action) {
        case 'addImage':
          this.$refs.fileInput.click();
          break;
        case 'requestPayment':
          this.isPaymentModalOpen = true;
          break;
        case 'endChat':
          this.isEndChatModalOpen = true;
          break;
      }
      this.isMenuOpen = false;
    },
    async handleSendPayment() {
      if (this.$route.query.isDisabled) {
        return;
      }
      if (!this.amount || !this.paymentDescription) {
        return;
      }

      const formData = new FormData();
      formData.append('receiverUsername', this.$route.params.username);
      formData.append('amount', this.amount);
      formData.append('description', this.paymentDescription);

      await chatMessagesService.sendTransactionMessage(formData).catch(error => {
        this.$router.push(`/error/${error.status}`);
      }).finally(() => {
        this.amount = 0;
        this.paymentDescription = '';
        this.isPaymentModalOpen = false;
      });
    },
    async handleEndChat() {
      try {
        if (this.$route.query.isDisabled) {
          return;
        }
        await chatMessagesService.sendEndRequestMessage(this.$route.params.username);
        this.isEndChatModalOpen = false;
      } catch (error) {
        this.$router.push(`/error/${error.status}`);
      }
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
        <h2 class="default-info no-select">No chat selected</h2>
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
              <ClipIcon @click="toggleMenu" class="icon" :class="{ 'rotated': isMenuOpen }"
                v-if="!this.$route.query.isDisabled" />
              <textarea v-model="message" placeholder="Napisz wiadomość" rows="1"
                @keydown.enter="handleEnterPress($event)" @input="adjustTextareaHeight($event)"></textarea>
              <input type="file" ref="fileInput" accept="image/*" style="display: none" @change="handleFileSelect">
            </span>
            <Transition name="send-icon">
              <SendIcon v-if="(message.length > 0 || selectedImages.length > 0) && !this.$route.query.isDisabled"
                @click="send" />
            </Transition>
          </span>
        </span>
      </section>
    </Transition><!-- Payment Modal -->
    <Transition name="modal">
      <div v-if="isPaymentModalOpen" class="modal-overlay">
        <div class="modal background">
          <h3>Zaproś opłatę</h3>
          <div class="modal-content">
            <div class="input-group">
              <input type="number" v-model="amount" placeholder="Kwota" min="0" step="0.01" required>
            </div>
            <div class="input-group">
              <input type="text" v-model="paymentDescription" placeholder="Opis płatności" required>
            </div>
          </div>
          <div class="modal-actions">
            <button @click="isPaymentModalOpen = false" class="cancel-button">
              Anuluj
            </button>
            <button @click="handleSendPayment" class="submit-button" :disabled="!amount || !paymentDescription">
              Wyślij
            </button>
          </div>
        </div>
      </div>
    </Transition>
    <!-- End Chat Confirmation Modal -->
    <Transition name="modal">
      <div v-if="isEndChatModalOpen" class="modal-overlay">
        <div class="modal background">
          <h3>Potwierdź zakończenie</h3>
          <div class="modal-content">
            <p>Czy na pewno chcesz zakończyć to zlecenie?</p>
            <p class="warning-text">Ta akcja jest nieodwracalna.</p>
          </div>
          <div class="modal-actions">
            <button @click="isEndChatModalOpen = false" class="cancel-button">
              Anuluj
            </button>
            <button @click="handleEndChat" class="submit-button danger">
              Zakończ
            </button>
          </div>
        </div>
      </div>
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
  bottom: 100%;
  display: flex;
  flex-direction: column;
  gap: 10px;
  padding-bottom: 10px;
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

.icon {
  cursor: pointer;
  transition: transform 0.3s;
}

.rotated {
  transform: rotate(45deg);
  transition: transform 0.3s;
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

/* Modal styles */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal {
  padding: 20px;
  border-radius: 15px;
  min-width: 300px;
  border: 0.5px solid var(--element-border-light-color);

  h3 {
    margin-bottom: 20px;
    text-align: center;
  }
}

.modal-content {
  display: flex;
  flex-direction: column;
  gap: 15px;
  margin-bottom: 20px;
}

.input-group input {
  width: 100%;
  padding: 10px;
  border-radius: 8px;
  border: 1px solid var(--element-border-light-color);
  background-color: var(--element-light-color);
  color: var(--text-color);

  &:focus {
    outline: none;
    border-color: var(--accent-color);
  }
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

.submit-button,
.cancel-button {
  border: 0.5px solid var(--element-light-border-color);
  padding: 8px 16px;
  border-radius: 8px;
  border: none;
  cursor: pointer;
  transition: background-color 0.2s;
}

.submit-button {
  background-color: var(--element-light-color);
  color: white;

  &:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  &:not(:disabled):hover {
    background-color: var(--element-hover-light--color);
  }
}

.cancel-button {
  background-color: var(--element-light-color);


  &:hover {
    background-color: var(--element-hover-light-color);
  }
}

/* Modal animation */
.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.3s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.warning-text {
  color: var(--delete-button-border-color);
  font-size: 14px;
  margin-top: 8px;
}

.submit-button.danger {
  background-color: var(--delete-button-border-color);
}
</style>