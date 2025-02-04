<script>
import Message from './Message.vue';
import { chatMessagesService } from '@/services/chat';

export default {
  name: "Chat",
  components: {
    Message
  },
  data() {
    return {
      messages: [],
      username: ''
    };
  },
  async mounted() {
    this.username = this.$route.params.username;

    try {
      this.messages = await chatMessagesService.getConversationMessages(this.username);
      this.scrollToBottom();
    } catch (error) {
      console.error('Failed to load messages:', error);
    }
  },
  async created() {
    await chatMessagesService.connect();

  },
  beforeDestroy() {
    if (this.unsubscribe) {
      this.unsubscribe();
    }
    chatMessagesService.disconnect();
  },
  methods: {
    scrollToBottom() {
      const messagesContainer = this.$el.querySelector('.messages');
      if (messagesContainer) {
        messagesContainer.scrollTop = messagesContainer.scrollHeight;
      }
    },
    addToMessages(message) {
      this.messages.push(message);
    }
  }
};
</script>

<template>
  <main class="chat-container">
    <section class="messages" ref="messages">
      <Message v-for="message in messages" :key="message.id" :message="message" />
    </section>
  </main>
</template>

<style scoped>
.chat-container {
  height: 100%;
  display: flex;
  flex-direction: column;
  min-height: 0;
}

.messages {
  flex: 1;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 10px;
  min-height: 0;
  padding-right: 10px;
}

.messages::-webkit-scrollbar {
  width: 8px;
}

.messages::-webkit-scrollbar-track {
  background: transparent;
}

.messages::-webkit-scrollbar-thumb {
  background-color: transparent;
  border-radius: 3px;
  transition: background-color 1s;
}

.messages:hover::-webkit-scrollbar-thumb {
  background-color: var(--element-light-color);
  border: 1px solid var(--element-hover-light-color);
  border-radius: 2px;
  transition: background-color 1s;
}
</style>