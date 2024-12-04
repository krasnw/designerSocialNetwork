<script>
import { userService } from '@/services/user';

export default {
  name: "ProfileInfo",
  data() {
    return {
      isLoggedIn: localStorage.getItem('JWT') !== null,
      user: {
        name: '',
        image: 'https://placehold.co/600x600',
      }
    }
  },
  methods: {
    checkLoginStatus() {
      this.isLoggedIn = localStorage.getItem('JWT') !== null;
      // Принудительно вызываем обновление компонента
      this.$forceUpdate();
    },
    goToLogin() {
      this.$router.push({ name: 'login' });
    },
    goToRegister() {
      this.$router.push({ name: 'register' });
    },
    goToProfile() {
      this.$router.push({ name: 'myProfile' });
    },
    async loadUserData() {
      if (this.isLoggedIn) {
        try {
          const userData = await userService.getMyData();
          this.user.name = `${userData.firstName} ${userData.lastName}`;
          if (userData.profilePicture) {
            this.user.image = userData.profilePicture;
          }
        } catch (error) {
          console.error('Ошибка при загрузке данных пользователя:', error);
        }
      }
    }
  },
  async mounted() {
    // Добавляем слушатель для нового события
    window.addEventListener('loginStatusChanged', this.checkLoginStatus);
    window.addEventListener('storage', this.checkLoginStatus);
    await this.loadUserData();
  },
  watch: {
    isLoggedIn(newValue) {
      if (newValue) {
        this.loadUserData();
      }
    }
  },
  beforeDestroy() {
    // Удаляем оба слушателя
    window.removeEventListener('loginStatusChanged', this.checkLoginStatus);
    window.removeEventListener('storage', this.checkLoginStatus);
  }
};
</script>

<template>
  <span class="wrapper">

    <article class="sidebar-profile" v-if="isLoggedIn">
      <div class="username-wrapper" @click="goToProfile">
        <h3 class="username">{{ user.name }}</h3>
      </div>
      <img :src="user.image" alt="Profile picture" />
    </article>
    <article class="sidebar-profile" v-else>
      <button @click="goToLogin">Zaloguj się</button>
      <button @click="goToRegister">Stwórz konto</button>
    </article>

  </span>
</template>

<style scoped>
.wrapper {
  padding: 20px 20px;
  background-color: var(--element-light-color);
  border-bottom: 0.5px solid var(--element-border-light-color);
}

.sidebar-profile {
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: space-between;
  gap: 20px;
}

.sidebar-profile button {
  background-color: var(--element-light-color);
  border-radius: 7px;
  border: 0.5px solid white;
  padding: 12px 25px;
  color: var(--primery-text-color);
  font-size: 16px;
  font-weight: 700;
}

.sidebar-profile button:hover {
  background-color: var(--element-hover-light-color);
  cursor: pointer;
}

.username-wrapper {
  background-color: var(--element-light-color);
  border-radius: 7px;
  border: 0.5px solid white;
  padding: 12px 50px;
  cursor: pointer;
}

.username {
  color: var(--primery-text-color);
  font-size: 16px;
  font-weight: 700;
}


.sidebar-profile img {
  border: 0.5px solid white;
  background-color: var(--element-border-light-color);
  padding: 2px;
  width: 50px;
  height: 50px;
  border-radius: 50%;
  object-fit: cover;
}
</style>
