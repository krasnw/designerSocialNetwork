<script>
export default {
  name: "LoginPage",
  data() {
    return {
      username: '',
      password: '',
      error: null,
      usernameError: false,
      passwordError: false
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
    handleLogin(e) {
      e.preventDefault();
      this.validateField('username');
      this.validateField('password');

      if (this.username.trim() === '' || this.password.trim() === '') {
        this.error = 'Wszystkie pola są wymagane.';
        return;
      }

      // Здесь логика отправки данных на сервер
      this.error = null;
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
        <button type="submit" class="button">Zaloguj się</button>
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
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
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
  background-color: rgba(255, 255, 255, 0.15);
  font-weight: 600;
  font-size: 14px;
  color: white;
}

.button:hover {
  background-color: rgba(255, 255, 255, 0.3);
  cursor: pointer;
}

.error-garbage {
  margin-top: 20px;
  display: flex;
  flex-direction: column;
  gap: 10px;

  h3 {
    font-size: 22px;
    text-shadow: 1px 1px 2px rgba(0, 0, 0, 1);
  }

  p {
    padding: 20px;
    font-size: 16px;
    color: rgba(255, 255, 255, 0.7);
    width: max-content;
    max-width: 500px;
  }

  #deleteErr {
    padding: 10px 20px;
    border-radius: 10px;
    border: 0.5px solid rgba(255, 255, 255, 0.3);
    background-color: rgba(255, 255, 255, 0.15);
    backdrop-filter: blur(10px);
    font-weight: 600;
    font-size: 14px;
    width: max-content;
    color: white;
  }

  #deleteErr:hover {
    background-color: rgba(255, 255, 255, 0.3);
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
  color: rgba(255, 170, 170, 0.5);
  /* Цвет текста placeholder */
}
</style>