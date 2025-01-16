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
        this.error = "Treść zlecenia nie może być pusta";
        return false;
      }
      return true;
    },
    async sendTask() {
      try {
        this.error = null;
        if (!this.validateForm()) return;

        await userService.sendRequest(
          this.$route.params.username,
          this.description
        );
        this.returnToPortfolio();
      } catch (error) {
        console.error("Error sending task:", error);
        this.error = "Przepraszamy, funkcja jest tymczasowo niedostępna";
      }
    }
  }
};

</script>

<template>
  <main>
    <h2 class="page-name">Złożenie zlecenia</h2>
    <article class="background task">
      <textarea class="reply-textarea" placeholder="Napisz treść zlecenia" v-model="description"></textarea>
      <p v-if="error" class="error-message">{{ error }}</p>
      <span class="control-buttons">
        <button @click="returnToPortfolio" class="accept-button button">Powróć</button>
        <button @click="sendTask" class="accept-button button">Wyslij</button>
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