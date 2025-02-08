<script>
import NavigationBar from './components/NavigationBar.vue';
import SideBar from './components/Sidebar/SideBar.vue';

export default {
  name: "App",
  components: {
    NavigationBar,
    SideBar
  },
  data() {
    return {
      isSidebarHidden: JSON.parse(localStorage.getItem('sidebarHidden') || 'false'),
      routerViewKey: 0 // добавляем новое свойство
    }
  },
  methods: {
    showSidebar(routeName) {
      const pagesWithoutSidebar = ['add-post', ''];
      return !pagesWithoutSidebar.includes(routeName);
    },
    handleSidebarState(hidden) {
      this.isSidebarHidden = hidden;
    },
    reloadView() {
      this.routerViewKey += 1;
    }
  },
  computed: {
    mainContentStyle() {
      const sidebarWidth = this.isSidebarHidden ? '0px' : '340px';
      return {
        width: `calc(100% - ${this.showSidebar(this.currentRoute) ? sidebarWidth : '0px'})`,
        paddingLeft: '100px',
      }
    },
    currentRoute() {
      return this.$route.name
    }
  }
};
</script>

<template>
  <NavigationBar />
  <section class="main-content" :style="mainContentStyle">
    <router-view v-slot="{ Component }" :key="routerViewKey">
      <transition name="move" mode="out-in">
        <component :is="Component" />
      </transition>
    </router-view>
  </section>
  <SideBar v-if="showSidebar(currentRoute)" :page="currentRoute" @sidebar-state-changed="handleSidebarState" />
</template>



<style>
/* Basic styling */
:root {
  --font: 'Montserrat';
  --secondary-font: 'Inter';
  --bank-card-font: 'Michroma', monospace;
  --element-dark-color: rgba(30, 30, 30, 0.4);
  --element-dark-hover-color: rgba(30, 30, 30, 0.6);
  --element-light-color: rgba(255, 255, 255, 0.15);
  --element-hover-light-color: rgba(255, 255, 255, 0.2);
  --element-border-light-color: rgba(255, 255, 255, 0.30);
  --delete-button-color: rgba(255, 57, 57, 0.40);
  --delete-button-border-color: rgba(255, 94, 94, 0.60);
  --success-color: rgba(128, 255, 128, 0.7);
  --info-text-color: rgba(255, 255, 255, 0.7);
  --placeholder-color: rgba(255, 255, 255, 0.6);
  --placeholder-error-color: rgba(255, 170, 170, 0.8);
  --shadow-color: rgba(0, 0, 0, 0.25);
  --text-color: rgba(255, 255, 255, 0.7);
  --primery-text-color: rgba(255, 255, 255, 0.85);
  --message-bg-left: rgba(60, 60, 60, 0.4);
  --message-bg-right: rgba(90, 90, 90, 0.4);
}

html {
  box-sizing: border-box;
  overscroll-behavior: none;
}

*,
*:before,
*:after {
  box-sizing: inherit;
  color: white;
}

body {
  margin: 0;
  padding: 0;
  background-image: url('assets/Images/main_bg5.png');
  background-size: cover;
  background-position: center;
  background-attachment: fixed;
  height: 100vh;
  font-family: var(--font);
  overscroll-behavior: none;
}

hr {
  margin: 0;
  border: none;
  background-color: var(--element-light-color);
  border-radius: 1px;
}

a {
  text-decoration: none;
  background: linear-gradient(120deg, #4f6fbb 0%, #71cadc 100%);
  -webkit-background-clip: text;
  background-clip: text;
  -webkit-text-fill-color: transparent;
}

a:hover {
  text-decoration: underline;
  cursor: pointer;
}


h1,
h2,
h3,
h4,
h5,
h6 {
  color: var(--primery-text-color);
  margin: 0;
}

p {
  font-family: var(--secondary-font);
  color: var(--text-color);
  margin: 0;
}

.page-name {
  font-weight: 700;
  font-size: 30px;
  margin-top: 50px;
  margin-bottom: 30px;
  -webkit-user-select: none;
  -ms-user-select: none;
  user-select: none;
}

.background {
  background-color: var(--element-dark-color);
  backdrop-filter: blur(60px);
  border: 0.5px solid var(--element-border-light-color);
  border-radius: 15px;
}

.button {
  border: 0.5px solid;
  border-radius: 12px;
  padding: 10px 15px;
  width: 100px;
  color: rgba(255, 255, 255, 0.8);
  font-weight: 700;
  font-size: 14px;
  backdrop-filter: blur(40px);
  box-shadow: 5px 5px 25px 0px rgba(0, 0, 0, 0.25);
  cursor: pointer;
  width: fit-content;
  display: flex;
  flex-wrap: nowrap;
  align-items: center;
  justify-content: center;
  gap: 10px;
  -webkit-user-select: none;
  -ms-user-select: none;
  user-select: none;
  transition: all 0.3s;
}

.button:hover {
  transform: scale(1.03);
  transition: all 0.3s;
}

input,
textarea {
  resize: none;
  border: 0.5px solid var(--element-border-light-color);
  border-radius: 10px;
  background-color: var(--element-light-color);
  padding: 19px 23px;
  box-shadow: 5px 5px 25px var(--shadow-color);
  font-weight: 700;
  font-family: var(--font);
  font-size: clamp(9px, 1.5vw, 14px);
}

input::placeholder,
textarea::placeholder {
  color: var(--placeholder-color);
  /* Цвет текста placeholder */
}

input:focus,
textarea:focus {
  outline: 1.5px solid #999;
  background-color: var(--element-light-color);
}

input:hover,
textarea:hover {
  background-color: var(--element-hover-light-color);
}

select {
  padding: 10px;
  border: 0.5px solid var(--element-border-light-color);
  border-radius: 10px;
  background-color: var(--element-light-color);
  color: white;
  font-family: var(--font);
}

.spinner {
  animation: spin 2s linear infinite;
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }

  100% {
    transform: rotate(360deg);
  }
}

.no-select {
  -webkit-user-select: none;
  -ms-user-select: none;
  user-select: none;
}

.green-gradient {
  background: linear-gradient(120deg, #4fbb94 0%, #709fbc 100%);
  -webkit-background-clip: text;
  background-clip: text;
  -webkit-text-fill-color: transparent;
}

.red-gradient {
  background: linear-gradient(120deg, #ec4389 0%, #eba83d 100%);
  -webkit-background-clip: text;
  background-clip: text;
  -webkit-text-fill-color: transparent;
}

.blue-gradient {
  background: linear-gradient(120deg, #8d74df 0%, #5aaee3 100%);
  -webkit-background-clip: text;
  background-clip: text;
  -webkit-text-fill-color: transparent;
}

.delete-button {
  background: var(--delete-button-color);
  border-color: var(--delete-button-border-color);
}

.accept-button {
  background: var(--element-light-color);
  border-color: var(--element-border-light-color);
}

.error-wrap {
  padding: 20px;
  width: fit-content;
  max-width: 400px;
}

/* Page styling */
.main-content {
  z-index: 0;
  max-height: 100svh;
  overflow-y: auto;
  overflow-x: hidden;
  transition: width 0.3s;

  /* Стилизация скроллбара */
  &::-webkit-scrollbar {
    width: 8px;
  }

  &::-webkit-scrollbar-track {
    background: #fff1;
    border-radius: 4px;
    backdrop-filter: blur(5px);
  }

  &::-webkit-scrollbar-thumb {
    background: var(--element-border-light-color);
    border-radius: 4px;
  }
}

.post-like {
  display: flex;
  align-items: center;
}

/* Animations */
.move-enter-active,
.move-leave-active {
  transition: all 0.3s ease;
  /* position: absolute; */
}

.move-enter-from {
  opacity: 0;
  transform: translateX(-100%);
}

.move-leave-to {
  opacity: 0;
  /* margin-left: -100%; */
  transform: translateY(-100%);
}

/* global animations */
@keyframes scaleAnimation {
  0% {
    transform: scale(1);
  }

  50% {
    transform: scale(0.9);
  }

  100% {
    transform: scale(1);
  }
}
</style>
