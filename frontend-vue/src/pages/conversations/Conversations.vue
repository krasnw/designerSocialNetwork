<script>
import ClipIcon from '@/assets/Icons/ClipIcon.vue';
import SendIcon from '@/assets/Icons/SendIcon.vue';

export default {
  name: "ConversationsPage",
  components: {
    SendIcon,
    ClipIcon
  }
};
</script>

<template>
  <main class="conversation">
    <h2 class="default-info no-select">Brak wybranego czatu</h2>
    <span class="message-input">
      <span class="text-edit-panel">
        <ClipIcon />
        <input type="text" placeholder="Twoja wiadomość" />
      </span>
      <SendIcon />
    </span>
  </main>
</template>

<style scoped>
.conversation {
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  height: 100svh;
  position: relative;
  margin-right: 40px;
}

.default-info {
  font-size: 20px;
  font-weight: 600;
  color: var(--info-text-color);
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

  input {
    margin: 0;
    padding: 15px 5px;
    border: none;
    background: none;
    box-shadow: none;
    width: 100%;
  }

  input:focus {
    outline: none;
  }
}

.message-input {
  position: absolute;
  bottom: 15px;
  left: 0;
  width: 100%;
  padding: 0 15px;
  display: flex;
  align-items: center;
  justify-content: space-between;

  background-color: var(--message-bg-left);
  border: 0.5px solid var(--element-light-color);
  border-radius: 15px;
  backdrop-filter: blur(80px);
}
</style>