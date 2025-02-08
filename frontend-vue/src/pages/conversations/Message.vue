<script>
import RubyIcon from '@/assets/Icons/RubyIcon.vue';
import { imageDirectory } from '@/services/constants';
import { chatService } from '@/services/chat';

export default {
  name: "Message",
  components: {
    RubyIcon
  },
  props: {
    message: {
      type: Object,
      required: true
    }
  },
  data() {
    return {
      selectedImage: null,
      showModal: false,
      messageTypes: {
        REGULAR: 0,
        PAYMENT_REQUEST: 1,
        PAYMENT_APPROVED: 2,
        END_REQUEST: 3,
        END_APPROVED: 4
      }
    }
  },
  computed: {
    isOutgoing() {
      return this.message.receiverUsername == this.$route.params.username;
    },
    isRegularMessage() {
      return this.message.type === this.messageTypes.REGULAR;
    },
    isPaymentRequest() {
      return this.message.type === this.messageTypes.PAYMENT_REQUEST;
    },
    isPaymentApproved() {
      return this.message.type === this.messageTypes.PAYMENT_APPROVED;
    },
    isEndRequest() {
      return this.message.type === this.messageTypes.END_REQUEST;
    },
    isEndApproved() {
      return this.message.type === this.messageTypes.END_APPROVED;
    },
    messageSize() {
      if (!this.isRegularMessage) return 'payment';
      const textLength = this.message.textContent.split(' ').length ?? 1;
      return this.message.imagePaths.length === 0
        ? (textLength > 30 ? 'large' : textLength > 15 ? 'medium' : 'small')
        : 'medium';
    },
    messageClass() {
      if (!this.isRegularMessage) return { 'payment-message': true };
      return {
        [this.messageSize]: true,
        'incoming': !this.isOutgoing,
        'outgoing': this.isOutgoing
      }
    },
    showPayButton() {
      return this.isPaymentRequest &&
        !this.message.isApproved &&
        this.message.receiverUsername !== this.$route.params.username;
    },
    showEndButton() {
      return this.isEndRequest &&
        !this.message.isApproved &&
        this.message.receiverUsername !== this.$route.params.username;
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
    },
    formattedText(text) {
      return String(text || '')
        .replace(/\r?\n/g, '<br>');
    },
    async handlePayment() {
      if (this.$route.query.isDisabled) {
        return;
      }
      await chatService.approveTransaction(this.message.transactionHash)
        .catch((err) => {
          this.$router.push(`/error/${err.status}`);
        });

      this.message.isApproved = true;
    },
    async handleEndApproval() {
      if (this.$route.query.isDisabled) {
        return;
      }
      await chatService.approveEndRequest(this.message.endRequestHash)
        .catch((err) => {
          this.$router.push(`/error/${err.status}`);
        });

      this.message.isApproved = true;
    },
    formattedAmount(amount) {
      return `${amount.toFixed(2)}`;
    },
    formattedDate(dateString) {
      const date = new Date(dateString);
      return new Intl.DateTimeFormat('pl-PL', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      }).format(date);
    }
  }
};
</script>

<template>
  <article class="message-container" :class="{ 'center-message': !isRegularMessage }">
    <!-- Regular message -->
    <div v-if="isRegularMessage" class="message background" :class="messageClass">
      <span class="images-container" v-if="this.message.imagePaths?.length > 0">
        <img v-for="imagePath in this.message.imagePaths" :key="imagePath" :src="imagePathHandler(imagePath)"
          @click="openImage(imagePath)" alt="Obrazek" />
      </span>
      <p class="message-text" v-html="formattedText(message.textContent)"></p>
    </div>

    <!-- Payment request message -->
    <div v-else-if="isPaymentRequest" class="message payment-message background flex-column">
      <h3>Prośba o płatność</h3>
      <span class="amount-container">
        <p class="amount">{{ formattedAmount(message.amount) }}</p>
        <RubyIcon />
      </span>
      <p class="description">{{ message.description }}</p>
      <div v-if="message.isApproved" class="status approved">
        Opłacone
      </div>
      <div v-else class="status pending">
        <template v-if="showPayButton">
          <button @click="handlePayment" class="pay-button">Opłać</button>
        </template>
        <template v-else>
          Oczekuje na płatność
        </template>
      </div>
    </div>

    <!-- Payment approved message -->
    <div v-else-if="isPaymentApproved" class="message payment-message background flex-column">
      <div class="status approved">
        <p>Płatność zatwierdzona przez {{ message.approvedBy }}</p>
        <p class="timestamp">{{ formattedDate(message.approvedAt) }}</p>
      </div>
    </div>

    <!-- End request message -->
    <div v-else-if="isEndRequest" class="message payment-message background flex-column">
      <h3>Prośba o zakończenie zlecenia</h3>
      <div v-if="message.isApproved" class="status approved">
        Zakończone
      </div>
      <div v-else class="status pending">
        <template v-if="showEndButton">
          <button @click="handleEndApproval" class="pay-button">Potwierdź zakończenie</button>
        </template>
        <template v-else>
          Oczekuje na potwierdzenie
        </template>
      </div>
    </div>

    <!-- End approved message -->
    <div v-else-if="isEndApproved" class="message payment-message background flex-column">
      <div class="status approved">
        <p>Zlecenie zakończone przez {{ message.approvedBy }}</p>
        <p class="timestamp">{{ formattedDate(message.approvedAt) }}</p>
      </div>
    </div>

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

.center-message {
  justify-content: center !important;
}

.payment-message {
  text-align: center;
  padding: 15px;
  border-radius: 15px;
  background-color: var(--element-dark-color) !important;
  max-width: 300px;
}

.flex-column {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.amount-container {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 10px;
}

.payment-message .amount {
  font-size: 1.5em;
  font-weight: bold;
  margin: 0;
}

.payment-message .description {
  color: var(--text-color);
  margin: 0;
}

.status {
  padding: 5px;
  border-radius: 5px;
}

.status.pending {
  color: var(--text-color);
  font-weight: 600;
}

.status.approved {
  color: var(--success-color);
  font-weight: 600;
}

.pay-button {
  background-color: var(--element-light-color);
  border: 0.5px solid var(--element-border-light-color);
  padding: 8px 16px;
  color: var(--text-color);
  font-weight: 600;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
}

.pay-button:hover {
  background-color: var(--element-hover-light-color);
  transition: all 0.2s;
}

.pay-button:active {
  transform: scale(0.98);
  transition: all 0.2s;
}

.timestamp {
  font-size: 0.8em;
  color: var(--text-color);
  margin-top: 5px;
}
</style>