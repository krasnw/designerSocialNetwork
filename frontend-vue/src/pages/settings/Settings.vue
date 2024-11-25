<script>
export default {
  name: "SettingsPage",
};
</script>

<template>
  <main>
    <h2 class="page-name">Ustawienia</h2>
    <section class="settings background">
      <h3>Ustawienia aplikacji</h3>
      <article class="settings-cell">
        <p>Sidebar domyślnie jest otwarty</p>
        <label class="switch">
          <input type="checkbox">
          <span class="slider round"></span>
        </label>
      </article>
    </section>
  </main>
</template>

<style scoped>
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