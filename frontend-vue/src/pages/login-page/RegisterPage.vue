<script>
import Spinner from '@/assets/Icons/Spinner.vue';
import { authService } from '@/services/auth';

export default {
  name: "RegisterPage",
  components: {
    Spinner
  },
  data() {
    return {
      username: '',
      email: '',
      phoneNumber: '',
      firstName: '',
      lastName: '',
      password: '',
      passwordRepeat: '',
      errors: {
        username: null,
        email: null,
        firstName: null,
        lastName: null,
        password: null,
        passwordRepeat: null,
        phoneNumber: null
      },
      serverError: null,
      isLoading: false,
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
        phoneNumber: () => {
          if (this.phoneNumber.trim() === '') return 'Numer telefonu jest wymagany';
          // example of phone number validation +48123123123
          if (!/^\+\d{1,3}\d{9}$/.test(this.phoneNumber)) return 'Nieprawidłowy format numeru telefonu';
          return null;
        },
        password: () => {
          if (this.password.trim() === '') return 'Hasło jest wymagane';
          if (this.password.length < 6) return 'Hasło musi mieć min. 8 znaków';
          // password must have number
          if (!/\d/.test(this.password)) return 'Hasło musi zawierać cyfrę';
          // password must have uppercase letter
          if (!/[A-Z]/.test(this.password)) return 'Hasło musi zawierać wielką literę';
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
      ['username', 'email', 'firstName', 'lastName', 'password', 'passwordRepeat', 'phoneNumber'].forEach(field => {
        this.validateField(field);
      });
      return !Object.values(this.errors).some(error => error !== null);
    },

    async handleRegister(e) {
      e.preventDefault();
      if (!this.validateAll()) return;

      try {
        this.isLoading = true;
        await authService.register({
          username: this.username,
          email: this.email,
          firstName: this.firstName,
          lastName: this.lastName,
          password: this.password,
          phoneNumber: this.phoneNumber
        });

        window.dispatchEvent(new Event('loginStatusChanged'));
        this.$router.push({ name: 'feed' });
      } catch (error) {
        this.serverError = error.message;
      } finally {
        this.isLoading = false;
      }
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
      <form class="form background" autocomplete="off" @submit="handleRegister">
        <div class="register-form first-column">
          <h3>Dane logowania</h3>
          <div class="form-group">
            <input autocomplete="off" type="text" v-model="username" :class="{ 'error': errors.username }"
              placeholder="Nazwa użytkownika" @blur="validateField('username')" />
            <span class="error-message" v-if="errors.username">{{ errors.username }}</span>
          </div>

          <div class="form-group">
            <input autocomplete="off" type="email" v-model="email" :class="{ 'error': errors.email }"
              placeholder="Email" @blur="validateField('email')" />
            <span class="error-message" v-if="errors.email">{{ errors.email }}</span>
          </div>

          <div class="form-group">
            <input autocomplete="new-password" type="password" v-model="password" :class="{ 'error': errors.password }"
              placeholder="Hasło" @blur="validateField('password')" />
            <span class="error-message" v-if="errors.password">{{ errors.password }}</span>
          </div>

          <div class="form-group">
            <input autocomplete="new-password" type="password" v-model="passwordRepeat"
              :class="{ 'error': errors.passwordRepeat }" placeholder="Powtórz hasło"
              @blur="validateField('passwordRepeat')" />
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
            <input type="text" v-model="lastName" :class="{ 'error': errors.lastName }" placeholder="Nazwisko"
              @blur="validateField('lastName')" />
            <span class="error-message" v-if="errors.lastName">{{ errors.lastName }}</span>
          </div>
          <div class="form-group">
            <input autocomplete="off" type="text" v-model="phoneNumber" :class="{ 'error': errors.phoneNumber }"
              placeholder="Numer telefonu (+48...)" @blur="validateField('phoneNumber')" />
            <span class="error-message" v-if="errors.phoneNumber">{{ errors.phoneNumber }}</span>
          </div>
          <button type="submit" class="button">
            Zarejestruj się
            <Spinner v-if="isLoading" class="spinner" />
          </button>
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
  background-color: var(--element-light-color);
  font-weight: 600;
  font-size: 14px;
  color: var(--primery-text-color);
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

.register-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
  width: max-content;
  padding: 20px;
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
}
</style>