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
    this.unsubscribe = chatMessagesService.onMessage(async (messagePromise) => {
      try {
        console.log("Message received:", messagePromise);
        const message = await messagePromise;
        this.addToMessages(message);
        this.$nextTick(() => {
          this.scrollToBottom();
        });
      } catch (error) {
        console.error('Error processing message:', error);
      }
    });
  },
  beforeDestroy() {
    if (this.unsubscribe) {
      this.unsubscribe();
    }
    chatMessagesService.disconnect();
  },
  computed: {
    filteredMessages() {
      const type3Count = this.messages.filter(msg => msg.type === 3).length;
      const type4Count = this.messages.filter(msg => msg.type === 4).length;

      return this.messages.filter(msg =>
        msg.type !== 3 || (msg.type === 3 && type3Count > type4Count)
      );
    }
  },
  methods: {
    scrollToBottom() {
      const messagesContainer = this.$el.querySelector('.messages');
      if (messagesContainer) {
        messagesContainer.scrollTop = messagesContainer.scrollHeight;
      }
    },
    transformMessageKeys(message) {
      return Object.keys(message).reduce((acc, key) => {
        const newKey = key.charAt(0).toLowerCase() + key.slice(1);
        acc[newKey] = message[key];
        return acc;
      }, {});
    },
    addToMessages(message) {
      const transformedMessage = this.transformMessageKeys(message);
      console.log('Adding message:', transformedMessage);
      this.messages.push(transformedMessage);
    }
  }
};
</script>

<template>
  <main class="chat-container">
    <section class="messages" ref="messages">
      <Message v-for="message in filteredMessages" :key="message.id" :message="message" />
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
  justify-content: flex-end;
  gap: 10px;
  min-height: 0;
  padding: 0 10px 10px 15px;
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