<script>
import NavigationBar from './components/NavigationBar.vue';
import SideBar from './components/SideBar.vue';

export default {
  name: "App",
  components: {
    NavigationBar,
    SideBar
  },
};
</script>

<template>
  <NavigationBar />
  <section class="main-content">
    <router-view v-slot="{ Component }">
      <transition name="move" mode="out-in">
        <component :is="Component" />
      </transition>
    </router-view>
  </section>
</template>

<style>
/* Basic styling */
:root {
  --font: 'Montserrat';
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
  background-image: url('assets/Images/main_bg.jpg');
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
  background-color: rgb(255 255 255 / 0.15);
  border-radius: 1px;
}

a {
  text-decoration: none;
  color: #09f;
}

a:hover {
  text-decoration: underline;
}

h1,
h2,
h3,
h4,
h5,
h6 {
  margin: 0;
}

.page-name {
  font-weight: 700;
  font-size: 30px;
  color: rgba(255, 255, 255, 0.9);
  margin-top: 50px;
  margin-bottom: 30px;
}

.background {
  background-color: rgba(30, 30, 30, 0.4);
  backdrop-filter: blur(60px);
  border: 0.5px solid rgba(255, 255, 255, 0.30);
  border-radius: 15px;
}

.button {
  border: 0.5px solid;
  border-radius: 10px;
  padding: 10px 30px;
  color: white;
  font-weight: 700;
  font-size: 17px;
  backdrop-filter: blur(40px);
  box-shadow: 5px 5px 25px 0px rgba(0, 0, 0, 0.25);
  cursor: pointer;
}

input,
textarea {
  resize: none;
  border: 0.5px solid rgba(255, 255, 255, 0.30);
  border-radius: 10px;
  background-color: rgba(255, 255, 255, 0.12);
  padding: 19px 23px;
  margin-top: 23px;
  box-shadow: 5px 5px 25px rgba(0, 0, 0, 0.25);
  font-weight: 800;
  font-family: var(--font);
  font-size: clamp(12px, 1.5vw, 18px);
}

input::placeholder,
textarea::placeholder {
  color: rgba(255, 255, 255, 0.5);
  /* Цвет текста placeholder */
}

input:focus,
textarea:focus {
  outline: 1.5px solid #999;
  background-color: rgba(255, 255, 255, 0.15);
}

input:hover,
textarea:hover {
  background-color: rgba(255, 255, 255, 0.20);
}



/* Page styling */
.main-content {
  margin-left: 100px;
  z-index: 0;
}

/* Animations */
.move-enter-active,
.move-leave-active {
  transition: all 0.3s ease;
  position: absolute;
}

.move-enter-from {
  opacity: 0;
  transform: translateX(-100%);
}

.move-leave-to {
  opacity: 0;
  transform: translateY(-100%);
}
</style>
