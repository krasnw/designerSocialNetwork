<script>
import Spinner from '@/assets/Icons/Spinner.vue';
import { authService } from '@/services/auth';


export default {
  name: "LoginPage",
  components: {
    Spinner
  },
  data() {
    return {
      username: '',
      password: '',
      error: null,
      usernameError: false,
      passwordError: false,
      isLoading: false
    };
  },
  methods: {
    validateField(field) {
      if (field === 'username') {
        this.usernameError = this.username.trim() === '';
      } else if (field === 'password') {
        this.passwordError = this.password.trim() === '';
      }
    },
    async handleLogin(e) {
      e.preventDefault();
      this.validateField('username');
      this.validateField('password');

      if (this.username.trim() === '' || this.password.trim() === '') {
        this.error = 'Wszystkie pola są wymagane.';
        return;
      }

      try {
        this.isLoading = true;
        await authService.login({
          username: this.username,
          password: this.password
        });

        window.dispatchEvent(new Event('loginStatusChanged'));
        this.$router.push({ name: 'feed' });
      } catch (error) {
        this.error = error.message;
      } finally {
        this.isLoading = false;
      }
    },
    deleteError() {
      this.error = null;
    }
  }
};
</script>

<template>
  <main>
    <h2 class="page-name">Zaloguj się</h2>
    <section>
      <form class="login-form background" @submit="handleLogin">
        <input type="text" v-model="username" class="input" :class="{ 'error': usernameError }" placeholder="Email"
          @blur="validateField('username')" />
        <input type="password" v-model="password" class="input" :class="{ 'error': passwordError }" placeholder="Hasło"
          @blur="validateField('password')" />
        <button type="submit" class="button">Zaloguj się
          <Spinner v-if="isLoading" class="spinner" />
        </button>
      </form>
      <transition name="slide-fade">
        <article class="error-garbage" v-show="error">
          <h3>Ups... coś poszło nie tak</h3>
          <p class="background">{{ error }}</p>
          <button @click="deleteError" id="deleteErr">Zamknij</button>
        </article>
      </transition>
    </section>
  </main>
</template>

<style scoped>
.login-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
  width: max-content;
  padding: 20px;
  box-shadow: 0 4px 8px var(--shadow-color);
}

.login-form input,
.login-form button {
  margin: 0;
  width: 100%;
  padding: 10px;
  border-radius: 5px;
  font-size: 16px;
}

.button {
  padding: 10px 20px;
  border-radius: 10px;
  background-color: var(--element-light-color);
  font-weight: 600;
  font-size: 14px;
  color: var(--primery-text-color);
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 10px;
}

.button:hover {
  background-color: var(--element-border-light-color);
  cursor: pointer;
}

.error-garbage {
  margin-top: 20px;
  display: flex;
  flex-direction: column;
  gap: 10px;

  h3 {
    font-size: 22px;
    text-shadow: 1px 1px 2px var(--shadow-color);
  }

  p {
    padding: 20px;
    font-size: 16px;
    color: var(--info-text-color);
    width: max-content;
    max-width: 500px;
  }

  #deleteErr {
    padding: 10px 20px;
    border-radius: 10px;
    border: 0.5px solid var(--element-border-light-color);
    background-color: var(--element-light-color);
    backdrop-filter: blur(10px);
    font-weight: 600;
    font-size: 14px;
    width: max-content;
    color: var(--primery-text-color);
  }

  #deleteErr:hover {
    background-color: var(--element-border-light-color);
    cursor: pointer;
  }
}

.slide-fade-enter-active,
.slide-fade-leave-active {
  transition: all 0.3s ease;
}

.slide-fade-enter-from,
.slide-fade-leave-to {
  transform: translateX(-20px);
  opacity: 0;
}

.input.error {
  border: 1px solid #f88;
}

.input.error::placeholder {
  color: var(--placeholder-error-color);
}
</style>