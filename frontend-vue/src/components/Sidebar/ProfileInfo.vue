<script>
export default {
  name: "ProfileInfo",
  data() {
    return {
      isLoggedIn: localStorage.getItem('JWT') !== null,
      user: {
        name: 'Paweł Topski',
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
    }
  },
  mounted() {
    // Добавляем слушатель для нового события
    window.addEventListener('loginStatusChanged', this.checkLoginStatus);
    window.addEventListener('storage', this.checkLoginStatus);
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
      <div class="username-wrapper">
        <p class="username">{{ user.name }}</p>
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
  background-color: rgba(255, 255, 255, 0.1);
  border-bottom: 0.5px solid rgb(255 255 255 / 0.3);
}

.sidebar-profile {
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: space-between;
  gap: 20px;
}

.sidebar-profile button {
  background-color: rgba(255, 255, 255, 0.15);
  border-radius: 7px;
  border: 0.5px solid white;
  padding: 12px 25px;
  color: white;
  font-size: 16px;
  font-weight: 700;
}

.sidebar-profile button:hover {
  background-color: rgba(255, 255, 255, 0.3);
  cursor: pointer;
}

.username-wrapper {
  background-color: rgba(255, 255, 255, 0.15);
  border-radius: 7px;
  border: 0.5px solid white;
  padding: 12px 50px;
}

.username {
  color: white;
  font-size: 16px;
  font-weight: 700;
}


.sidebar-profile img {
  border: 0.5px solid white;
  background-color: rgba(255, 255, 255, 0.3);
  padding: 2px;
  width: 50px;
  height: 50px;
  border-radius: 50%;
  object-fit: cover;
}
</style>
