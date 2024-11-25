<script>
export default {
  name: "RegisterPage",
  data() {
    return {
      username: '',
      email: '',
      firstName: '',
      middleName: '',
      lastName: '',
      password: '',
      passwordRepeat: '',
      errors: {
        username: null,
        email: null,
        firstName: null,
        middleName: null,
        lastName: null,
        password: null,
        passwordRepeat: null
      },
      serverError: null
    };
  },
  methods: {
    validateField(field) {
      const validations = {
        username: () => {
          if (this.username.trim() === '') return 'Nazwa użytkownika jest wymagana';
          if (this.username.length < 3) return 'Nazwa użytkownika musi mieć min. 3 znaki';
          return null;
        },
        email: () => {
          if (this.email.trim() === '') return 'Email jest wymagany';
          if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.email)) return 'Nieprawidłowy format email';
          return null;
        },
        firstName: () => {
          if (this.firstName.trim() === '') return 'Imię jest wymagane';
          return null;
        },
        lastName: () => {
          if (this.lastName.trim() === '') return 'Nazwisko jest wymagane';
          return null;
        },
        password: () => {
          if (this.password.trim() === '') return 'Hasło jest wymagane';
          if (this.password.length < 6) return 'Hasło musi mieć min. 6 znaków';
          return null;
        },
        passwordRepeat: () => {
          if (this.passwordRepeat !== this.password) return 'Hasła nie są zgodne';
          return null;
        }
      };

      if (field in validations) {
        this.errors[field] = validations[field]();
      }
    },

    validateAll() {
      ['username', 'email', 'firstName', 'lastName', 'password', 'passwordRepeat'].forEach(field => {
        this.validateField(field);
      });
      return !Object.values(this.errors).some(error => error !== null);
    },

    handleRegister(e) {
      e.preventDefault();
      if (!this.validateAll()) return;

      // Здесь будет логика отправки данных на сервер
      // Пример обработки серверной ошибки:
      // if (serverResponse.error) {
      //   if (serverResponse.error.field) {
      //     this.errors[serverResponse.error.field] = serverResponse.error.message;
      //   } else {
      //     this.serverError = serverResponse.error.message;
      //   }
      // }
    },

    clearError(field) {
      if (field) {
        this.errors[field] = null;
      } else {
        this.serverError = null;
      }
    }
  }
};
</script>

<template>
  <main>
    <h2 class="page-name">Rejestracja</h2>
    <section>
      <form class="form background" @submit="handleRegister">
        <div class="register-form first-column">
          <h3>Dane logowania</h3>
          <div class="form-group">
            <input type="text" v-model="username" :class="{ 'error': errors.username }" placeholder="Nazwa użytkownika"
              @blur="validateField('username')" />
            <span class="error-message" v-if="errors.username">{{ errors.username }}</span>
          </div>

          <div class="form-group">
            <input type="email" v-model="email" :class="{ 'error': errors.email }" placeholder="Email"
              @blur="validateField('email')" />
            <span class="error-message" v-if="errors.email">{{ errors.email }}</span>
          </div>

          <div class="form-group">
            <input type="password" v-model="password" :class="{ 'error': errors.password }" placeholder="Hasło"
              @blur="validateField('password')" />
            <span class="error-message" v-if="errors.password">{{ errors.password }}</span>
          </div>

          <div class="form-group">
            <input type="password" v-model="passwordRepeat" :class="{ 'error': errors.passwordRepeat }"
              placeholder="Powtórz hasło" @blur="validateField('passwordRepeat')" />
            <span class="error-message" v-if="errors.passwordRepeat">{{ errors.passwordRepeat }}</span>
          </div>
        </div>

        <div class="register-form second-column">
          <h3>Dane o użytkowniku</h3>
          <div class="form-group">
            <input type="text" v-model="firstName" :class="{ 'error': errors.firstName }" placeholder="Imię"
              @blur="validateField('firstName')" />
            <span class="error-message" v-if="errors.firstName">{{ errors.firstName }}</span>
          </div>

          <div class="form-group">
            <input type="text" v-model="middleName" placeholder="Drugie imię (opcjonalnie)" />
          </div>

          <div class="form-group">
            <input type="text" v-model="lastName" :class="{ 'error': errors.lastName }" placeholder="Nazwisko"
              @blur="validateField('lastName')" />
            <span class="error-message" v-if="errors.lastName">{{ errors.lastName }}</span>
          </div>
          <button type="submit" class="button">Zarejestruj się</button>
        </div>
      </form>

      <transition name="slide-fade">
        <article class="error-garbage" v-if="serverError">
          <h3>Ups... coś poszło nie tak</h3>
          <p class="background">{{ serverError }}</p>
          <button @click="clearError()" id="deleteErr">Zamknij</button>
        </article>
      </transition>
    </section>
  </main>
</template>

<style scoped>
.form {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
  width: max-content;
}

.form-group input,
.form-group button {
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
  color: rgba(255, 170, 170, 0.8);
  /* Цвет текста placeholder */
}

.register-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
  width: max-content;
  padding: 20px;
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
}

.form-group {
  position: relative;
  width: 100%;
  margin-bottom: 5px;
}

.error-message {
  color: #f88;
  font-size: 12px;
  position: absolute;
  bottom: -18px;
  left: 0;
}

.input.error {
  border-color: #f88;
  background-color: rgba(255, 0, 0, 0.1);
}
</style>