<script setup>
import { RouterView, RouterLink, useRouter } from 'vue-router'
import { computed, onMounted } from 'vue'
import authService from './services/auth';

const router = useRouter()
const isUserLogged = computed(() => authService.authState.value)

onMounted(() => {
  authService.isLogged();
})

const logout = () => {
  authService.logout();
  router.push('/login');
}
</script>

<template>
  <main>
    <nav class="navbar">
      <div class="nav-links">
        <RouterLink class="nav-link" to="/reports/users">Użytkownicy</RouterLink>
        <RouterLink class="nav-link" to="/reports/posts">Publikacje</RouterLink>
      </div>
      <RouterLink class="nav-link" to="/login" v-if="!isUserLogged">Login</RouterLink>
      <h4 class="nav-link cursor-pointer hover:opacity-90" @click="logout" v-else>
        <span class="red-gradient">Logout</span>
      </h4>
    </nav>
    <RouterView />
  </main>
</template>

<style scoped></style>
