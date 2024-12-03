<script>
export default {
  name: "SettingsPage",
  data() {
    return {
      allowArrow: localStorage.getItem('allowArrow') === 'true',
      allowWheel: localStorage.getItem('allowWheel') === 'true',
      sidebarAlwaysOpen: localStorage.getItem('sidebarAlwaysOpen') === 'true',
    };
  },
  methods: {
    logout() {
      localStorage.removeItem('JWT');
      window.dispatchEvent(new Event('loginStatusChanged'));
      this.$router.push({ name: 'login' });
    },
    isLoggedIn() {
      return localStorage.getItem('JWT') !== null;
    },
    toggleArrow() {
      localStorage.setItem('allowArrow', String(this.allowArrow));
    },
    toggleWheel() {
      localStorage.setItem('allowWheel', String(this.allowWheel));
    },
    toggleSidebar() {
      localStorage.setItem('sidebarAlwaysOpen', String(this.sidebarAlwaysOpen));
      window.dispatchEvent(new Event('sidebarSettingChanged'));
    },
  },
  mounted() {
    // Меняем значение по умолчанию на false
    if (localStorage.getItem('allowArrow') === null) {
      localStorage.setItem('allowArrow', 'false');
    }
    if (localStorage.getItem('allowWheel') === null) {
      localStorage.setItem('allowWheel', 'false');
    }
    if (localStorage.getItem('sidebarAlwaysOpen') === null) {
      localStorage.setItem('sidebarAlwaysOpen', 'false');
    }
    this.isLoggedIn();
  },
};
</script>

<template>
  <main>
    <h2 class="page-name">Ustawienia</h2>
    <section class="wrapper">
      <section class="settings background">
        <h3>Ustawienia widoku</h3>
        <article class="settings-cell">
          <p>Sidebar domyślnie jest widoczny</p>
          <label class="switch">
            <input type="checkbox" v-model="sidebarAlwaysOpen" @change="toggleSidebar">
            <span class="slider round"></span>
          </label>
        </article>

        <article class="settings-cell">
          <p>Włączyć strzałki dla przeglądu zdjęć</p>
          <label class="switch">
            <input type="checkbox" v-model="allowArrow" @change="toggleArrow">
            <span class="slider round"></span>
          </label>
        </article>

        <article class="settings-cell">
          <p>Przełączenie pomiędzy zdjęciami za pomocą kółka myszy</p>
          <label class="switch">
            <input type="checkbox" v-model="allowWheel" @change="toggleWheel">
            <span class="slider round"></span>
          </label>
        </article>
      </section>

      <section class="settings background" v-if="isLoggedIn()">
        <h3>Ustawienia konta</h3>
        <article class="settings-cell">
          <p>Wylogowanie z aktywnego konta</p>
          <button class="button" @click="logout">Wyloguj się</button>
        </article>
      </section>
      <section class="settings background" v-else>
        <h3>Ustawienia konta</h3>
        <article class="settings-cell">
          <p>Nie jesteś zalogowany, zaloguj się aby dokonać ustawień swojego konta</p>
        </article>
      </section>
    </section>
  </main>
</template>

<style scoped>
.wrapper {
  display: flex;
  flex-direction: column;
  gap: 30px;
}

.button {
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

.settings {
  padding: 30px 40px;
  max-width: 925px;
  width: 80%;
  min-width: 550px;
}

.settings-cell {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 20px;
}

/* toggle */
.switch {
  position: relative;
  display: inline-block;
  width: 30px;
  height: 17px;
}

/* Hide default HTML checkbox */
.switch input {
  opacity: 0;
  width: 0;
  height: 0;
}

/* The slider */
.slider {
  position: absolute;
  cursor: pointer;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(255, 255, 255, 0.20);
  -webkit-transition: .4s;
  transition: .4s;
}

.slider:before {
  position: absolute;
  content: "";
  height: 13px;
  width: 13px;
  left: 2px;
  bottom: 2px;
  background-color: white;
  -webkit-transition: .4s;
  transition: .4s;
}

input:checked+.slider {
  background-color: #2196F3;
}

input:focus+.slider {
  box-shadow: 0 0 1px #2196F3;
}

input:checked+.slider:before {
  -webkit-transform: translateX(13px);
  -ms-transform: translateX(13px);
  transform: translateX(13px);
}

/* Rounded sliders */
.slider.round {
  border-radius: 17px;
}

.slider.round:before {
  border-radius: 50%;
}
</style>