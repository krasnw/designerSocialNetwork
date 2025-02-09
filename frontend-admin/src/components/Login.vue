<script>
import authService from '../services/auth';

export default {
  name: 'Login',
  data() {
    return {
      username: '',
      password: '',
      error: '',
      fieldErrors: {
        username: false,
        password: false
      }
    }
  },
  methods: {
    async login() {
      // Reset errors
      this.error = '';
      this.fieldErrors.username = false;
      this.fieldErrors.password = false;

      // Validate fields
      if (!this.username.trim()) {
        this.fieldErrors.username = true;
      }
      if (!this.password.trim()) {
        this.fieldErrors.password = true;
      }

      // If any field is empty, stop login process
      if (!this.username.trim() || !this.password.trim()) {
        return;
      }

      const formData = new FormData();
      formData.append('username', this.username);
      formData.append('password', this.password);

      try {
        await authService.login(formData);
        this.$router.push('/reports/users');
      } catch (error) {
        this.error = error;
      }
    },
    clearError(field) {
      this.fieldErrors[field] = false;
    }
  }
};
</script>

<template>
  <div class="flex relative items-center justify-center min-h-screen error-anchor">
    <div class="w-full max-w-md p-8 space-y-6 bg-neutral-900 rounded-lg shadow-md">
      <h1 class="text-2xl font-bold text-center text-gray-500">Logowanie do systemu</h1>

      <form class="space-y-4">
        <div>
          <label class="block text-sm font-medium text-gray-700">Imię użytkownika</label>
          <input type="text" class="w-full px-3 py-2 mt-1 border rounded-md focus:outline-none"
            :class="{ 'border-red-500': fieldErrors.username }" placeholder="Wprowadź imię" v-model="username"
            @focus="clearError('username')">
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700">Hasło</label>
          <input type="password" class="w-full px-3 py-2 mt-1 border rounded-md focus:outline-none"
            :class="{ 'border-red-500': fieldErrors.password }" placeholder="Wprowadź hasło" v-model="password"
            @focus="clearError('password')">
        </div>

        <button @click="login" type="button"
          class="w-full px-4 py-2 text-white bg-blue-500 rounded-md hover:bg-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-500">
          Zaloguj się
        </button>
      </form>
    </div>
    <div class="error-container absolute top-8 flex gap-4" v-if="error">
      <div
        class="error-message bg-neutral-700 px-4 py-2 rounded-md border border-red-500 max-w-md flex justify-center items-center">
        <p class="text-red-500 truncate">{{ error }}</p>
      </div>
      <button class="reject-button" @click="error = ''">Schowaj</button>
    </div>
  </div>
</template>

<style scoped>
input:focus {
  border-color: #4f6fbb;
}
</style>