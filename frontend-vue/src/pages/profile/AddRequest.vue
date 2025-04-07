<script>
import { userService } from '@/services/user';

export default {
  data() {
    return {
      description: "",
      error: null
    };
  },
  methods: {
    returnToPortfolio() {
      this.$router.push("/" + this.$route.params.username + "/portfolio");
    },
    validateForm() {
      if (!this.description.trim()) {
        this.error = "The task description cannot be empty";
        return false;
      }
      return true;
    },
    async sendTask() {
      try {
        this.error = null;
        if (!this.validateForm()) return;

        const formData = new FormData();
        formData.append("description", this.description);
        formData.append("receiver", this.$route.params.username);

        await userService.sendRequest(formData);
        this.returnToPortfolio();
      } catch (error) {
        this.$router.push(`/error/${error.status}`);
      }
    }
  }
};

</script>

<template>
  <main>
    <h2 class="page-name">Submit a Task</h2>
    <article class="background task">
      <textarea class="reply-textarea" placeholder="Write the task description" v-model="description"></textarea>
      <p v-if="error" class="error-message">{{ error }}</p>
      <span class="control-buttons">
        <button @click="returnToPortfolio" class="accept-button button">Return</button>
        <button @click="sendTask" class="accept-button button">Send</button>
      </span>
    </article>
  </main>
</template>

<style scoped>
.task {
  padding: 20px;
  max-width: 925px;
  width: 80%;
  min-width: 550px;
  transition: all 0.3s ease;
  overflow: hidden;
  border-radius: 30px;
}

.reply-textarea {
  margin: 0;
  width: 100%;
  height: 150px;
  transition: all 0.3s ease;
}

.control-buttons {
  margin-top: 10px;
  display: flex;
  justify-content: space-between;
  gap: 20px;
}

.accept-button {
  background: var(--element-light-color);
  border-color: var(--element-border-light-color);
  transition: all 0.3s ease;
}

.accept-button:hover {
  background: var(--element-hover-light-color);
  transition: all 0.3s ease;
}

.error-message {
  color: red;
  margin-top: 10px;
  text-align: center;
}
</style>