<script>
import SideBarArrow from '@/assets/Icons/SideBarArrow.vue';
import ProfileInfo from './ProfileInfo.vue';
import Filter from './Filter.vue';
import Search from './Search.vue';


export default {
  name: "SideBar",
  components: {
    SideBarArrow,
    ProfileInfo,
    Filter,
    Search
  },
  props: {
    page: {
      type: String,
      required: true
    }
  },
  data() {
    return {
      hidden: JSON.parse(localStorage.getItem('sidebarHidden')) || false,
      rotated: JSON.parse(localStorage.getItem('sidebarRotated')) || true,
    };
  },
  watch: {
    hidden(newValue) {
      localStorage.setItem('sidebarHidden', JSON.stringify(newValue));
    },
    rotated(newValue) {
      localStorage.setItem('sidebarRotated', JSON.stringify(newValue));
    },
  },
  methods: {
    toggleSidebar() {
      this.hidden = !this.hidden;
      this.rotated = !this.rotated;
    },
  },
};
</script>

<template>
  <aside :style="{ width: hidden ? '10px' : '350px' }" class="sidebar-wrapper">

    <button class="arrow-button" @click="toggleSidebar">
      <span :class="{ rotated: rotated }">
        <SideBarArrow />
      </span>
    </button>


    <section v-if="!hidden" class="sidebar">
      <ProfileInfo />

      <!-- cases for other pages -->
      <div v-if="page === 'feed'" class="wrap">
        <Search />
        <Filter />
      </div>
      <div v-else-if="page === 'profile'">
        <h2>Profile</h2>
      </div>
      <div v-else-if="page === 'add-post'">
        <h2>Ranking</h2>
      </div>
    </section>
  </aside>
</template>

<style scoped>
.wrap {
  margin-top: 20px;
  padding: 0px 20px;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.sidebar-wrapper {
  position: fixed;
  top: 0;
  right: 0;
  backdrop-filter: blur(64px);
  height: 100svh;
  background-color: rgb(47 47 47 / 0.4);
  border-left: 0.5px solid rgb(255 255 255 / 0.15);
  transition: width 0.3s;
}

.arrow-button {
  position: absolute;
  top: 104px;
  left: -25px;
  width: 25px;
  height: 30px;
  background-color: rgb(47 47 47 / 0.4);
  backdrop-filter: blur(64px);
  border-top: 0.5px solid rgb(255 255 255 / 0.15);
  border-left: 0.5px solid rgb(255 255 255 / 0.15);
  border-bottom: 0.5px solid rgb(255 255 255 / 0.15);
  border-right: none;
  color: white;
  border-radius: 20px 0px 0px 20px;
  cursor: pointer;
}

.arrow-button:hover {
  background-color: rgb(47 47 47 / 0.6);

  span {
    transform: rotate(-20deg);
  }
}

.arrow-button span {
  display: inline-block;
  transition: transform 0.5s;
  display: flex;
  justify-content: center;
  align-items: center;
}

.arrow-button span.rotated {
  transform: rotate(-180deg);
}

.sidebar {
  display: flex;
  flex-direction: column;
  height: 100svh;
  width: 100%;
  background-image: url('/src/assets/Images/noise.png');
  max-height: 100svh;
  /* Добавляем максимальную высоту */
  overflow-y: auto;
  /* Добавляем вертикальную прокрутку */

  /* Стилизация скроллбара */
  &::-webkit-scrollbar {
    width: 8px;
  }

  &::-webkit-scrollbar-track {
    background: #0000;
    border-left: 0.5px solid rgb(255 255 255 / 0.15);
  }

  &::-webkit-scrollbar-thumb {
    background: rgba(255, 255, 255, 0.15);
    border-radius: 2px;
  }

  &::-webkit-scrollbar-thumb:hover {
    background: rgba(255, 255, 255, 0.2);
  }
}
</style>